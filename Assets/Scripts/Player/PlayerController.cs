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

    // 💡【追加】強制落下中かどうかのフラグと超重力の設定
    [HideInInspector]
    public bool isForcedFalling = false;
    [Header("強制落下時の超重力倍率")]
    public float forcedFallGravityMultiplier = 15.0f;

    private Rigidbody rb;
    private bool isGrounded = true;
    private float targetXPosition = 0f;

    // 💡【残しました】沼による減速・回復制御中かどうかを判別するフラグ
    [HideInInspector]
    public bool isSpeedControlledByMud = false;

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
        isForcedFalling = false; // 初期化
    }

    void Update()
    {
        if (Keyboard.current == null) return;

        if (Time.timeScale == 0f)
        {
            if (Keyboard.current.rKey.wasPressedThisFrame)
            {
                RetryGame();
                return;
            }

            if (Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                ReturnToTitle();
                return;
            }

            return;
        }

        // 💡【修正】強制落下中でない場合のみ、左右移動とジャンプを受け付ける
        if (!isForcedFalling)
        {
            if (Keyboard.current.aKey.wasPressedThisFrame || Keyboard.current.leftArrowKey.wasPressedThisFrame) MoveLane(false);
            else if (Keyboard.current.dKey.wasPressedThisFrame || Keyboard.current.rightArrowKey.wasPressedThisFrame) MoveLane(true);

            if (Keyboard.current.spaceKey.wasPressedThisFrame) Jump();
        }
        else
        {
            // 強制落下中は横位置をその場に固定し、前進スピードも即座に完全停止
            targetXPosition = transform.position.x;
            forwardSpeed = 0f;
        }

        // 💡 沼の制御中でなく、かつ強制落下中でない場合のみ自動スピードアップを行う
        if (!isSpeedControlledByMud && forwardSpeed < maxSpeed && !isForcedFalling)
        {
            forwardSpeed += speedIncreaseRate * (forwardSpeed * 0.2f) * Time.deltaTime;
            if (forwardSpeed > maxSpeed) forwardSpeed = maxSpeed;
        }

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

        // 💡【修正】強制落下中でない場合のみ、レーン変更のスライド移動を行う
        if (!isForcedFalling)
        {
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
        }
        else
        {
            // 強制落下中は横方向の速度を完全に「0」にして、慣性での横滑りを防ぐ
            xVelocity = 0f;
        }

        float yVelocity = rb.linearVelocity.y;

        // 💡【修正】強制落下中は超重力を適用する
        if (isForcedFalling)
        {
            yVelocity += Physics.gravity.y * forcedFallGravityMultiplier * Time.fixedDeltaTime;
        }
        else if (!isGrounded)
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

        // forwardSpeed が 0f になるので、Z軸方向（前進）もピタッとその場で停止します
        rb.linearVelocity = new Vector3(xVelocity, yVelocity, forwardSpeed);
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

    // 💡【追加】ハズレ床を踏んだ瞬間に、前進・横移動の全速度をシャットアウトして真下に落とす関数
    public void ForceStartFalling()
    {
        isForcedFalling = true;
        targetXPosition = transform.position.x; // ターゲット位置を今いる横座標に固定
        forwardSpeed = 0f;                     // 前進も完全ストップ

        if (rb != null)
        {
            // 横(X)と前進(Z)の物理的な速度をその場で完全に 0 にリセットする
            rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
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