using UnityEngine;

public class CoinManager : MonoBehaviour
{
    // シングルトン（どこからでも1行で呼び出せる仕組み）
    public static CoinManager Instance { get; private set; }

    [Header("コインのプレファブ")]
    public GameObject coinPrefab;

    [Header("コインの出現する高さ")]
    public float coinSpawnY = 1.2f;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    /// <summary>
    /// 指定されたレーンとZ座標に、歪まない安全な方法でコインの束を生成する
    /// </summary>
    public void SpawnCoinGroup(float startZ, float spawnX, int runLength, float blockLength, float parentBlockZ)
    {
        if (coinPrefab == null) return;

        for (int i = 0; i < runLength; i++)
        {
            // 1.5m ずつ間隔を空けて奥に並べる
            float spawnZ = startZ + (i * 1.5f);

            // 床の長さを超えて飛び出さないようにガード
            if (spawnZ > parentBlockZ + blockLength - 2f) break;

            Vector3 worldCoinPosition = new Vector3(spawnX, coinSpawnY, spawnZ);

            // 🚀 親（床）を指定せずに、完全に独立したオブジェクトとして生成する（これで絶対に歪まない！）
            GameObject coin = Instantiate(coinPrefab, worldCoinPosition, coinPrefab.transform.rotation);

            // プレファブ本来のスケールを確実に適用
            coin.transform.localScale = coinPrefab.transform.localScale;

            // 🚀 床の子供にしない代わりに、手前に流れるスクリプトを直接付与する
            if (coin.GetComponent<StageMover>() == null)
            {
                coin.AddComponent<StageMover>();
            }

            // 🚀 画面外（プレイヤーの後ろ）に通り過ぎたら自動で消滅するようタイマーをかける
            Destroy(coin, 10f);
        }
    }
}