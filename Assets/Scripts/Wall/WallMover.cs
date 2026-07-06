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

    // 床の流れるスピードをPlayerController等から取得するための変数
    [Header("床が手前に流れるベース速度（プレイヤースピード）")]
    private float currentScrollSpeed = 0f;

    [Header("ゲームオーバー画面のUIパネル")]
    public GameObject gameOverPanel;

    private PlayerController playerController;
    private int penaltyLevel = 0; // 0:安全, 1:警告, 2:即死
    private bool isPenalizing = false;
    private Coroutine penaltyCoroutine;

    void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerController = player.GetComponent<PlayerController>();

            // 💡【貫通防止】生まれたその瞬間に、プレイヤーの背後に壁を強制移動
            float startZ = 0f - targetDistance;
            transform.position = new Vector3(transform.position.x, transform.position.y, startZ);
        }

        // 💡【自動検索の復活】プレファブから生成されても、シーン上のGameOverPanelを自動で見つける
        if (gameOverPanel == null)
        {
            GameObject canvas = GameObject.Find("Canvas");
            if (canvas != null)
            {
                // Canvas の中から「GameOverPanel」という名前の子オブジェクトを非表示でも見つける
                Transform panelTransform = canvas.transform.Find("GameOverPanel");
                if (panelTransform != null)
                {
                    gameOverPanel = panelTransform.gameObject;
                }
            }
        }

        // 💡 見つかったら、念のためゲーム開始時はパネルを非表示にしておく
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
        else
        {
            Debug.LogWarning("⚠️ Canvasの中に 'GameOverPanel' が見つかりませんでした！名前を確認してください。");
        }
    }

    void Update()
    {
        if (Time.timeScale == 0f || playerController == null) return;

        float currentX = transform.position.x;
        float currentY = transform.position.y;
        float targetZ = transform.position.z;

        // プレイヤーの現在の前進速度（＝床が手前に流れる速度）を取得
        currentScrollSpeed = playerController.forwardSpeed;

        if (isPenalizing)
        {
            // 🔥【1回目ヒット：警告中】
            // プレイヤーの真後ろ（手前側 Z = -warningDistance）に強制固定して恐怖感を出す！
            targetZ = 0f - warningDistance;
        }
        else
        {
            // 🏃‍♂️【通常時 / 回復後】
            // 床や障害物と同じように、毎フレーム手前（Zのマイナス方向）にスクロールさせる！
            targetZ -= currentScrollSpeed * Time.deltaTime;

            // 💡 もし警告状態から復帰した直後なら、滑らかに元の遠い位置（手前側）へ戻していく処理
            if (transform.position.z < -targetDistance)
            {
                targetZ = Mathf.Lerp(transform.position.z, 0f - targetDistance, normalFollowSmooth * Time.deltaTime);
            }
        }

        // 壁の位置を更新
        transform.position = new Vector3(currentX, currentY, targetZ);
    }

    // 障害物に当たった時に呼ばれる処理
    public void HandleObstacleHit()
    {
        if (Time.timeScale == 0f) return;

        penaltyLevel++;

        if (penaltyLevel == 1)
        {
            Debug.Log("【警告】障害物に1回接触！壁が真後ろに即座に張り付いた！");

            // プレイヤーの一歩手前（背後）に一瞬でワープ
            transform.position = new Vector3(transform.position.x, transform.position.y, 0f - warningDistance);

            isPenalizing = true;

            if (penaltyCoroutine != null) StopCoroutine(penaltyCoroutine);
            penaltyCoroutine = StartCoroutine(PenaltyRecoveryRoutine());
        }
        else if (penaltyLevel >= 2)
        {
            Debug.Log("【即死】壁が真後ろにいる間もう一度衝突！ゲームオーバー！");

            transform.position = new Vector3(transform.position.x, transform.position.y, 0f);
            TriggerGameOver();
        }
    }

    private IEnumerator PenaltyRecoveryRoutine()
    {
        yield return new WaitForSeconds(penaltyDuration);

        isPenalizing = false;
        penaltyLevel = 0;
        Debug.Log("【安全】危機を脱出した。壁が離れていきます。");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && Time.timeScale > 0f)
        {
            if (penaltyLevel >= 2)
            {
                Debug.Log("【ゲームオーバー】壁に完全に追いつかれた！");
                TriggerGameOver();
            }
        }
    }

    private void TriggerGameOver()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true); // パネルを表示する
        }
        else
        {
            Debug.LogError("⚠️ GameOverPanel が見つかっていないため、画面を表示できません！");
        }
        Time.timeScale = 0f; // ゲームを一時停止
    }
}