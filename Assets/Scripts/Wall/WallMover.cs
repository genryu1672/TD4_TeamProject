using System.Collections;
using UnityEngine;

public class WallMover : MonoBehaviour
{
    [Header("通常時のプレイヤーとのキープ距離（m）")]
    public float targetDistance = 8f;

    [Header("通常時の壁の追従の滑らかさ")]
    public float normalFollowSmooth = 2f;

    [Header("1回ヒット時：プレイヤーのすぐ後ろ何メートルの位置に貼り付くか")]
    public float warningDistance = 3.5f;

    [Header("1回ヒット時：壁が近くに留まる時間（秒）")]
    public float penaltyDuration = 5.0f;

    private float currentScrollSpeed = 0f;
    private PlayerController playerController;
    private int penaltyLevel = 0;
    private bool isPenalizing = false;
    private Coroutine penaltyCoroutine;

    // 💡 管理人を呼び出すための変数
    private GameOverManager gameOverManager;

    void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) player = GameObject.Find("Player(Clone)"); // 生成クローン対策

        if (player != null)
        {
            playerController = player.GetComponent<PlayerController>();
            // 💡 初期位置も、プレイヤーの「実際のZ座標」から通常距離（8m）引いた位置にする
            float startZ = player.transform.position.z - targetDistance;
            transform.position = new Vector3(transform.position.x, transform.position.y, startZ);
        }

        gameOverManager = FindAnyObjectByType<GameOverManager>();
    }

    void Update()
    {
        if (Time.timeScale == 0f || playerController == null) return;

        currentScrollSpeed = playerController.forwardSpeed;

        float currentX = transform.position.x;
        float currentY = transform.position.y;
        float targetZ = transform.position.z;

        // 💡 リアルタイムでプレイヤーの現在のZ座標を取得
        float playerZ = playerController.transform.position.z;

        if (isPenalizing)
        {
            // 💡【修正】固定の「0」ではなく、「今のプレイヤーのZ」からwarningDistanceを引いた位置にがっちり固定！
            targetZ = playerZ - warningDistance;
        }
        else
        {
            // 💡【通常時】プレイヤーの後ろ（-targetDistance）の位置を滑らかに追いかけます
            float normalTargetZ = playerZ - targetDistance;
            targetZ = Mathf.Lerp(transform.position.z, normalTargetZ, normalFollowSmooth * Time.deltaTime);
        }

        // X座標とY座標もプレイヤーにぴったり合わせてズレを防ぎます
        transform.position = new Vector3(playerController.transform.position.x, playerController.transform.position.y, targetZ);
    }

    public void HandleObstacleHit()
    {
        if (Time.timeScale == 0f) return;

        penaltyLevel++;

        if (penaltyLevel == 1)
        {
            Debug.Log("【警告】障害物に1回接触！壁が背後に急接近します！");

            // 💡【修正】当たったその瞬間、壁を「今のプレイヤーのZ座標 - 3.5m」の位置へワープさせます！
            if (playerController != null)
            {
                float targetZ = playerController.transform.position.z - warningDistance;
                transform.position = new Vector3(playerController.transform.position.x, playerController.transform.position.y, targetZ);
            }

            isPenalizing = true;

            if (penaltyCoroutine != null) StopCoroutine(penaltyCoroutine);
            penaltyCoroutine = StartCoroutine(PenaltyRecoveryRoutine());
        }
        else if (penaltyLevel >= 2)
        {
            Debug.Log("💀 ペナルティ2回目！ゲームオーバーを呼び出します。");
            CallGameOver();
        }
    }

    private IEnumerator PenaltyRecoveryRoutine()
    {
        yield return new WaitForSeconds(penaltyDuration);
        isPenalizing = false;
        penaltyLevel = 0;
        Debug.Log("🛡️ 警告時間終了。壁が通常距離（8m）に下がります。");
    }

    // 💡 急接近した際、壁のコライダーが物理的にプレイヤーに触れてしまった時の即死を防ぐため、
    //「本当に背後から追いつかれた（距離が1m以下になった）とき」だけゲームオーバーにする
    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player") && Time.timeScale > 0f)
        {
            float distanceZ = Mathf.Abs(other.transform.position.z - transform.position.z);
            if (distanceZ <= 1.0f)
            {
                Debug.Log("💀 壁に完全に追いつかれました！");
                CallGameOver();
            }
        }
    }

    private void CallGameOver()
    {
        if (gameOverManager != null)
        {
            gameOverManager.TriggerGameOver();
        }
        else
        {
            // 💡 もしGameOverManagerがシーンになく、GameManagerが管理している場合はこちらで代用
            if (GameManager.Instance != null)
            {
                GameManager.Instance.TriggerGameOver();
            }
            else
            {
                Debug.LogError("⚠️ ゲームオーバー管理スクリプトが見つかりません。");
            }
        }
    }
}