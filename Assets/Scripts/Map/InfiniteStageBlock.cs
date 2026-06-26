using UnityEngine;

public class InfiniteStageBlock : MonoBehaviour
{
    [Header("足場の長さ（Z軸のサイズ）")]
    public float blockLength = 30f;

    [Header("このステージの総枚数（5枚なら5）")]
    public int totalBlocks = 5;

    [Header("この足場が持っている障害物の親オブジェクト")]
    public Transform obstacleGroup;

    void Update()
    {
        if (Time.timeScale == 0f || PlayerController.Instance == null) return;

        // 1. 手前に移動させる
        float speed = PlayerController.Instance.forwardSpeed;
        transform.Translate(Vector3.back * speed * Time.deltaTime, Space.World);

        // 2. プレイヤー（Z=0）の後ろ（足場の長さの半分以上）に回り込んだら、一番奥にテレポート
        if (transform.position.z < -(blockLength / 2f))
        {
            // 他の全4枚の遥か後方にきれいにドッキングさせる計算
            float teleportDistance = blockLength * totalBlocks;
            transform.position += new Vector3(0, 0, teleportDistance);

            // 3. テレポートした瞬間に、障害物の位置をランダムにシャッフルする！
            RelocateObstacles();
        }
    }

    // 💡 障害物の位置をランダムに再配置する処理
    void RelocateObstacles()
    {
        if (obstacleGroup == null) return;

        // 子オブジェクト（障害物Cube）をすべてスキャンして位置をバラバラにする
        foreach (Transform obstacle in obstacleGroup)
        {
            int randomLane = Random.Range(-1, 2); // -1, 0, 1 (左、中央、右)
            float spawnX = randomLane * 3.0f;    // レーン幅3.0
            float randomZ = Random.Range(-blockLength / 2f + 5f, blockLength / 2f - 5f); // 足場の中央からのローカル距離

            // Yは1.0fで床の上に設置
            obstacle.localPosition = new Vector3(spawnX, 1.0f, randomZ);
        }
    }
}