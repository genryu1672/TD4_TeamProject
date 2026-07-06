using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    [Header("ベースとなる足場プレファブ（StageBlock）")]
    public GameObject stageBlockPrefab;

    [Header("障害物のプレファブ（1x1x1のCube）")]
    public GameObject obstaclePrefab;

    [Header("減速床のプレファブ (※別スクリプト化したため使用しません)")]
    public GameObject speedDownZonePrefab;

    [Header("追従するプレイヤーのTransform (※床移動型なので基本不要ですが残します)")]
    public Transform playerTransform;

    [Header("足場の長さ（Z軸のサイズ）")]
    public float blockLength = 30f;

    [Header("画面内に事前に用意しておく足場の数")]
    public int maxBlocks = 5;

    [Header("障害物の出現する高さ（普通用）")]
    public float obstacleSpawnY = 1.0f;

    private List<GameObject> activeBlocks = new List<GameObject>();
    private float nextSpawnZ = 0f;
    private int totalSpawnedBlocks = 0;

    public void SetPlayer(Transform target)
    {
        playerTransform = target;
    }

    void Start()
    {
        if (playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) playerTransform = player.transform;
        }

        nextSpawnZ = 0f;
        for (int i = 0; i < maxBlocks; i++)
        {
            SpawnBlock(i >= 2);
        }
    }

    void Update()
    {
        if (activeBlocks.Count == 0) return;

        GameObject container = GameObject.Find("StageContainer");

        if (container != null)
        {
            float currentWorldZ = activeBlocks[0].transform.localPosition.z + container.transform.position.z;

            if (currentWorldZ <= -blockLength)
            {
                SpawnBlock(true);
                RemoveOldBlock();
            }
        }
        else
        {
            if (activeBlocks[0].transform.position.z <= -blockLength)
            {
                SpawnBlock(true);
                RemoveOldBlock();
            }
        }
    }

    void SpawnBlock(bool spawnObstacle)
    {
        if (stageBlockPrefab == null) return;

        GameObject block = Instantiate(stageBlockPrefab, Vector3.zero, Quaternion.identity);

        GameObject container = GameObject.Find("StageContainer");
        if (container != null)
        {
            block.transform.SetParent(container.transform);
            block.transform.localPosition = new Vector3(0f, 0f, nextSpawnZ);
        }
        else
        {
            block.transform.position = new Vector3(0f, 0f, nextSpawnZ);
        }

        nextSpawnZ += blockLength;

        activeBlocks.Add(block);
        totalSpawnedBlocks++;

        bool shouldBeQuiz = (totalSpawnedBlocks == 5 || (totalSpawnedBlocks > 5 && (totalSpawnedBlocks - 5) % 20 == 0));

        QuizFloorController quiz = block.GetComponent<QuizFloorController>();
        if (quiz != null)
        {
            quiz.InitializeQuizState(shouldBeQuiz);

            // 💡【ここを追記！】
            // クイズステージじゃない普通の床のとき、30%の確率で「沼レーン」を1つ作る
            if (!shouldBeQuiz && Random.Range(0f, 1f) < 0.30f)
            {
                int randomLane = Random.Range(0, 3);
                GameObject targetFloor = null;

                if (randomLane == 0) targetFloor = quiz.leftFloor;
                if (randomLane == 1) targetFloor = quiz.centerFloor;
                if (randomLane == 2) targetFloor = quiz.rightFloor;

                if (targetFloor != null)
                {
                    // 🎨 見た目を沼っぽく茶色にする
                    var renderer = targetFloor.GetComponent<MeshRenderer>();
                    if (renderer != null) renderer.material.color = new Color(0.4f, 0.25f, 0.15f);

                    // 🛠️ 【超重要】ここで別スクリプト（SlowMudZone）をリアルタイムでペタッと貼り付ける！
                    targetFloor.AddComponent<SlowMudZone>();
                }
            }
        }

        if (spawnObstacle)
        {
            GenerateRandomObstacles(block);
            GenerateRandomCoins(block);
        }
    }

    void GenerateRandomObstacles(GameObject parentBlock)
    {
        if (obstaclePrefab == null) return;

        // 1. ランダムにレーン（0〜2）を決める
        int randomLane = Random.Range(0, 3);

        QuizFloorController quiz = parentBlock.GetComponent<QuizFloorController>();
        if (quiz == null) return;

        // 2. 決まったレーンに応じた「床オブジェクト」を取得する
        GameObject targetFloor = null;
        switch (randomLane)
        {
            case 0: targetFloor = quiz.leftFloor; break;
            case 1: targetFloor = quiz.centerFloor; break;
            case 2: targetFloor = quiz.rightFloor; break;
        }

        if (targetFloor == null) return;

        // 💡【チェック】もしその床が「沼（SlowMudZone）」なら障害物は置かない
        if (targetFloor.GetComponent<SlowMudZone>() != null)
        {
            Debug.Log("⚠️ 選んだレーンが沼なので障害物の生成をスキップしました");
            return;
        }

        // 3. 床のローカル座標（特にX座標）を基準にして障害物を生成する
        // X座標は床の位置、Z座標はブロックの真ん中（blockLength / 2f）
        Vector3 localSpawnPosition = new Vector3(targetFloor.transform.localPosition.x, obstacleSpawnY, blockLength / 2f);

        GameObject obstacle = Instantiate(obstaclePrefab, Vector3.zero, Quaternion.identity);

        obstacle.transform.SetParent(parentBlock.transform);
        obstacle.transform.localPosition = localSpawnPosition;

        Debug.Log($"📦 {targetFloor.name} のレーンに障害物を生成しました！");
    }

    void GenerateRandomCoins(GameObject parentBlock)
    {
        if (CoinManager.Instance == null) return;
    }

    void RemoveOldBlock()
    {
        if (activeBlocks.Count > 0)
        {
            Destroy(activeBlocks[0]);
            activeBlocks.RemoveAt(0);
        }
    }
}