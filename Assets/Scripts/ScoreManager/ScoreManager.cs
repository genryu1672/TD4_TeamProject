using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    [Header("現在のスコア")]
    public float currentScore = 0f;

    [Header("前回のスコア（NEW!）")]
    public float lastScore = 0f;

    [Header("ハイスコア")]
    public float highScore = 0f;

    // 保存用のキー名
    private const string HighScoreKey = "RunGame_HighScore";
    private const string LastScoreKey = "RunGame_LastScore"; // 💡 前回のスコア用

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        // 🚀 【テスト用】これだけを追加してゲームを一度再生する！
        //ResetHighScore();

        // 💡 ゲーム開始時に、保存されているハイスコアと前回のスコアを読み込む
        highScore = PlayerPrefs.GetFloat(HighScoreKey, 0f);
        lastScore = PlayerPrefs.GetFloat(LastScoreKey, 0f); // 💡 前回値をロード
    }

    void Update()
    {
        if (Time.timeScale == 0f || PlayerController.Instance == null) return;

        // スコアを毎フレーム加算
        currentScore += PlayerController.Instance.forwardSpeed * Time.deltaTime;

        // 現在のスコアがハイスコアを超えたら、リアルタイムにハイスコアも更新
        if (currentScore > highScore)
        {
            highScore = currentScore;
        }
    }

    // 💡 リトライ時やゲームオーバー時に呼び出される関数
    public void SaveHighScore()
    {
        // 🚀 【ここを追加】今回のスコアを「前回のスコア」として上書き保存する
        lastScore = currentScore;
        PlayerPrefs.SetFloat(LastScoreKey, lastScore);

        // ハイスコアの数値を保存
        PlayerPrefs.SetFloat(HighScoreKey, highScore);

        // データを確定
        PlayerPrefs.Save();

        Debug.Log($"データを保存しました。前回: {(int)lastScore} / 最高: {(int)highScore}");
    }

    // デバッグ用：データをリセットしたい時用
    public void ResetHighScore()
    {
        PlayerPrefs.DeleteKey(HighScoreKey);
        PlayerPrefs.DeleteKey(LastScoreKey);
        highScore = 0f;
        lastScore = 0f;
    }
}