using UnityEngine;

public class SlowMudZone : MonoBehaviour
{
    private static SlowMudZone activeZone = null;

    [Header("沼に乗っている時の目標前進スピード (この速度まで落ちる)")]
    public float slowForwardSpeed = 10.0f;

    [Header("沼に乗った時の減速スピード (値を大きくするほど一瞬でガクッと落ちる)")]
    public float decelerationSpeed = 120.0f;

    [Header("減速した状態を維持する時間 (秒) (※沼の上から降りた後はすぐ加速します)")]
    public float slowDuration = 2.0f;

    [Header("元の速度に戻る時の加速スピード (値を小さくするほどノロノロと戻る)")]
    public float accelerationSpeed = 8.0f;

    private float originalSpeedCache = 15f;
    private float currentTargetSlowSpeed;
    private PlayerController playerController;

    private enum MudState
    {
        Normal,
        SlowingDown, // 沼に乗って減速中
        StayingSlow, // 沼の上でノロノロ維持中
        Recovering   // 沼から降りて、その瞬間の速度から元の速度へじわじわ加速中
    }
    private MudState currentState = MudState.Normal;
    private float stateTimer = 0f;
    private bool isPlayerDirectlyOn = false;

    void Start()
    {
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj == null) playerObj = GameObject.Find("Player(Clone)");
        if (playerObj != null)
        {
            playerController = playerObj.GetComponent<PlayerController>();
        }
    }

    void Update()
    {
        if (playerController == null) return;

        // 🚨【高速すり抜け・通り過ぎ対策セーフティ】
        if (currentState == MudState.SlowingDown || currentState == MudState.StayingSlow)
        {
            float distanceZ = playerController.transform.position.z - transform.position.z;
            if (distanceZ > 15.0f || transform.position.z < -15.0f)
            {
                if (isPlayerDirectlyOn)
                {
                    OnPlayerStepOff();
                }
            }
        }

        // 💡【競合対策】自分がアクティブでない場合は、速度操作を一切行わない
        if (activeZone != null && activeZone != this && currentState != MudState.Normal)
        {
            currentState = MudState.Normal;
            return;
        }

        switch (currentState)
        {
            case MudState.Normal:
                break;

            case MudState.SlowingDown:
                // 🐌 沼に乗っている間は目標速度に向かって減速させる
                playerController.forwardSpeed = Mathf.MoveTowards(
                    playerController.forwardSpeed,
                    currentTargetSlowSpeed,
                    decelerationSpeed * Time.deltaTime
                );

                if (playerController.forwardSpeed <= currentTargetSlowSpeed ||
                    Mathf.Approximately(playerController.forwardSpeed, currentTargetSlowSpeed))
                {
                    playerController.forwardSpeed = currentTargetSlowSpeed;
                    currentState = MudState.StayingSlow;
                    stateTimer = 0f;
                    Debug.Log($"🐌 沼：最大減速（{currentTargetSlowSpeed}）に到達。");
                }
                break;

            case MudState.StayingSlow:
                // 🛑 沼の上にいる間は低速を絶対固定
                playerController.forwardSpeed = currentTargetSlowSpeed;

                // プレイヤーがまだ直接沼の上に立っている場合は、タイマーを進めず低速を維持
                if (isPlayerDirectlyOn)
                {
                    stateTimer = 0f;
                    return;
                }

                // 沼から降りた後、初めてこの維持タイマーがカウントスタート
                stateTimer += Time.deltaTime;
                if (stateTimer >= slowDuration)
                {
                    currentState = MudState.Recovering;
                    Debug.Log($"🕒 沼：脱出後の維持時間が経過。現在の低速から目標 {originalSpeedCache} へ加速を開始！");
                }
                break;

            case MudState.Recovering:
                // 📈 【超重要】減速が途中で終わっていても、その現在の速度から、元のスピードに向かって超ノロノロ加速する
                playerController.forwardSpeed = Mathf.MoveTowards(
                    playerController.forwardSpeed,
                    originalSpeedCache,
                    accelerationSpeed * Time.deltaTime
                );

                if (playerController.forwardSpeed >= originalSpeedCache ||
                    Mathf.Approximately(playerController.forwardSpeed, originalSpeedCache))
                {
                    playerController.forwardSpeed = originalSpeedCache;
                    currentState = MudState.Normal;

                    if (playerController != null)
                    {
                        playerController.isSpeedControlledByMud = false;
                    }

                    if (activeZone == this) activeZone = null;
                    Debug.Log("🏁 沼：元の速度に完全に復帰しました！");
                }
                break;
        }
    }

    // 🚀 プレイヤーが踏んだとき（減速開始）
    public void OnPlayerStepOn()
    {
        if (playerController == null) return;

        isPlayerDirectlyOn = true;
        playerController.isSpeedControlledByMud = true;

        // すでに別の沼を踏んで減速中だった場合、元の通常スピードの記憶を引き継ぐ
        if (activeZone != null && activeZone != this)
        {
            this.originalSpeedCache = activeZone.originalSpeedCache;
            activeZone.currentState = MudState.Normal;
        }
        else if (currentState == MudState.Normal)
        {
            originalSpeedCache = playerController.forwardSpeed;
        }

        activeZone = this;
        currentTargetSlowSpeed = slowForwardSpeed;

        if (originalSpeedCache <= slowForwardSpeed)
        {
            currentTargetSlowSpeed = originalSpeedCache * 0.5f;
            Debug.Log($"⚠️ 元々遅いため、沼目標速度を {currentTargetSlowSpeed} に自動調整しました");
        }

        currentState = MudState.SlowingDown;
        Debug.Log("🐌 沼：新しく踏んだ沼で減速を開始しました！");
    }

    // 🚀 プレイヤーが降りたとき（減速解除）
    public void OnPlayerStepOff()
    {
        if (playerController == null) return;

        isPlayerDirectlyOn = false;

        // 💡【ここを修正！】
        // プレイヤーが沼から降りた瞬間（または通り過ぎた瞬間）は、
        // 「目標まで落ちるのを待つ」必要がなくなったので、減速中（SlowingDown）であっても、
        // 即座に「その瞬間の速度からじわじわ加速（Recovering）」へ移行させます！
        if (currentState == MudState.SlowingDown || currentState == MudState.StayingSlow)
        {
            currentState = MudState.Recovering;
            Debug.Log($"📈 沼：脱出を検知。現在の速度 {playerController.forwardSpeed} から、そのままスムーズに加速復帰を開始します！");
        }
    }

    public void ForceResetToNormal()
    {
        currentState = MudState.Normal;
        if (playerController != null)
        {
            playerController.isSpeedControlledByMud = false;
        }
    }

    void OnDestroy()
    {
        if (activeZone == this)
        {
            activeZone = null;

            if (playerController != null && currentState == MudState.Recovering)
            {
                playerController.isSpeedControlledByMud = false;
                Debug.Log("⚠️ 加速途中で沼オブジェクトが消滅。急ワープを防止し、そのままの速度からプレイヤーの自己加速に委ねました。");
            }
            else if (currentState != MudState.Normal && playerController != null)
            {
                playerController.forwardSpeed = originalSpeedCache;
                playerController.isSpeedControlledByMud = false;
            }
        }
    }
}