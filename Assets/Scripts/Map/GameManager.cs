using UnityEngine;
using UnityEngine.SceneManagement; // 💡 シーンのリトライ（再読み込み）に必要

public class GameManager : MonoBehaviour
{
    // 💡 外部（PlayerControllerなど）から「GameManager.Instance」で呼べるようにする
    public static GameManager Instance { get; private set; }

    [Header("生成するプレイヤーのプレファブ")]
    public GameObject playerPrefab;
    public Vector3 playerStartPosition = new Vector3(0, 1, 0);

    [Header("生成する壁（Wall）のプレファブ")]
    public GameObject wallPrefab;
    [Header("プレイヤーからどれくらい後ろに壁を出すか")]
    public float wallOffsetZ = -5f;

    [Header("関連するスクリプト")]
    public MapGenerator mapGenerator;
    public SimpleCameraFollow cameraFollow;

    [Header("ゲームオーバー時に表示するUIパネル")]
    public GameObject gameOverPanel;

    private bool isGameOver = false;

    void Awake()
    {
        // 💡 シングルトンの初期化処理
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // 1. プレイヤーを生成
        GameObject player = Instantiate(playerPrefab, playerStartPosition, Quaternion.identity);
        Transform playerTransform = player.transform;

        // 2. 壁（Wall）をプレイヤーの真後ろに生成
        if (wallPrefab != null)
        {
            Vector3 wallStartPosition = playerStartPosition + new Vector3(0, 0, wallOffsetZ);
            Instantiate(wallPrefab, wallStartPosition, Quaternion.identity);
        }

        // 3. マップ生成スクリプトにプレイヤーを教える
        if (mapGenerator != null)
        {
            mapGenerator.SetPlayer(playerTransform);
        }

        // 4. カメラ追従スクリプトにプレイヤーを教える
        if (cameraFollow != null)
        {
            cameraFollow.target = playerTransform;
        }
    }

    void Start()
    {
        // 💡 ゲーム開始時はゲームオーバー画面を隠しておく
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
    }

    // 🚀 【新規追加】プレイヤーが落ちたときに呼ばれる関数
    public void TriggerGameOver()
    {
        if (isGameOver) return; // 重複発動を防止
        isGameOver = true;

        Debug.Log("💀 ゲームオーバー！");

        // 1. スコアとハイスコアを保存
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.SaveHighScore();
        }

        // 2. プレイヤーの前進を止める
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj == null) playerObj = GameObject.Find("Player(Clone)");
        if (playerObj != null)
        {
            PlayerController playerCtrl = playerObj.GetComponent<PlayerController>();
            if (playerCtrl != null)
            {
                playerCtrl.forwardSpeed = 0f;
            }
        }

        // 3. ゲームオーバーUIを表示する
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }
    }

    // 🚀 【新規追加】UIのリトライボタンから呼ばれる関数
    public void RetryGame()
    {
        // 現在のステージを最初からやり直す
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}