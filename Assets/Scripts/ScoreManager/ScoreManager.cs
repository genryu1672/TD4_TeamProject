using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    [Header("現在のスコア")]
    public float currentScore = 0f;

    [Header("ハイスコア")]
    public float highScore = 0f;

    // スコアが保存されているか確認するためのキー名
    private const string HighScoreKey = "RunGame_HighScore";

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        // 💡 ゲーム開始時に、パソコンに保存されているハイスコアを読み込む
        // まだ一度も保存されていない（初回プレイ）場合は 0 が入ります
        highScore = PlayerPrefs.GetFloat(HighScoreKey, 0f);
    }

    void Update()
    {
        if (Time.timeScale == 0f || PlayerController.Instance == null) return;

        // 💡 プレイヤーの現在の速度（forwardSpeed）に合わせて、スコアを毎フレーム加算
        // これで「速く走るほどスコアが早く増える」ようになります
        currentScore += PlayerController.Instance.forwardSpeed * Time.deltaTime;

        // 現在のスコアがハイスコアを超えたら、リアルタイムにハイスコアも更新
        if (currentScore > highScore)
        {
            highScore = currentScore;
        }
    }

    // 💡 障害物に当たった時（ゲームオーバー時）にこの関数を呼び出して保存する！
    public void SaveHighScore()
    {
        // 現在のハイスコアの数値をパソコンに保存
        PlayerPrefs.SetFloat(HighScoreKey, highScore);
        PlayerPrefs.Save(); // 念のため即座に書き込み確定
        Debug.Log($"ハイスコアを保存しました: {(int)highScore}");
    }

    // 💡 デバッグ用：ハイスコアをリセットしたい時はこれをどこかで呼ぶ
    public void ResetHighScore()
    {
        PlayerPrefs.DeleteKey(HighScoreKey);
        highScore = 0f;
    }
}