using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuizFloorController : MonoBehaviour
{
    public static int globalBlockCount = 0;

    [Header("この床をクイズステージ（3レーン）にするか")]
    public bool isQuizStage = false;

    [Header("クイズ中の前進スピード")]
    public float slowForwardSpeed = 5.0f;

    private int trapLane;
    private GameObject[] lanes = new GameObject[3];
    private bool isQuizActive = false;
    private Transform playerTransform;
    private PlayerController playerController;

    void Awake()
    {
        globalBlockCount++;

        // 1回目のクイズは「5枚目」にすぐ出して、それ以降は「20枚ごと」に出現させる！
        if (globalBlockCount == 5 || (globalBlockCount > 5 && (globalBlockCount - 5) % 20 == 0))
        {
            isQuizStage = true;
        }
    }

    void Start()
    {
        GameObject leftFloor = null;
        GameObject centerFloor = null;
        GameObject rightFloor = null;

        // すべての子要素から、名前を基準に「左・中央・右」の床をピンポイントで探す
        foreach (Transform child in transform.GetComponentsInChildren<Transform>(true))
        {
            string nameLower = child.name.ToLower();
            if (nameLower.Contains("left")) leftFloor = child.gameObject;
            if (nameLower.Contains("right")) rightFloor = child.gameObject;

            if (child != transform && (nameLower.Contains("stageblock") || nameLower.Contains("center")))
            {
                centerFloor = child.gameObject;
            }
        }

        if (centerFloor == null) centerFloor = gameObject;

        lanes[0] = leftFloor;
        lanes[1] = centerFloor;
        lanes[2] = rightFloor;

        // 🚀 【重要：復活処理】新しい床が作られたときは、見た目(Renderer)と衝突判定(Collider)を確実にONに戻す！
        for (int i = 0; i < 3; i++)
        {
            if (lanes[i] != null)
            {
                foreach (Renderer r in lanes[i].GetComponentsInChildren<Renderer>(true)) r.enabled = true;
                foreach (Collider c in lanes[i].GetComponentsInChildren<Collider>(true)) c.enabled = true;
            }
        }

        // クイズかどうかに関わらず、左右の床は最初から常に表示する！
        if (leftFloor != null) leftFloor.SetActive(true);
        if (rightFloor != null) rightFloor.SetActive(true);

        if (isQuizStage)
        {
            BoxCollider trigger = gameObject.AddComponent<BoxCollider>();
            trigger.isTrigger = true;
            trigger.size = new Vector3(6f, 4f, 10f);
            trigger.center = new Vector3(0f, 2f, 0f);

            Debug.Log($"★3レーンクイズ床が前方にセットされました（通算{globalBlockCount}枚目）");
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (isQuizStage && !isQuizActive && (other.CompareTag("Player") || other.name.Contains("Player")))
        {
            StartQuiz(other.gameObject);
        }
    }

    public void StartQuiz(GameObject player)
    {
        isQuizActive = true;
        playerTransform = player.transform;

        trapLane = Random.Range(0, 3);
        string[] laneNames = { "左レーン", "中央レーン", "右レーン" };
        Debug.Log($"🎯 【クイズ開始】ハズレ床 ⇒ 【{laneNames[trapLane]}】");

        playerController = player.GetComponent<PlayerController>();

        StartCoroutine(QuizSequence());
    }

    private IEnumerator QuizSequence()
    {
        Debug.Log("【ネプリーグ】前進中… 3... 2... 1...");

        float timer = 0f;
        bool isRed = false;

        // 罠になるレーンに属するすべてのRendererを取得
        Renderer[] trapRenderers = lanes[trapLane] != null ? lanes[trapLane].GetComponentsInChildren<Renderer>(true) : new Renderer[0];

        // 🚀 【赤の強調表示】元の色をしっかりと保存（マテリアルカラー直接変更に対応）
        Dictionary<Renderer, Color> originalColors = new Dictionary<Renderer, Color>();
        foreach (Renderer r in trapRenderers)
        {
            if (r != null && r.material != null)
            {
                originalColors[r] = r.material.color;
            }
        }

        // 3秒間、ハズレの床を赤くチカチカ点滅させる
        while (timer < 3.0f)
        {
            isRed = !isRed;
            foreach (Renderer r in trapRenderers)
            {
                if (r != null && r.material != null)
                {
                    // 🚀 はっきりと真っ赤にするための強調処理
                    r.material.color = isRed ? Color.red : (originalColors.ContainsKey(r) ? originalColors[r] : Color.white);
                }
            }
            yield return new WaitForSeconds(0.2f);
            timer += 0.2f;
        }

        // 点滅が終わったら一度元の色に戻す
        foreach (Renderer r in trapRenderers)
        {
            if (r != null && originalColors.ContainsKey(r)) r.material.color = originalColors[r];
        }

        yield return new WaitForSeconds(0.5f);

        int playerLaneIndex = 1; // デフォルト中央
        if (playerController != null)
        {
            playerLaneIndex = playerController.GetCurrentLane();
        }

        // 💥 ハズレ床を非表示＆判定なしにする（消去）
        if (lanes[trapLane] != null)
        {
            Debug.Log($"💥 罠発動！ 【{lanes[trapLane].name}】を消去！");
            foreach (Renderer r in lanes[trapLane].GetComponentsInChildren<Renderer>(true)) r.enabled = false;
            foreach (Collider c in lanes[trapLane].GetComponentsInChildren<Collider>(true)) c.enabled = false;
        }

        yield return new WaitForSeconds(0.5f);

        // 勝敗判定
        if (playerLaneIndex == trapLane)
        {
            Debug.Log("ゲームオーバー！ハズレを踏んで落下！");
            if (playerController != null) playerController.forwardSpeed = 0f;
            Rigidbody playerRb = playerTransform.GetComponent<Rigidbody>();
            if (playerRb != null) playerRb.isKinematic = false;
        }
        else
        {
            Debug.Log("正解！セーフ！そのまま駆け抜ける！");
        }

        isQuizActive = false;
    }
}