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
        if (playerTransform == null || activeBlocks.Count == 0) return;

        // ★プレイヤーが今いる足場を通り過ぎたら、進行方向に新しい道を生成して古い道を消す
        if (playerTransform.position.z > activeBlocks[0].transform.position.z + blockLength)
        {
            SpawnBlock(true);
            RemoveOldBlock();
        }
    }

    void SpawnBlock(bool spawnObstacle)
    {
        if (stageBlockPrefabs.Length == 0) return;

        // ランダム、または最初のプレファブを選択
        GameObject selectedPrefab = stageBlockPrefabs[Random.Range(0, stageBlockPrefabs.Length)];
        GameObject block = Instantiate(selectedPrefab);
        block.transform.position = new Vector3(0, 0, nextSpawnZ);
        activeBlocks.Add(block);

        // 障害物の生成
        if (spawnObstacle && obstaclePrefab != null)
        {
            GenerateRandomObstacles(block.transform, nextSpawnZ);
        }

        // 次の生成位置を足場の長さ分だけ進める
        nextSpawnZ += blockLength;
    }

    void GenerateRandomObstacles(Transform parentBlock, float blockZ)
    {
        int obstacleCount = Random.Range(1, 3);

        for (int i = 0; i < obstacleCount; i++)
        {
            int randomLane = Random.Range(-1, 2);
            float spawnX = randomLane * laneDistance;
            float spawnZ = blockZ + Random.Range(5f, blockLength - 5f);

            Vector3 obstaclePosition = new Vector3(spawnX, 1.0f, spawnZ);

            GameObject obstacle = Instantiate(obstaclePrefab, obstaclePosition, Quaternion.identity);
            obstacle.transform.SetParent(parentBlock);
        }
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