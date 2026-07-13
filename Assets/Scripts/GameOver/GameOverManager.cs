using System.Collections;
using UnityEngine;
using TMPro;

public class GameOverManager : MonoBehaviour
{
    [Header("ゲームオーバー画面のUIパネル")]
    public GameObject gameOverPanel;

    [Header("ゲームオーバー画面のタイム用TMPテキスト")]
    public TextMeshProUGUI timeText;

    // 🎵 BGM追加：インスペクターから曲を設定できるようにする変数
    [Header("🎵 ゲームプレイ中のBGM")]
    public AudioClip gamePlayBGM;

    [Header("🎵 ゲームオーバー時のBGM")]
    public AudioClip gameOverBGM;

    private float survivalTime = 0f;
    private bool isGameOver = false;

    void Start()
    {
        survivalTime = 0f;
        isGameOver = false;

        if (gameOverPanel == null)
        {
            GameObject canvas = GameObject.Find("Canvas");
            if (canvas != null)
            {
                Transform panelTransform = canvas.transform.Find("GameOverPanel");
                if (panelTransform != null) gameOverPanel = panelTransform.gameObject;
            }
        }

        if (gameOverPanel != null && timeText == null)
        {
            Transform tTextTransform = gameOverPanel.transform.Find("TimeText");
            if (tTextTransform != null) timeText = tTextTransform.GetComponent<TextMeshProUGUI>();
        }

        if (gameOverPanel != null) gameOverPanel.SetActive(false);

        // 🌟【新機能】タイトルから引き継いだ不死身のBGMマネージャーをプレイ中の曲に変える
        GameObject playBgmObj = GameObject.Find("BGM_Manager");
        if (playBgmObj != null)
        {
            AudioSource audioSource = playBgmObj.GetComponent<AudioSource>();
            if (audioSource != null && gamePlayBGM != null)
            {
                // 💡 現在の曲がプレイBGMと違う、または「まだ何も曲がセットされていない(null)」なら再生する
                if (audioSource.clip != gamePlayBGM || audioSource.clip == null)
                {
                    audioSource.Stop();             // タイトル曲（または前の曲）を止める
                    audioSource.clip = gamePlayBGM; // プレイ中の曲をセット
                    audioSource.loop = true;        // ループ再生にする
                    audioSource.Play();             // 確実に再生スタート！
                }
            }
        }
    }

    void Update()
    {
        if (!isGameOver && Time.timeScale > 0f)
        {
            survivalTime += Time.deltaTime;
        }
    }

    public void TriggerGameOver()
    {
        if (isGameOver) return;
        isGameOver = true;

        Debug.Log("【ゲームオーバー】一括管理システムが発動しました。");

        // 🎵 BGM切り替え：ゲームオーバーになったら曲を切り替える
        GameObject playBgmObj = GameObject.Find("BGM_Manager");
        if (playBgmObj != null)
        {
            AudioSource audioSource = playBgmObj.GetComponent<AudioSource>();
            if (audioSource != null && gameOverBGM != null)
            {
                audioSource.Stop();             // プレイBGMを停止
                audioSource.clip = gameOverBGM; // ゲームオーバーBGMをセット
                audioSource.loop = true;        // ループ再生にする
                audioSource.Play();             // 再生！
            }
        }

        // 1. タイムテキストの文字を更新し、中央揃えを強制する
        if (timeText != null)
        {
            timeText.text = "TIME: " + survivalTime.ToString("F2") + "s";
            timeText.alignment = TextAlignmentOptions.Center;

            RectTransform timeRect = timeText.GetComponent<RectTransform>();
            if (timeRect != null)
            {
                timeRect.anchoredPosition = new Vector2(0f, -150f);
                timeRect.anchorMin = new Vector2(0.5f, 0.5f);
                timeRect.anchorMax = new Vector2(0.5f, 0.5f);
                timeRect.pivot = new Vector2(0.5f, 0.5f);
            }
        }

        // 2. スコアテキストをお引っ越しさせて中央揃えにする
        GameObject currentScoreObj = GameObject.Find("CurrentScoreText");
        if (currentScoreObj != null && gameOverPanel != null)
        {
            currentScoreObj.transform.SetParent(gameOverPanel.transform);

            RectTransform scoreRect = currentScoreObj.GetComponent<RectTransform>();
            if (scoreRect != null)
            {
                scoreRect.anchoredPosition = new Vector2(0f, -340f);
                scoreRect.anchorMin = new Vector2(0.5f, 0.5f);
                scoreRect.anchorMax = new Vector2(0.5f, 0.5f);
                scoreRect.pivot = new Vector2(0.5f, 0.5f);
            }

            TextMeshProUGUI scoreTMP = currentScoreObj.GetComponent<TextMeshProUGUI>();
            if (scoreTMP != null)
            {
                scoreTMP.alignment = TextAlignmentOptions.Center;
            }
        }

        if (gameOverPanel != null) gameOverPanel.SetActive(true);
        Time.timeScale = 0f;
    }
}