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

                // ======================================================================
                // ✨【大修正】すり抜けバグを防ぐため、最初から「普通の固い床」として配置します！
                // ======================================================================
                var collider = lanes[i].GetComponent<Collider>();
                if (collider != null)
                {
                    collider.enabled = true;
                    collider.isTrigger = false; // すり抜けない固い床にする
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

    void FixedUpdate() { }

    public void StartQuiz(GameObject player)
    {
        if (isQuizStage == false) return;
        if (trapLane == -1 || isQuizActive) return;

        isQuizActive = true;
    }

    // 🚀プレイヤーが「いずれかの床」に着地した瞬間に呼ばれる
    public void OnPlayerEnterFloor(GameObject steppedFloor)
    {
        if (isQuizStage && !isQuizActive)
        {
            isQuizActive = true;
        }

        // 💡 【ここを追記！】
        // 3つのレーンをチェックして、沼スクリプトがついている床があれば通知を送る
        for (int i = 0; i < lanes.Length; i++)
        {
            if (lanes[i] == null) continue;

            SlowMudZone mud = lanes[i].GetComponent<SlowMudZone>();
            if (mud != null)
            {
                if (lanes[i] == steppedFloor)
                {
                    mud.OnPlayerStepOn(); // 今踏んだのが沼なら減速スイッチON
                }
                else
                {
                    mud.OnPlayerStepOff(); // 沼以外のレーンに移ったら解除
                }
            }
        }

        if (!isQuizStage || !isQuizActive || isTrapTriggered) return;

        // 踏まれた床が、ハズレの床（赤）と同じものだったら
        if (trapLane >= 0 && trapLane < 3 && lanes[trapLane] == steppedFloor)
        {
            TriggerTrap();
        }
        else
        {
            Debug.Log("🟢 セーフの床に着地しました。安全です。");
        }
    }

    // 🚀ハズレ床を奈落に落とす
    private void TriggerTrap()
    {
        isTrapTriggered = true; // 重複発動防止

        if (trapLane >= 0 && trapLane < 3 && lanes[trapLane] != null)
        {
            string[] laneNames = { "左レーン", "中央レーン", "右レーン" };
            Debug.Log($"🎯 【赤い床を完全検知！】⇒ 【{laneNames[trapLane]}】が落ちます！");

            var col = lanes[trapLane].GetComponent<Collider>();
            if (col != null) col.isTrigger = true;

            // 赤い床自体をローカル座標の下方に瞬間移動（奈落へ落とす）
            Vector3 targetLocalPos = lanes[trapLane].transform.localPosition;
            targetLocalPos.y = -100f;
            lanes[trapLane].transform.localPosition = targetLocalPos;

            Invoke("EvaluateResult", 0.1f);
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
            if (playerRb != null)
            {
                playerRb.isKinematic = false;
                playerRb.useGravity = true;
            }
        }
        else
        {
            Debug.Log($"正解！セーフ！");
        }
    }
}