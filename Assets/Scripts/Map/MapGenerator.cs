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

            // クイズステージじゃない普通の床のとき、5%の確率で「沼レーン」を1つ作る
            if (!shouldBeQuiz && Random.Range(0f, 1f) < 0.05f)
            {
                int randomLane = Random.Range(0, 3);
                GameObject targetFloor = null;

                if (randomLane == 0) targetFloor = quiz.leftFloor;
                if (randomLane == 1) targetFloor = quiz.centerFloor;
                if (randomLane == 2) targetFloor = quiz.rightFloor;

                if (targetFloor != null)
                {
                    // 🎨 見た目を「泥のマテリアル」に差し替えて明るく設定する
                    var renderer = targetFloor.GetComponent<MeshRenderer>();
                    if (renderer != null && mudMaterial != null)
                    {
                        // 🟢 ここでマテリアルを泥に変更！
                        renderer.material = mudMaterial;

                        // 色を「真っ白」にして画像自体の明るさを100%引き出す
                        renderer.material.color = Color.white;

                        // テカテカした嫌な反射（光沢）をゼロにしてマットで見やすくする
                        renderer.material.SetFloat("_Smoothness", 0.0f);
                    }

                    // 🛠️ 【超重要】ここで別スクリプト（SlowMudZone）をリアルタイムでペタッと貼り付ける！
                    targetFloor.AddComponent<SlowMudZone>();
                }
            }
        }

        // ─── 🛠️ 修正後 ───
        // 次にクイズ床が来るか、または直前にクイズ床があったかを計算します
        bool nextIsQuiz = (totalSpawnedBlocks + 1 == 5 || (totalSpawnedBlocks + 1 > 5 && (totalSpawnedBlocks + 1 - 5) % 20 == 0));
        bool prevWasQuiz = (totalSpawnedBlocks - 1 == 5 || (totalSpawnedBlocks - 1 > 5 && (totalSpawnedBlocks - 1 - 5) % 20 == 0));

        // 「今の床」「次の床」「前の床」のどれかがクイズ床なら、このブロックには障害物を出さない！
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

        // ★追加：このブロックがクイズステージ（消える床がある状態）なら、障害物は一切生成しない
        if (quiz.isQuizStage)
        {
            Debug.Log("🔒 クイズステージ（消える床）なので、障害物の生成をスキップしました。");
            return;
        }

        // ─── ここから下は変更なし（通常の床の処理） ───
        int randomLane = Random.Range(0, 3);

        GameObject targetFloor = null;
        switch (randomLane)
        {
            case 0: targetFloor = quiz.leftFloor; break;
            case 1: targetFloor = quiz.centerFloor; break;
            case 2: targetFloor = quiz.rightFloor; break;
        }

        if (targetFloor == null) return;

        if (targetFloor.GetComponent<SlowMudZone>() != null)
        {
            Debug.Log("⚠️ 選んだレーンが沼なので障害物の生成をスキップしました");
            return;
        }

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