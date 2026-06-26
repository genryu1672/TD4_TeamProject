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
        }
    }

    void Update()
    {
        if (Time.timeScale == 0f || playerController == null) return;

        float currentX = transform.position.x;
        float currentY = transform.position.y;
        float targetZ;

        // 💡 プレイヤーのZ座標が常に 0 なので、基準を「0」として計算します
        if (isPenalizing)
        {
            // 【1回目ヒット：警告中】
            // プレイヤーの真後ろ（-warningDistance）に固定
            targetZ = 0f - warningDistance;
        }
        else
        {
            // 【通常時 / 回復後】
            // 通常のキープ距離（-targetDistance）に滑らかに、または直接移動させる
            // 💡 スルスルと元の位置に戻る演出にするため、ここは Lerp を使って滑らかに戻すと見栄えが良いです！
            targetZ = Mathf.Lerp(transform.position.z, 0f - targetDistance, normalFollowSmooth * Time.deltaTime);
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

            // 💡 プレイヤーのZ=0を基準に、一瞬で背後にワープ
            transform.position = new Vector3(transform.position.x, transform.position.y, 0f - warningDistance);

            isPenalizing = true;

            if (penaltyCoroutine != null) StopCoroutine(penaltyCoroutine);
            penaltyCoroutine = StartCoroutine(PenaltyRecoveryRoutine());
        }
        else if (penaltyLevel >= 2)
        {
            Debug.Log("【即死】壁が真後ろにいる間もう一度衝突！ゲームオーバー！");

            // 💡 プレイヤーの位置（Z=0）に完全に重ねて終了
            transform.position = new Vector3(transform.position.x, transform.position.y, 0f);
            Time.timeScale = 0f;
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
                Time.timeScale = 0f;
            }
            else
            {
                Debug.Log("（壁ワープによる接触を安全にスルーしました）");
            }
        }
    }
}