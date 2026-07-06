using System.Collections;
using UnityEngine;

public class WallMover : MonoBehaviour
{
    [Header("通常時のプレイヤーとのキープ距離（m）")]
    public float targetDistance = 8f;

    [Header("通常時の壁の追従の滑らかさ")]
    public float normalFollowSmooth = 2f;

    [Header("1回ヒット時：プレイヤーのすぐ後ろ何メートルの位置に張り付くか")]
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
        if (player != null)
        {
            playerController = player.GetComponent<PlayerController>();
            float startZ = 0f - targetDistance;
            transform.position = new Vector3(transform.position.x, transform.position.y, startZ);
        }

        // 💡 こちらも FindAny に変更！
        gameOverManager = FindAnyObjectByType<GameOverManager>();
    }

    void Update()
    {
        if (Time.timeScale == 0f || playerController == null) return;

        currentScrollSpeed = playerController.forwardSpeed;

        float currentX = transform.position.x;
        float currentY = transform.position.y;
        float targetZ = transform.position.z;

        if (isPenalizing)
        {
            targetZ = 0f - warningDistance;
        }
        else
        {
            targetZ -= currentScrollSpeed * Time.deltaTime;
            if (transform.position.z < -targetDistance)
            {
                targetZ = Mathf.Lerp(transform.position.z, 0f - targetDistance, normalFollowSmooth * Time.deltaTime);
            }
        }

        transform.position = new Vector3(currentX, currentY, targetZ);
    }

    public void HandleObstacleHit()
    {
        if (Time.timeScale == 0f) return;

        penaltyLevel++;

        if (penaltyLevel == 1)
        {
            Debug.Log("【警告】障害物に1回接触！");
            transform.position = new Vector3(transform.position.x, transform.position.y, 0f - warningDistance);
            isPenalizing = true;

            if (penaltyCoroutine != null) StopCoroutine(penaltyCoroutine);
            penaltyCoroutine = StartCoroutine(PenaltyRecoveryRoutine());
        }
        else if (penaltyLevel >= 2)
        {
            CallGameOver();
        }
    }

    private IEnumerator PenaltyRecoveryRoutine()
    {
        yield return new WaitForSeconds(penaltyDuration);
        isPenalizing = false;
        penaltyLevel = 0;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && Time.timeScale > 0f)
        {
            if (penaltyLevel >= 2) CallGameOver();
        }
    }

    // 💡 管理人にゲームオーバーを要請する
    private void CallGameOver()
    {
        if (gameOverManager != null)
        {
            gameOverManager.TriggerGameOver();
        }
        else
        {
            Debug.LogError("⚠️ GameOverManagerが見つかりません！GameManagerオブジェクトにあるか確認してください。");
        }
    }
}