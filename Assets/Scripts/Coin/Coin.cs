using UnityEngine;

public class Coin : MonoBehaviour
{
    [Header("獲得時の追加スコア")]
    public float scoreValue = 100f;

    [Header("回転スピード")]
    public float rotateSpeed = 100f;

    void Update()
    {
        // 💡 【ここを修正！】
        // Vector3.up から Vector3.forward に変更します
        transform.Rotate(Vector3.forward * rotateSpeed * Time.deltaTime);
    }

    // 💡 プレイヤーがすり抜けた（トリガーに触れた）瞬間に実行される
    private void OnTriggerEnter(Collider other)
    {
        // 触れてきたオブジェクトのタグが「Player」だった場合
        if (other.CompareTag("Player"))
        {
            // スコアを加算する
            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.currentScore += scoreValue;
                Debug.Log($"コイン獲得！ スコア +{scoreValue}");
            }

            // 💡 獲得したエフェクトや音を鳴らす場合はここに書く

            // コイン自体を消滅させる
            Destroy(gameObject);
        }
    }
}