using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuizFloorController : MonoBehaviour
{
    [Header("この床をクイズステージ（3レーン）にするか")]
    public bool isQuizStage = false;

    [Header("クイズ中の前進スピード")]
    public float slowForwardSpeed = 5.0f;

    [Header("3つの床オブジェクトをそれぞれドラッグ＆ドロップしてください")]
    public GameObject leftFloor;
    public GameObject centerFloor;
    public GameObject rightFloor;

    private int trapLane = -1;
    private GameObject[] lanes = new GameObject[3];
    private bool isQuizActive = false;
    private bool isEvaluated = false;
    private Transform playerTransform;
    private PlayerController playerController;

    void Awake() { }

    // 🚀【やり方1 特化版】位置のリセットと初期化
    public void InitializeQuizState(bool isQuiz)
    {
        isQuizStage = isQuiz;

        // 自分の足元にある子オブジェクトを名前から確実に取得
        leftFloor = transform.Find("LeftFloor")?.gameObject;
        centerFloor = transform.Find("CenterFloor")?.gameObject;
        rightFloor = transform.Find("RightFloor")?.gameObject;

        lanes[0] = leftFloor;
        lanes[1] = centerFloor;
        lanes[2] = rightFloor;

        // 🚀【最重要】位置の強制リセット
        // 以前のクイズで奈落に落とされた床があっても、ここで元の高さ（Y = 0）に強制的に戻します！
        for (int i = 0; i < lanes.Length; i++)
        {
            if (lanes[i] != null)
            {
                // オブジェクト、見た目、コライダーは常にすべて「ON」のままにする（バグ防止）
                lanes[i].SetActive(true);

                var renderer = lanes[i].GetComponent<MeshRenderer>();
                if (renderer != null) renderer.enabled = true;

                var collider = lanes[i].GetComponent<Collider>();
                if (collider != null) collider.enabled = true;

                // 🚀 ローカル座標のYを0に戻すことで、親（StageBlock）と同じ高さに綺麗に揃います
                Vector3 localPos = lanes[i].transform.localPosition;
                localPos.y = 0f;
                lanes[i].transform.localPosition = localPos;
            }
        }

        // センサーのオンオフ
        Transform sensor = transform.Find("QuizSensor");
        if (isQuizStage)
        {
            trapLane = Random.Range(0, 3);
            isQuizActive = false;
            isEvaluated = false;

            if (sensor != null) sensor.gameObject.SetActive(true);
        }
        else
        {
            trapLane = -1;
            isQuizActive = false;
            isEvaluated = false;

            if (sensor != null) sensor.gameObject.SetActive(false);
        }
    }

    void Start()
    {
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj == null) playerObj = GameObject.Find("Player(Clone)");
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
            playerController = playerObj.GetComponent<PlayerController>();
        }
    }

    public void StartQuiz(GameObject player)
    {
        if (isQuizStage == false) return;
        if (trapLane == -1 || isQuizActive) return;

        TriggerTrap();
    }

    // 🚀【やり方1 特化版】ハズレ床を奈落に落とす
    private void TriggerTrap()
    {
        isQuizActive = true;

        if (trapLane >= 0 && trapLane < 3 && lanes[trapLane] != null)
        {
            string[] laneNames = { "左レーン", "中央レーン", "右レーン" };
            Debug.Log($"🎯 【ハズレ床ワープ】⇒ 【{laneNames[trapLane]}】を奈落の底へ叩き落としました！");

            // 🚀【核心】SetActiveを触らず、位置だけを下方に100メートル吹き飛ばす
            lanes[trapLane].transform.position += new Vector3(0f, -100f, 0f);

            // ハズレ床に乗っている障害物やコインも一緒に巻き添えで落とす
            Transform trapFloorTransform = lanes[trapLane].transform;
            foreach (Transform child in trapFloorTransform)
            {
                if (child.gameObject.CompareTag("Obstacle") || child.name.Contains("Obstacle") || child.name.Contains("Coin"))
                {
                    child.position += new Vector3(0f, -100f, 0f);
                }
            }

            Invoke("EvaluateResult", 1.5f);
        }
    }

    private void EvaluateResult()
    {
        if (!isQuizStage || isEvaluated || playerTransform == null) return;
        isEvaluated = true;

        int playerLaneIndex = 1;
        float playerX = playerTransform.position.x;
        if (playerX < -1.5f) playerLaneIndex = 0;
        else if (playerX > 1.5f) playerLaneIndex = 2;
        else playerLaneIndex = 1;

        string[] laneNames = { "左レーン", "中央レーン", "右レーン" };

        if (playerLaneIndex == trapLane)
        {
            Debug.Log($"ゲームオーバー！ハズレの【{laneNames[trapLane]}】を走ったため落下！");
            if (playerController != null) playerController.forwardSpeed = 0f;
            Rigidbody playerRb = playerTransform.GetComponent<Rigidbody>();
            if (playerRb != null) playerRb.isKinematic = false;
        }
        else
        {
            Debug.Log($"正解！ハズレは【{laneNames[trapLane]}】でした。セーフ！");
        }
    }
}