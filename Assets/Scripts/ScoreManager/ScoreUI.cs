using UnityEngine;
using TMPro; // ?? TextMeshProを使うために必要

public class ScoreUI : MonoBehaviour
{
    [Header("UIテキストの紐付け")]
    public TextMeshProUGUI currentScoreText; // 現在のスコア用
    public TextMeshProUGUI highScoreText;    // ハイスコア用
    public TextMeshProUGUI lastScoreText;    // 前回のスコア用（もしあれば）

    void Update()
    {
        if (ScoreManager.Instance == null) return;

        // --- 現在のスコアを更新 ---
        if (currentScoreText != null)
        {
            // (int)でキャストして小数点を切り捨てて表示
            currentScoreText.text = "SCORE: " + ((int)ScoreManager.Instance.currentScore).ToString();
        }

        // --- ハイスコアを更新 ---
        if (highScoreText != null)
        {
            highScoreText.text = "HI-SCORE: " + ((int)ScoreManager.Instance.highScore).ToString();
        }
    }

    void Start()
    {
        // --- 前回のスコアはゲーム開始時に1回だけ表示を更新 ---
        if (ScoreManager.Instance != null && lastScoreText != null)
        {
            lastScoreText.text = "LAST: " + ((int)ScoreManager.Instance.lastScore).ToString();
        }
    }
}