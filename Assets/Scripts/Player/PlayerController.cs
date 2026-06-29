using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance { get; private set; }

    [Header("移動速度")]
    public float forwardSpeed = 10f;
    public float laneShiftSpeed = 25f;

    [Header("スピード加速設定")]
    public float speedIncreaseRate = 0.1f;
    public float maxSpeed = 40f;

    [Header("レーン設定")]
    public float laneDistance = 3.0f;
    private int currentLane = 1;

    [Header("ジャンプ設定")]
    public float jumpForce = 12f;          // 💡 少し高さを出すために12に調整（インスペクターで変更可）
    public float jumpGravityMultiplier = 3.0f; // 💡 ジャンプ上昇中の重力倍率（フワフワ防止）
    public float fallMultiplier = 5.0f;     // 💡 落下中の重力倍率（サッと降りる）

    private Rigidbody rb;
    private bool isGrounded = true;
    private float targetXPosition = 0f;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        Time.timeScale = 1f;
        rb = GetComponent<Rigidbody>();
        targetXPosition = 0f;
    }

    void Update()
    {
        if (Keyboard.current == null) return;

        // 💡 【リトライ機能を追加】
        // Time.timeScaleが0（ゲームオーバーやポーズ中）でも関係なく、Rキーが押されたらリトライします
        if (Keyboard.current.rKey.wasPressedThisFrame)
        {
            RetryGame();
            return; // シーンが切り替わるので、以降の処理はスキップ
        }

        if (Time.timeScale == 0f) return;

        if (forwardSpeed < maxSpeed)
        {
            // 💡 【加速力アップ！】
            forwardSpeed += speedIncreaseRate * (forwardSpeed * 0.2f) * Time.deltaTime;

            if (forwardSpeed > maxSpeed) forwardSpeed = maxSpeed;
        }

        if (Keyboard.current.aKey.wasPressedThisFrame || Keyboard.current.leftArrowKey.wasPressedThisFrame) MoveLane(false);
        else if (Keyboard.current.dKey.wasPressedThisFrame || Keyboard.current.rightArrowKey.wasPressedThisFrame) MoveLane(true);

        if (Keyboard.current.spaceKey.wasPressedThisFrame) Jump();
    }

    void FixedUpdate()
    {
        if (Time.timeScale == 0f) return;

        float currentX = transform.position.x;
        float xVelocity = 0f;

        if (Mathf.Abs(currentX - targetXPosition) > 0.01f)
        {
            float directionX = Mathf.Sign(targetXPosition - currentX);
            xVelocity = directionX * laneShiftSpeed;
            float nextX = currentX + xVelocity * Time.fixedDeltaTime;
            if ((directionX > 0 && nextX > targetXPosition) || (directionX < 0 && nextX < targetXPosition))
            {
                xVelocity = (targetXPosition - currentX) / Time.fixedDeltaTime;
            }
        }

        float yVelocity = rb.linearVelocity.y;

        // 💡 【ジャンプの挙動をクッキリ修正】
        if (!isGrounded)
        {
            if (yVelocity < 0)
            {
                // ① 頂点から落ちるとき（一瞬で着地させる）
                yVelocity += Physics.gravity.y * (fallMultiplier - 1) * Time.fixedDeltaTime;
            }
            else if (yVelocity > 0)
            {
                // ② ボタンを押して上昇中（ここにも重力をかけてフワフワ感をなくす）
                yVelocity += Physics.gravity.y * (jumpGravityMultiplier - 1) * Time.fixedDeltaTime;
            }
        }

        // Z軸は完全に0で固定
        rb.linearVelocity = new Vector3(xVelocity, yVelocity, 0f);
        transform.rotation = Quaternion.identity;
    }

    void MoveLane(bool goingRight)
    {
        if (goingRight) { if (currentLane < 2) currentLane++; }
        else { if (currentLane > 0) currentLane--; }
        targetXPosition = (currentLane - 1) * laneDistance;
    }

    void Jump()
    {
        if (isGrounded)
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, jumpForce, 0f);
            isGrounded = false;
        }
    }

    // 💡 【リトライの実行処理】
    void RetryGame()
    {
        // 🚀 シーンが切り替わる（リセットされる）直前に、ハイスコアを保存する命令を出す
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.SaveHighScore();
        }

        // 現在開いているシーンの名前を取得
        string currentSceneName = SceneManager.GetActiveScene().name;
        // 同じシーンを最初からロードし直す
        SceneManager.LoadScene(currentSceneName);
    }

    private void OnCollisionStay(Collision c) { if (c.gameObject.CompareTag("Ground")) isGrounded = true; }
    private void OnCollisionExit(Collision c) { if (c.gameObject.CompareTag("Ground")) isGrounded = false; }

    // 🚀 クイズ開始時にレーン位置を中央（1）に強制リセットする関数
    public void ResetToCenterLane()
    {
        currentLane = 1;
        targetXPosition = 0f;
        transform.position = new Vector3(0f, transform.position.y, transform.position.z);
    }

    // 🚀 クイズ判定時に、今どのレーンにいるかをクイズ側に教える関数 (0:左, 1:中央, 2:右)
    public int GetCurrentLane()
    {
        return currentLane;
    }
}