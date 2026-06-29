using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    [Header("ベースとなる足場プレファブ（StageBlock）")]
    public GameObject stageBlockPrefab;

    [Header("障害物のプレファブ（1x1x1のCube）")]
    public GameObject obstaclePrefab;

    [Header("追従するプレイヤーのTransform")]
    public Transform playerTransform;

    [Header("足場の長さ（Z軸のサイズ）")]
    public float blockLength = 30f;

    [Header("画面内に事前に用意しておく足場の数")]
    public int maxBlocks = 5;

    [Header("障害物の出現する高さ")]
    public float obstacleSpawnY = 1.0f;

    private List<GameObject> activeBlocks = new List<GameObject>();
    private float nextSpawnZ = 0f;

    // 🚀 エラー解消のためにこの関数を追加します
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

        // 最初に一気に床を生成
        for (int i = 0; i < maxBlocks; i++)
        {
            // 最初の方の床には障害物を置かない
            SpawnBlock(i >= 2);
        }
    }

    void Update()
    {
        if (activeBlocks.Count == 0) return;

        // 一番古い床の後端がプレイヤーを通り過ぎたら新しく生成
        float blockBackendZ = activeBlocks[0].transform.position.z + (blockLength / 2f);
        if (blockBackendZ < -blockLength)
        {
            SpawnBlock(true);
            RemoveOldBlock();
        }
    }

    void SpawnBlock(bool spawnObstacle)
    {
        if (stageBlockPrefab == null) return;

        // 🚀 常に同じベース床（StageBlock）を生成する（位置ズレを完全防止）
        Vector3 spawnPos = new Vector3(0f, 0f, nextSpawnZ);
        GameObject block = Instantiate(stageBlockPrefab, spawnPos, Quaternion.identity);
        nextSpawnZ += blockLength;

        block.transform.position = spawnPos;
        block.AddComponent<StageMover>();
        activeBlocks.Add(block);

        // 障害物とコインの生成（通常の床として機能させる場合）
        if (spawnObstacle)
        {
            GenerateRandomObstacles(block);
            GenerateRandomCoins(block);
        }
    }

    void GenerateRandomObstacles(GameObject parentBlock)
    {
        if (obstaclePrefab == null) return;

        // 床に QuizFloorController がついていて、かつそこでクイズがアクティブ（3レーン化）になる予定の床なら障害物は置かない
        var quiz = parentBlock.GetComponent<QuizFloorController>();
        if (quiz != null && quiz.isQuizStage) return;

        // 🚀 【追加】ランダムでレーンを選ぶ (0:左, 1:中央, 2:右)
        int randomLane = Random.Range(0, 3);

        // 🚀 【追加】プレイヤーの移動幅（1.5）に合わせて、X座標を決定する
        // 0なら -1.5 (左), 1なら 0 (中央), 2なら 1.5 (右) になります
        float spawnX = (randomLane - 1) * 1.5f;

        float spawnZ = parentBlock.transform.position.z + (blockLength / 2f);

        // 🚀 【修正】固定だった 0f を、ランダムに決まった spawnX に変更します
        Vector3 spawnPosition = new Vector3(spawnX, obstacleSpawnY, spawnZ);

        GameObject obstacle = Instantiate(obstaclePrefab, spawnPosition, Quaternion.identity);
        obstacle.transform.SetParent(parentBlock.transform);
    }

    void GenerateRandomCoins(GameObject parentBlock)
    {
        var quiz = parentBlock.GetComponent<QuizFloorController>();
        if (quiz != null && quiz.isQuizStage) return;

        if (CoinManager.Instance == null) return;
        int coinGroupCount = Random.Range(1, 3);
        for (int g = 0; g < coinGroupCount; g++)
        {
            // 🚀 【追加】コインも真ん中固定をやめて、ランダムに散らします
            int randomLane = Random.Range(0, 3);
            float spawnX = (randomLane - 1) * 1.5f; // 左(-1.5), 中央(0), 右(1.5)

            float startZ = parentBlock.transform.position.z + Random.Range(3f, blockLength - 10f);
            int runLength = Random.Range(3, 6);
            CoinManager.Instance.SpawnCoinGroup(startZ, spawnX, runLength, blockLength, parentBlock.transform.position.z);
        }
    }

    void RemoveOldBlock()
    {
        if (activeBlocks.Count > 0)
        {
            Destroy(activeBlocks[0]);
            activeBlocks.RemoveAt(0);
            nextSpawnZ -= blockLength;
        }
    }
}