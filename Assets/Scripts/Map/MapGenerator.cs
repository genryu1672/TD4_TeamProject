using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    [Header("生成する足場のプレファブ（複数登録できるように配列にします）")]
    public GameObject[] stageBlockPrefabs; // ★GameObject から GameObject[] に変更

    [Header("追従するプレイヤーのTransform")]
    [HideInInspector]
    public Transform playerTransform;

    [Header("足場の長さ（Z軸のサイズ）")]
    public float blockLength = 30f;

    [Header("画面内に事前に用意しておく足場の数")]
    public int maxBlocks = 5;

    private List<GameObject> activeBlocks = new List<GameObject>();
    private float nextSpawnZ = 0f;

    public void SetPlayer(Transform target)
    {
        playerTransform = target;
    }

    void Start()
    {
        for (int i = 0; i < maxBlocks; i++)
        {
            SpawnBlock();
        }
    }

    void Update()
    {
        if (playerTransform == null) return;

        if (playerTransform.position.z > activeBlocks[0].transform.position.z + blockLength)
        {
            SpawnBlock();
            RemoveOldBlock();
        }
    }

    void SpawnBlock()
    {
        // ★登録されたプレファブの中からランダムで1つ選ぶ処理
        int randomIndex = Random.Range(0, stageBlockPrefabs.Length);
        GameObject selectedPrefab = stageBlockPrefabs[randomIndex];

        GameObject block = Instantiate(selectedPrefab); // 選ばれたものを生成

        block.transform.position = new Vector3(0, 0, nextSpawnZ);
        activeBlocks.Add(block);
        nextSpawnZ += blockLength;
    }

    void RemoveOldBlock()
    {
        Destroy(activeBlocks[0]);
        activeBlocks.RemoveAt(0);
    }
}