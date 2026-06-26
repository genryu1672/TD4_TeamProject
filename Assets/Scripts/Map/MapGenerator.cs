using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    [Header("生成する足場のプレファブ")]
    public GameObject[] stageBlockPrefabs;

    [Header("障害物のプレファブ（1x1x1のCube）")]
    public GameObject obstaclePrefab;

    // 💡 coinPrefab と coinSpawnY のインスペクター項目は削除しました

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

        // 🚀 もし選ばれたプレファブの名前が「3LaneStageBlock」だったら
        if (selectedPrefab.name == "3LaneStageBlock")
        {
            // 3本のレーンと黒線をまとめる親グループを作成
            GameObject parentGroup = new GameObject("3LaneStageGroup");
            parentGroup.transform.position = new Vector3(0, 0, nextSpawnZ);
            parentGroup.AddComponent<StageMover>();
            activeBlocks.Add(parentGroup);

            // -1(左), 0(中央), 1(右) のどれをハズレにするか決定
            int trapLane = Random.Range(-1, 2);

            // ① 1本の細いステージブロック(StageBlock)を3回ループして横に並べる
            for (int lane = -1; lane <= 1; lane++)
            {
                GameObject singleBlock = Instantiate(stageBlockPrefabs[0]);
                singleBlock.transform.SetParent(parentGroup.transform);
                singleBlock.transform.localPosition = new Vector3(lane * laneDistance, 0, 0);

                // もしこのレーンがハズレなら罠スクリプトをつける
                if (lane == trapLane)
                {
                    BreakableFloor trapScript = singleBlock.AddComponent<BreakableFloor>();
                    trapScript.isTrap = true;

                    BoxCollider col = singleBlock.GetComponent<BoxCollider>();
                    if (col != null) col.isTrigger = true;
                }
            }

            // 🎨 【ここを追加！】境目に「黒い線（仕切り）」を2本生成する
            // レーンとレーンの間（左と中央の間、中央と右の間）に配置します
            float[] linePositions = { -laneDistance / 2f, laneDistance / 2f };
            foreach (float lineX in linePositions)
            {
                GameObject line = GameObject.CreatePrimitive(PrimitiveType.Cube);
                line.name = "DividerLine";
                line.transform.SetParent(parentGroup.transform);

                // 床よりほんの少しだけ高く(0.02f)して、細長い黒線を配置
                line.transform.localPosition = new Vector3(lineX, 0.02f, blockLength / 2f);
                line.transform.localScale = new Vector3(0.15f, 1.02f, blockLength); // 幅15cmの線

                // コライダーは邪魔なので消す
                Destroy(line.GetComponent<BoxCollider>());

                // 色を黒にする
                Renderer lineRen = line.GetComponent<Renderer>();
                if (lineRen != null)
                {
                    lineRen.material.color = Color.black;
                }
            }

            // 🛠️ ② 近づくまで隠すための「ダミー床」をプログラムで自動追加！
            GameObject dummyFloor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            dummyFloor.name = "DummyFloor";
            dummyFloor.transform.SetParent(parentGroup.transform);
            dummyFloor.transform.localPosition = new Vector3(0, 0.01f, blockLength / 2f);
            dummyFloor.transform.localScale = new Vector3(laneDistance * 3, 1.01f, blockLength);
            Destroy(dummyFloor.GetComponent<BoxCollider>());

            // 🛠️ ③ プレイヤーの接近を検知する「センサー」を自動追加！
            GameObject sensorObj = new GameObject("Sensor");
            sensorObj.transform.SetParent(parentGroup.transform);
            sensorObj.transform.localPosition = new Vector3(0, 1f, 0f);

            BoxCollider sensorCol = sensorObj.AddComponent<BoxCollider>();
            sensorCol.isTrigger = true;
            sensorCol.size = new Vector3(laneDistance * 3, 5f, 1f);

            FloorSensor sensorScript = sensorObj.AddComponent<FloorSensor>();
            sensorScript.dummyFloor = dummyFloor;

            nextSpawnZ += blockLength;
            return;
        }

        // 普通の床のときは今までの処理
        GameObject block = Instantiate(selectedPrefab);
        block.transform.position = new Vector3(0, 0, nextSpawnZ);
        nextSpawnZ += blockLength;

        block.AddComponent<StageMover>();
        activeBlocks.Add(block);

        if (spawnObstacle)
        {
            if (obstaclePrefab != null) GenerateRandomObstacles(block);
            GenerateRandomCoins(block);
        }
    }

    void GenerateRandomObstacles(GameObject parentBlock)
    {
        int obstacleCount = Random.Range(1, 3);

        for (int i = 0; i < obstacleCount; i++)
        {
            int randomLane = Random.Range(-1, 2);
            float spawnX = randomLane * laneDistance;

            float spawnZ = parentBlock.transform.position.z + Random.Range(5f, blockLength - 5f);
            Vector3 worldObstaclePosition = new Vector3(spawnX, obstacleSpawnY, spawnZ);

            GameObject obstacle = Instantiate(obstaclePrefab);
            obstacle.transform.position = worldObstaclePosition;
            obstacle.transform.SetParent(parentBlock.transform, true);
        }
    }

    // 💡 コインの生成ロジックを CoinManager に頼む形にスッキリ化！
    void GenerateRandomCoins(GameObject parentBlock)
    {
        if (CoinManager.Instance == null) return;

        // 1枚の床に何箇所コインの束を置くか（1〜2箇所）
        int coinGroupCount = Random.Range(1, 3);

        for (int g = 0; g < coinGroupCount; g++)
        {
            int randomLane = Random.Range(-1, 2);
            float spawnX = randomLane * laneDistance;

            float startZ = parentBlock.transform.position.z + Random.Range(3f, blockLength - 10f);
            int runLength = Random.Range(3, 6);

            // 🚀 面倒な生成処理はすべてマネージャーにおまかせ！
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