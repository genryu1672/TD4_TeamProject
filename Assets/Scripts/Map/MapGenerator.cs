using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    [Header("ベースとなる足場プレファブ（StageBlock）")]
    public GameObject stageBlockPrefab;

    [Header("障害物のプレファブ（1x1x1のCube）")]
    public GameObject obstaclePrefab;

    [Header("追従するプレイヤーのTransform (※床移動型なので基本不要ですが残します)")]
    public Transform playerTransform;

    [Header("足場の長さ（Z軸のサイズ）")]
    public float blockLength = 30f;

    [Header("画面内に事前に用意しておく足場の数")]
    public int maxBlocks = 5;

    [Header("障害物の出現する高さ")]
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

        // 🚀 最初に初期枚数（例: 5枚）を Z = 0, 30, 60, 90, 120 に綺麗に並べる
        nextSpawnZ = 0f;
        for (int i = 0; i < maxBlocks; i++)
        {
            SpawnBlock(i >= 2);
        }
    }

    void Update()
    {
        if (activeBlocks.Count == 0) return;

        // 🚀【床移動型・専用システム】
        // 先頭（一番古い）床が手前に進んできて、完全に画面の後ろ（Zがマイナス方向）に消え去ったら
        // 古い床を消して、奥に新しい床を1枚補充する！
        if (activeBlocks[0].transform.position.z <= -blockLength)
        {
            // 奥に1枚補充（障害物あり）
            SpawnBlock(true);

            // 画面外に出た手前の床を消去
            RemoveOldBlock();
        }
    }

    void SpawnBlock(bool spawnObstacle)
    {
        if (stageBlockPrefab == null) return;

        // 🚀 次に生成すべき絶対座標（nextSpawnZ）に生成
        Vector3 spawnPos = new Vector3(0f, 0f, nextSpawnZ);
        GameObject block = Instantiate(stageBlockPrefab, spawnPos, Quaternion.identity);

        // 🚀【超重要】次の床の生成位置は、単純に blockLength 分だけプラスしていくだけ！
        // 変な引き算を混ぜないことで、絶対に床同士が隙間なく結合します。
        nextSpawnZ += blockLength;

        // 🚀 元のインスペクターにStageMoverがついているため、
        // ここで AddComponent<StageMover>() を重ねて行うと2重に動いてバグるので削除しました。

        activeBlocks.Add(block);
        totalSpawnedBlocks++;

        // 5枚目、そこから20枚ごとにクイズにするかどうかのフラグを決定
        bool shouldBeQuiz = (totalSpawnedBlocks == 5 || (totalSpawnedBlocks > 5 && (totalSpawnedBlocks - 5) % 20 == 0));

        // 生成したその場で、床を完全に初期化する
        QuizFloorController quiz = block.GetComponent<QuizFloorController>();
        if (quiz != null)
        {
            quiz.InitializeQuizState(shouldBeQuiz);
        }

        // 障害物とコインの生成
        if (spawnObstacle)
        {
            GenerateRandomObstacles(block);
            GenerateRandomCoins(block);
        }
    }

    void GenerateRandomObstacles(GameObject parentBlock)
    {
        if (obstaclePrefab == null) return;

        var quiz = parentBlock.GetComponent<QuizFloorController>();
        if (quiz != null && quiz.isQuizStage) return;

        int randomLane = Random.Range(0, 3);
        float spawnX = (randomLane - 1) * 1.5f;
        float spawnZ = parentBlock.transform.position.z + (blockLength / 2f);

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
            int randomLane = Random.Range(0, 3);
            float spawnX = (randomLane - 1) * 1.5f;

            float startZ = parentBlock.transform.position.z + Random.Range(3f, blockLength - 10f);
            int runLength = Random.Range(3, 6);
            CoinManager.Instance.SpawnCoinGroup(startZ, spawnX, runLength, blockLength, parentBlock.transform.position.z);
        }
    }

    void RemoveOldBlock()
    {
        if (activeBlocks.Count > 0)
        {
            // 🚀 手前の古い床を削除
            Destroy(activeBlocks[0]);
            activeBlocks.RemoveAt(0);

            // 🚀【バグの完全根絶】
            // 古いコードにあった「nextSpawnZ -= blockLength;」は完全に廃止！
            // これにより、生成位置が手前に狂って床が消失するバグが完全に直ります。
        }
    }
}