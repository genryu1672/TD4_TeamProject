using UnityEngine;
using UnityEngine.InputSystem; // ← 新しいInput Systemを使うために追加

public class PlayerController : MonoBehaviour
{
    [Header("移動速度")]
    public float forwardSpeed = 10f; // 自動で奥に進むスピード
    public float laneSpeed = 8f;    // 左右に動くスピード

    [Header("ジャンプ力")]
    public float jumpForce = 8f;

    private Rigidbody rb;
    private int jumpCount = 0;
    private float horizontalInput;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        // --- 新しいInput Systemでのキー入力取得 ---

        // 左右の入力（A/Dキー、または←/→キー）を取得
        horizontalInput = 0f;
        if (Keyboard.current != null)
        {
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed)
            {
                horizontalInput = -1f;
            }
            else if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed)
            {
                horizontalInput = 1f;
            }

            // スペースキーが「このフレームで押されたか」を判定（GetKeyDownの代わり）
            if (Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                Jump();
            }
        }
    }

    void FixedUpdate()
    {
        // 現在の速度を取得
        Vector3 velocity = rb.linearVelocity;

        // 1. 左右の移動速度を設定
        velocity.x = horizontalInput * laneSpeed;

        // 2. 自動で奥（Z軸方向）に前進し続ける
        velocity.z = forwardSpeed;

        // 速度をRigidbodyに適用
        rb.linearVelocity = velocity;

        // 3. チャリの向きを進行方向に向ける
        if (velocity != Vector3.zero)
        {
            Vector3 direction = new Vector3(velocity.x, 0, velocity.z);
            if (direction != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(direction);
            }
        }
    }

    void Jump()
    {
        if (jumpCount < 2)
        {
            Vector3 velocity = rb.linearVelocity;
            velocity.y = jumpForce;
            rb.linearVelocity = velocity;

            jumpCount++;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground")) // ←元からある地面の判定（※画像エラーにあるように、地面オブジェクトのTagを「Ground」に設定しておいてください！）
        {
            jumpCount = 0;
        }
    }
}