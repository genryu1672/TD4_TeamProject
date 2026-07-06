using UnityEngine;

public class SlowMudZone : MonoBehaviour
{
    [Header("沼に乗っている時の前進スピード")]
    public float slowForwardSpeed = 30.0f;

    [Header("沼から出たあとの加速スムーズ度 (値が大きいほど早く元に戻る)")]
    public float accelerationSpeed = 50.0f;

    private float originalSpeedCache = 15f;
    private PlayerController playerController;
    private bool isPlayerOnMud = false;
    private bool isRecoveringSpeed = false; // 💡 加速中かどうかのフラグ

    void Start()
    {
        // シーン内のプレイヤーを安全に取得
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

        // 🐌 沼に乗っている間は速度を強制固定
        if (isPlayerOnMud)
        {
            playerController.forwardSpeed = slowForwardSpeed;
        }
        // 📈 沼から降りたあと、元の速度に向かってジワジワ加速させる
        else if (isRecoveringSpeed)
        {
            // 現在の速度から originalSpeedCache まで毎秒 accelerationSpeed ずつ近づける
            playerController.forwardSpeed = Mathf.MoveTowards(
                playerController.forwardSpeed,
                originalSpeedCache,
                accelerationSpeed * Time.deltaTime
            );

            // 元の速度に完全に達したら、復帰処理を終了する
            if (Mathf.Approximately(playerController.forwardSpeed, originalSpeedCache))
            {
                playerController.forwardSpeed = originalSpeedCache; // 念のため完全に一致させる
                isRecoveringSpeed = false;
                Debug.Log("🏁 沼：元の速度に完全に復帰しました！");
            }
        }
    }

    // 🚀 クイズ床から呼ばれる（減速開始）
    public void OnPlayerStepOn()
    {
        if (playerController == null || isPlayerOnMud) return;

        isRecoveringSpeed = false; // 加速処理が走っていたら中断

        // 現在のスピードが減速速度より速ければ、元の速度として記憶する
        // （加速途中でまた沼に入った場合は、その時の速度ではなく大元の速度を維持する）
        if (!isRecoveringSpeed && playerController.forwardSpeed > slowForwardSpeed)
        {
            originalSpeedCache = playerController.forwardSpeed;
        }

        isPlayerOnMud = true;
        Debug.Log("🐌 沼：プレイヤーが乗ったので減速固定を開始しました！");
    }

    // 🚀 クイズ床から呼ばれる（減速解除）
    public void OnPlayerStepOff()
    {
        if (playerController == null || !isPlayerOnMud) return;

        isPlayerOnMud = false;
        isRecoveringSpeed = true; // 💡 ここからジワジワ加速を開始する

        Debug.Log("📈 沼：プレイヤーが脱出したので徐々に加速します！");
    }

    // 床が消滅するときに、万が一減速・加速したままだったら瞬時に元の速度に戻す安全装置
    void OnDestroy()
    {
        if ((isPlayerOnMud || isRecoveringSpeed) && playerController != null)
        {
            playerController.forwardSpeed = originalSpeedCache;
        }
    }
}