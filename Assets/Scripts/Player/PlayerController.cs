using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    [Header("移動速度")]
    public float forwardSpeed = 10f;
    public float laneShiftSpeed = 25f;

    [Header("スピード加速設定")]
    public float speedIncreaseRate = 0.1f;

    // ⬇️ 【ここを修正！】コメントアウトを解除して、インスペクターから上限を設定できるように復活させました
    public float maxSpeed = 40f;

    [Header("レーン設定")]
    public float laneDistance = 3.0f;
    private int currentLane = 1;      // 0: 左, 1: 中央, 2: 右

    [Header("ジャンプ設定")]
    public float jumpForce = 10f;
    public float fallMultiplier = 4.5f;

    private Rigidbody rb;
    private bool isGrounded = true;
    private float targetXPosition = 0f;

    void Start()
    {
        Time.timeScale = 1f;
        rb = GetComponent<Rigidbody>();
        targetXPosition = 0f;
    }

    void Update()
    {
        if (Keyboard.current == null) return;

        // リトライの入力判定
        if (Time.timeScale == 0f && Keyboard.current.rKey.wasPressedThisFrame)
        {
            RestartGame();
            return;
        }

        // ゲームオーバー等で時間が止まっているなら処理しない
        if (Time.timeScale == 0f) return;

        // forwardSpeed が maxSpeed（40や50）より小さい時だけ加速する
        if (forwardSpeed < maxSpeed)
        {
            forwardSpeed += speedIncreaseRate * Time.deltaTime;

            // もし超えてしまったら、最大値でピタッと固定するブレーキ
            if (forwardSpeed > maxSpeed)
            {
                forwardSpeed = maxSpeed;
            }
        }

        // --- レーン移動の入力 ---
        if (Keyboard.current.aKey.wasPressedThisFrame || Keyboard.current.leftArrowKey.wasPressedThisFrame)
        {
            MoveLane(false); // 左へ
        }
        else if (Keyboard.current.dKey.wasPressedThisFrame || Keyboard.current.rightArrowKey.wasPressedThisFrame)
        {
            MoveLane(true);  // 右へ
        }

        // --- ジャンプの入力 ---
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            Jump();
        }
    }

    void FixedUpdate()
    {
        if (Time.timeScale == 0f) return;

        // --- 1. 目標のX座標への移動速度を計算 ---
        float currentX = transform.position.x;
        float xVelocity = 0f;

        if (Mathf.Abs(currentX - targetXPosition) > 0.01f)
        {
            float directionX = Mathf.Sign(targetXPosition - currentX);
            xVelocity = directionX * laneShiftSpeed;

            // 行き過ぎ防止のブレーキ処理
            float nextX = currentX + xVelocity * Time.fixedDeltaTime;
            if ((directionX > 0 && nextX > targetXPosition) || (directionX < 0 && nextX < targetXPosition))
            {
                xVelocity = (targetXPosition - currentX) / Time.fixedDeltaTime;
            }
        }

        // --- 2. 落下重力の計算 ---
        float yVelocity = rb.linearVelocity.y;
        if (!isGrounded && yVelocity < 0)
        {
            yVelocity -= fallMultiplier * Time.fixedDeltaTime;
        }

        // --- 3. すべての速度（X, Y, Z）を1つのVector3にまとめて一発で適用 ---
        Vector3 finalVelocity = new Vector3(xVelocity, yVelocity, forwardSpeed);
        rb.linearVelocity = finalVelocity;

        // プレイヤーの角度は常に正面でロック
        transform.rotation = Quaternion.identity;
    }

    void MoveLane(bool goingRight)
    {
        if (goingRight)
        {
            if (currentLane < 2) currentLane++;
        }
        else
        {
            if (currentLane > 0) currentLane--;
        }

        targetXPosition = (currentLane - 1) * laneDistance;
    }

    void Jump()
    {
        if (isGrounded)
        {
            Vector3 velocity = rb.linearVelocity;
            velocity.y = jumpForce;
            rb.linearVelocity = velocity;

            isGrounded = false;
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = false;
        }
    }

    void RestartGame()
    {
        string currentSceneName = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(currentSceneName);
    }
}