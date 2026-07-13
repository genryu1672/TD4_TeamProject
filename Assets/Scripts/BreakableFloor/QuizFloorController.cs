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

    private int safeLane = -1; // ★修正：trapLaneから「safeLane（安全なレーン）」に変更
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
            safeLane = Random.Range(0, 3); // ★修正：3つのうち「1つだけ安全な床」を決める
            isQuizActive = false;
            isEvaluated = false;

            if (sensor != null) sensor.gameObject.SetActive(true);

            // ★修正：安全な床「以外」の2つの床を赤く光らせる
            for (int i = 0; i < lanes.Length; i++)
            {
                if (i == safeLane) continue; // 安全な床はスキップ

                if (lanes[i] != null)
                {
                    var trapRenderer = lanes[i].GetComponent<MeshRenderer>();
                    if (trapRenderer != null)
                    {
                        trapRenderer.material.color = Color.red;
                        trapRenderer.material.EnableKeyword("_EMISSION");
                        trapRenderer.material.SetColor("_EmissionColor", Color.red * 2.0f);
                    }
                }
            }
        }
        else
        {
            safeLane = -1; // ★修正
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
        if (isQuizStage && isTrapTriggered && !isEvaluated && playerTransform != null)
        {
            if (playerTransform.position.y < -3.0f)
            {
                isEvaluated = true;
                ExecuteGameOver();
            }
        }
    }

    public void StartQuiz(GameObject player)
    {
        if (isQuizStage == false) return;
        if (safeLane == -1 || isQuizActive) return; // ★修正

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

        // ★修正：踏んだ床が「安全な床ではない」なら罠を発動する
        int steppedLaneIndex = -1;
        for (int i = 0; i < lanes.Length; i++)
        {
            if (lanes[i] == steppedFloor)
            {
                steppedLaneIndex = i;
                break;
            }
        }

        if (steppedLaneIndex != -1 && steppedLaneIndex != safeLane)
        {
            TriggerTrap(steppedLaneIndex); // ★修正：踏んだハズレのレーン番号を渡す
        }
        else
        {
            Debug.Log("🟢 セーフの床に着地しました。安全です。");
        }
    }

    // ★修正：引数で落ちるレーンのインデックスを受け取るように変更
    private void TriggerTrap(int fallenLane)
    {
        isTrapTriggered = true;

        if (fallenLane >= 0 && fallenLane < 3 && lanes[fallenLane] != null)
        {
            string[] laneNames = { "左レーン", "中央レーン", "右レーン" };
            Debug.Log($"🎯 【赤い床を完全検知！】⇒ 【{laneNames[fallenLane]}】が落ちます！");

            var col = lanes[fallenLane].GetComponent<Collider>();
            if (col != null) col.isTrigger = true;

            Vector3 targetLocalPos = lanes[fallenLane].transform.localPosition;
            targetLocalPos.y = -100f;
            lanes[fallenLane].transform.localPosition = targetLocalPos;
        }
    }

    private void ExecuteGameOver()
    {
        // ★修正：ログ用に安全だったレーンを表示
        string[] laneNames = { "左レーン", "中央レーン", "右レーン" };
        Debug.Log($"ゲームオーバー！正解の【{laneNames[safeLane]}】以外から完全に落下しました！");

        GameOverManager gameOverManager = FindAnyObjectByType<GameOverManager>();
        if (gameOverManager != null)
        {
            gameOverManager.TriggerGameOver();
        }
        else
        {
            if (playerController != null) playerController.forwardSpeed = 0f;
            Time.timeScale = 0f;
        }
    }
}