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

    private Color defaultFloorColor = Color.gray;
    private bool hasSavedDefaultColor = false;
    private bool isTrapTriggered = false;

    void Awake() { }

    // 🚀位置のリセットと初期化
    public void InitializeQuizState(bool isQuiz)
    {
        isQuizStage = isQuiz;
        isTrapTriggered = false;

        leftFloor = transform.Find("LeftFloor")?.gameObject;
        centerFloor = transform.Find("CenterFloor")?.gameObject;
        rightFloor = transform.Find("RightFloor")?.gameObject;

        lanes[0] = leftFloor;
        lanes[1] = centerFloor;
        lanes[2] = rightFloor;

        for (int i = 0; i < lanes.Length; i++)
        {
            if (lanes[i] != null)
            {
                lanes[i].SetActive(true);

                var renderer = lanes[i].GetComponent<MeshRenderer>();
                if (renderer != null)
                {
                    renderer.enabled = true;
                    if (!hasSavedDefaultColor)
                    {
                        defaultFloorColor = renderer.material.color;
                        hasSavedDefaultColor = true;
                    }
                    renderer.material.color = defaultFloorColor;
                }

                var collider = lanes[i].GetComponent<Collider>();
                if (collider != null)
                {
                    collider.enabled = true;
                    collider.isTrigger = false;
                }

                Vector3 localPos = lanes[i].transform.localPosition;
                localPos.y = 0f;
                lanes[i].transform.localPosition = localPos;
            }
        }

        Transform sensor = transform.Find("QuizSensor");
        if (isQuizStage)
        {
            trapLane = Random.Range(0, 3);
            isQuizActive = false;
            isEvaluated = false;

            if (sensor != null) sensor.gameObject.SetActive(true);

            if (trapLane >= 0 && trapLane < 3 && lanes[trapLane] != null)
            {
                var trapRenderer = lanes[trapLane].GetComponent<MeshRenderer>();
                if (trapRenderer != null)
                {
                    trapRenderer.material.color = Color.red;
                }
            }
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

    void FixedUpdate()
    {
        // 💡 落下中のゲームオーバー判定：プレイヤーが一定の高さ（例：Yが -3以下）まで落ちたらGameOverPanelを呼び出す
        if (isQuizStage && isTrapTriggered && !isEvaluated && playerTransform != null)
        {
            if (playerTransform.position.y < -3.0f) // ✨この高さまでちゃんと落下したら
            {
                isEvaluated = true;
                ExecuteGameOver();
            }
        }
    }

    public void StartQuiz(GameObject player)
    {
        if (isQuizStage == false) return;
        if (trapLane == -1 || isQuizActive) return;

        isQuizActive = true;
    }

    public void OnPlayerEnterFloor(GameObject steppedFloor)
    {
        if (isQuizStage && !isQuizActive)
        {
            isQuizActive = true;
        }

        for (int i = 0; i < lanes.Length; i++)
        {
            if (lanes[i] == null) continue;

            SlowMudZone mud = lanes[i].GetComponent<SlowMudZone>();
            if (mud != null)
            {
                if (lanes[i] == steppedFloor)
                {
                    mud.OnPlayerStepOn();
                }
                else
                {
                    mud.OnPlayerStepOff();
                }
            }
        }

        if (!isQuizStage || !isQuizActive || isTrapTriggered) return;

        if (trapLane >= 0 && trapLane < 3 && lanes[trapLane] == steppedFloor)
        {
            TriggerTrap();
        }
        else
        {
            Debug.Log("🟢 セーフの床に着地しました。安全です。");
        }
    }

    private void TriggerTrap()
    {
        isTrapTriggered = true;

        if (trapLane >= 0 && trapLane < 3 && lanes[trapLane] != null)
        {
            string[] laneNames = { "左レーン", "中央レーン", "rightレーン" };
            Debug.Log($"🎯 【赤い床を完全検知！】⇒ 【{laneNames[trapLane]}】が落ちます！");

            var col = lanes[trapLane].GetComponent<Collider>();
            if (col != null) col.isTrigger = true;

            Vector3 targetLocalPos = lanes[trapLane].transform.localPosition;
            targetLocalPos.y = -100f;
            lanes[trapLane].transform.localPosition = targetLocalPos;

            // 💡 ここでの Invoke("EvaluateResult") は廃止して、実際の落下を待ちます。
        }
    }

    // 💡 プレイヤーが完全に落下した瞬間に、本物のGameOverPanelを一括で呼び出す処理
    private void ExecuteGameOver()
    {
        string[] laneNames = { "左レーン", "中央レーン", "右レーン" };
        Debug.Log($"ゲームオーバー！ハズレの【{laneNames[trapLane]}】から完全に落下しました！");

        GameOverManager gameOverManager = FindAnyObjectByType<GameOverManager>();
        if (gameOverManager != null)
        {
            gameOverManager.TriggerGameOver(); // ✨本物の一括管理パネルを起動！
        }
        else
        {
            // バックアップ用処理
            if (playerController != null) playerController.forwardSpeed = 0f;
            Time.timeScale = 0f;
        }
    }
}