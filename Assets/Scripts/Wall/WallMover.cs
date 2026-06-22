using System.Collections;
using UnityEngine;

public class WallMover : MonoBehaviour
{
    [Header("通常時のプレイヤーとのキープ距離（m）")]
    public float targetDistance = 8f;

    [Header("通常時の壁の追従の滑らかさ")]
    public float normalFollowSmooth = 2f;

    [Header("1回ヒット時：プレイヤーのすぐ後ろ何メートルの位置に張り付くか")]
    public float warningDistance = 3.5f; // ★安全のために初期値を3.5mに広げました

    [Header("1回ヒット時：壁が近くに留まる時間（秒）")]
    public float penaltyDuration = 5.0f;

    private PlayerController playerController;
    private int penaltyLevel = 0; // 0:安全, 1:警告（壁が真後ろ）, 2:即死
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

        float playerZ = playerController.transform.position.z;
        float currentX = transform.position.x;
        float currentY = transform.position.y;
        float targetZ;

        if (isPenalizing)
        {
            // 【1回目ヒット：警告中】
            // プレイヤーの真後ろ（warningDistance）に完全同期して張り付く
            targetZ = playerZ - warningDistance;
        }
        else
        {
            // 【通常時 / 回復後】
            // ★【ここを修正】Lerpによる手加減をやめ、プレイヤーの速度に合わせて
            // 指定した距離（targetDistance）を「完全にキープ」して並走させます！
            targetZ = playerZ - targetDistance;
        }

        // 壁の位置を完全に同期させて更新
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

            // 障害物に当たった瞬間に、一瞬でプレイヤーの後方に壁を移動（ワープ）させる
            transform.position = new Vector3(transform.position.x, transform.position.y, playerController.transform.position.z - warningDistance);

            isPenalizing = true;

            // すでに動いているタイマーがあれば一度リセットして最初から数え直す
            if (penaltyCoroutine != null) StopCoroutine(penaltyCoroutine);
            penaltyCoroutine = StartCoroutine(PenaltyRecoveryRoutine());
        }
        else if (penaltyLevel >= 2)
        {
            // ★【2回目ヒット：即死】
            Debug.Log("【即死】壁が真後ろにいる間にもう一度衝突！ゲームオーバー！");

            // 2回目なので、壁をプレイヤーの座標に完全に重ねてから時間を止めます
            transform.position = playerController.transform.position;
            Time.timeScale = 0f;
        }
    }

    private IEnumerator PenaltyRecoveryRoutine()
    {
        yield return new WaitForSeconds(penaltyDuration);

        // 指定された秒数が無傷で経過したらペナルティ解除、壁が通常位置へとスルスル離れていく
        isPenalizing = false;
        penaltyLevel = 0;
        Debug.Log("【安全】危機を脱出した。壁が離れていきます。");
    }

    // 壁のコライダー（OnTriggerEnter）によるゲームオーバー判定
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && Time.timeScale > 0f)
        {
            // ★【ここを修正】ペナルティレベルが2（2回目衝突）の時だけゲームオーバーにする安全弁
            if (penaltyLevel >= 2)
            {
                Debug.Log("【ゲームオーバー】壁に完全に追いつかれた！");
                Time.timeScale = 0f;
            }
            else
            {
                // 1回目のワープによる誤判定（暴発）だった場合は、ログを出して無視する
                Debug.Log("（壁ワープによる接触を安全にスルーしました）");
            }
        }
    }
}