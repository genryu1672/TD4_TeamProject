using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    [Header("生成する足場のプレファブ")]
    public GameObject[] stageBlockPrefabs;

    [Header("障害物のプレファブ（1x1x1のCube）")]
    public GameObject obstaclePrefab;

    [Header("追従するプレイヤーのTransform")]
    public Transform playerTransform;

    [Header("足場の長さ（Z軸のサイズ）")]
    public float blockLength = 30f;

    [Header("画面内に事前に用意しておく足場の数")]
    public int maxBlocks = 5;

    [Header("レーン設定")]
    public float laneDistance = 3.0f;

    [Header("障害物の出現する高さ（埋まる場合は数値を上げてね）")]
    public float obstacleSpawnY = 1.0f;

    private List<GameObject> activeBlocks = new List<GameObject>();
    private float nextSpawnZ = 0f;

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

        // 最初に配置する足場の生成
        for (int i = 0; i < maxBlocks; i++)
        {
            if (i < 2)
            {
                SpawnBlock(false); // 最初の2枚は障害物なし（安全地帯）
            }
            else
            {
                SpawnBlock(true);  // それ以降は障害物あり
            }
        }
    }

    void Update()
    {
        if (activeBlocks.Count == 0) return;

        // 一番手前の足場の「お尻（後端）」のZ座標を計算
        float blockBackendZ = activeBlocks[0].transform.position.z + (blockLength / 2f);

        // 💡 【ここを修正！】
        // 判定を「0f未満（プレイヤーの真横）」から「-blockLength（足場1枚分後ろ）」に変更します。
        // これにより、足場がプレイヤーを完全に通り過ぎて、画面の後ろに隠れるまで削除されなくなります。
        if (blockBackendZ < -blockLength)
        {
            SpawnBlock(true);
            RemoveOldBlock();
        }
    }

    void SpawnBlock(bool spawnObstacle)
    {
        if (stageBlockPrefabs.Length == 0) return;

        GameObject selectedPrefab = stageBlockPrefabs[Random.Range(0, stageBlockPrefabs.Length)];
        GameObject block = Instantiate(selectedPrefab);

        block.transform.position = new Vector3(0, 0, nextSpawnZ);
        nextSpawnZ += blockLength;

        block.AddComponent<StageMover>();
        activeBlocks.Add(block);

        if (spawnObstacle && obstaclePrefab != null)
        {
            GenerateRandomObstacles(block);
        }
    }

    // 💡 引数を Transform から GameObject 自体を受け取るように変更
    void GenerateRandomObstacles(GameObject parentBlock)
    {
        int obstacleCount = Random.Range(1, 3);

        for (int i = 0; i < obstacleCount; i++)
        {
            int randomLane = Random.Range(-1, 2);
            float spawnX = randomLane * laneDistance;

            // 💡 【ここを修正】
            // モデルの原点依存を廃止し、生成された床の現在の「ワールドZ座標」の真ん中付近（+15fなど）に直接配置します
            float spawnZ = parentBlock.transform.position.z + Random.Range(5f, blockLength - 5f);

            // 確実なワールド座標を作成
            Vector3 worldObstaclePosition = new Vector3(spawnX, obstacleSpawnY, spawnZ);

            GameObject obstacle = Instantiate(obstaclePrefab);

            // 先にワールド座標で位置を決めてから親子関係を結ぶ（これでズレません）
            obstacle.transform.position = worldObstaclePosition;
            obstacle.transform.SetParent(parentBlock.transform, true);
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