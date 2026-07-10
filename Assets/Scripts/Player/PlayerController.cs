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
    public float jumpForce = 12f;
    public float jumpGravityMultiplier = 3.0f;
    public float fallMultiplier = 5.0f;

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

        // 🌟【注目：ここを修正しました！】
        // Time.timeScale == 0f のとき（ゲームオーバー中など）のみ、RキーとESCキーを受け付けます
        if (Time.timeScale == 0f)
        {
            // Rキーでリトライ
            if (Keyboard.current.rKey.wasPressedThisFrame)
            {
                RetryGame();
                return;
            }

            // ESCキーでタイトルへ戻る
            if (Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                ReturnToTitle();
                return;
            }

            // ゲームオーバー中は、これ以降の移動やジャンプの処理を一切行わない
            return;
        }

        // -------------------------------------------------------------
        // 👇 ここから下は、通常プレイ中（Time.timeScale != 0f）のみ動く処理
        // -------------------------------------------------------------

        if (forwardSpeed < maxSpeed)
        {
            forwardSpeed += speedIncreaseRate * (forwardSpeed * 0.2f) * Time.deltaTime;
            if (forwardSpeed > maxSpeed) forwardSpeed = maxSpeed;
        }

        if (Keyboard.current.aKey.wasPressedThisFrame || Keyboard.current.leftArrowKey.wasPressedThisFrame) MoveLane(false);
        else if (Keyboard.current.dKey.wasPressedThisFrame || Keyboard.current.rightArrowKey.wasPressedThisFrame) MoveLane(true);

        if (Keyboard.current.spaceKey.wasPressedThisFrame) Jump();

        if (transform.position.y < -5f)
        {
            if (GameManager.Instance != null) GameManager.Instance.TriggerGameOver();
        }
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

        if (!isGrounded)
        {
            if (yVelocity < 0)
            {
                yVelocity += Physics.gravity.y * (fallMultiplier - 1) * Time.fixedDeltaTime;
            }
            else if (yVelocity > 0)
            {
                yVelocity += Physics.gravity.y * (jumpGravityMultiplier - 1) * Time.fixedDeltaTime;
            }
        }

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

    void RetryGame()
    {
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.SaveHighScore();
        }
        string currentSceneName = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(currentSceneName);
    }

    private void OnCollisionStay(Collision c) { if (c.gameObject.CompareTag("Ground")) isGrounded = true; }
    private void OnCollisionExit(Collision c) { if (c.gameObject.CompareTag("Ground")) isGrounded = false; }

    public void ResetToCenterLane()
    {
        currentLane = 1;
        targetXPosition = 0f;
        transform.position = new Vector3(0f, transform.position.y, transform.position.z);
    }

    public int GetCurrentLane()
    {
        return currentLane;
    }

    void ReturnToTitle()
    {
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.SaveHighScore();
        }
        SceneManager.LoadScene("Title");
    }
}