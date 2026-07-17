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

    [Header("減速床のマテリアル（SpeedDownZoneMaterial）")]
    public Material mudMaterial;

    [Header("追従するプレイヤーのTransform")]
    public Transform playerTransform;

    [Header("足場の長さ（Z軸のサイズ）")]
    public float blockLength = 30f;

    [Header("画面内に事前に用意しておく足場の数")]
    public int maxBlocks = 5;

    [Header("障害物の出現する高さ（普通用）")]
    public float obstacleSpawnY = 1.0f;

    [Header("沼が発生する確率 (0.0 ～ 1.0)")]
    [Range(0f, 1f)]
    public float mudSpawnChance = 0.2f;

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
        if (activeBlocks.Count == 0 || playerTransform == null) return;

        // 💡 プレイヤーがハズレを踏んで垂直落下（isForcedFalling）に入っている時の安全装置
        bool isPlayerForcedFalling = false;
        if (PlayerController.Instance != null && PlayerController.Instance.isForcedFalling)
        {
            isPlayerForcedFalling = true;
        }

        if (isPlayerForcedFalling)
        {
            SpawnBlock(true);
            RemoveOldBlock();
            return;
        }

        // 💡【時速360km超高速対策】先読みするゆとり距離を「15枚分（450m）」まで超拡大！
        // プレイヤーの視界（地平線）の遥か彼方まで常に床が完成している状態を作ります。
        float safetyBuffer = blockLength * 15f; // 15枚分 ＝ 450m

        while (activeBlocks.Count > 0)
        {
            float leadingBlockZ = activeBlocks[activeBlocks.Count - 1].transform.position.z;

            // プレイヤーの現在地から最前方までの距離が 450m 未満になったら、追いつくまで1フレームで超高速連打生成
            if (leadingBlockZ - playerTransform.position.z < safetyBuffer)
            {
                SpawnBlock(true);
                RemoveOldBlock();
                Debug.Log($"⚡ 超音速先読み：プレイヤーの進路（450m先）に床を連打生成しました。");
            }
            else
            {
                break;
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

            if (!shouldBeQuiz && Random.Range(0f, 1f) < mudSpawnChance)
            {
                int randomLane = Random.Range(0, 3);
                GameObject targetFloor = null;

                if (randomLane == 0) targetFloor = quiz.leftFloor;
                if (randomLane == 1) targetFloor = quiz.centerFloor;
                if (randomLane == 2) targetFloor = quiz.rightFloor;

                if (targetFloor != null)
                {
                    var renderer = targetFloor.GetComponent<MeshRenderer>();
                    if (renderer != null && mudMaterial != null)
                    {
                        renderer.material = mudMaterial;
                        renderer.material.color = Color.white;
                        renderer.material.SetFloat("_Smoothness", 0.0f);
                    }

                    targetFloor.AddComponent<SlowMudZone>();
                }
            }
        }

        bool nextIsQuiz = (totalSpawnedBlocks + 1 == 5 || (totalSpawnedBlocks + 1 > 5 && (totalSpawnedBlocks + 1 - 5) % 20 == 0));
        bool prevWasQuiz = (totalSpawnedBlocks - 1 == 5 || (totalSpawnedBlocks - 1 > 5 && (totalSpawnedBlocks - 1 - 5) % 20 == 0));

        if (spawnObstacle && !shouldBeQuiz && !nextIsQuiz && !prevWasQuiz)
        {
            GenerateRandomObstacles(block);
            GenerateRandomCoins(block);
        }
    }

    void GenerateRandomObstacles(GameObject parentBlock)
    {
        if (obstaclePrefab == null) return;

        QuizFloorController quiz = parentBlock.GetComponent<QuizFloorController>();
        if (quiz == null) return;

        if (quiz.isQuizStage)
        {
            Debug.Log("🔒 クイズステージ（消える床）なので、障害物の生成をスキップしました。");
            return;
        }

        int obstacleCount = Random.Range(1, 3);
        List<int> availableLanes = new List<int> { 0, 1, 2 };

        for (int i = 0; i < obstacleCount; i++)
        {
            int listIndex = Random.Range(0, availableLanes.Count);
            int randomLane = availableLanes[listIndex];
            availableLanes.RemoveAt(listIndex);

            GameObject targetFloor = null;
            switch (randomLane)
            {
                case 0: targetFloor = quiz.leftFloor; break;
                case 1: targetFloor = quiz.centerFloor; break;
                case 2: targetFloor = quiz.rightFloor; break;
            }

            if (targetFloor == null) continue;

            if (targetFloor.GetComponent<SlowMudZone>() != null)
            {
                Debug.Log($"⚠️ {targetFloor.name} は沼なので障害物の生成をスキップしました");
                continue;
            }

            Vector3 localSpawnPosition = new Vector3(targetFloor.transform.localPosition.x, obstacleSpawnY, blockLength / 2f);
            GameObject obstacle = Instantiate(obstaclePrefab, Vector3.zero, Quaternion.identity);

            obstacle.transform.SetParent(parentBlock.transform);
            obstacle.transform.localPosition = localSpawnPosition;

            Debug.Log($"📦 {targetFloor.name} のレーンに障害物を生成しました！ ({i + 1}個目)");
        }
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