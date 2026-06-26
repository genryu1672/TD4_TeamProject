using UnityEngine;

public class SimpleCameraFollow : MonoBehaviour
{
    [Header("追従するターゲット（プレイヤー）")]
    public Transform target;

    [Header("プレイヤーからのオフセット距離（X, Y, Z）")]
    public Vector3 offset = new Vector3(0, 3, -8);

    [Header("追従の滑らかさ（0だと追従しない、1だと即座に追従）")]
    [Range(0f, 1f)]
    public float smoothSpeed = 0.125f;

    [Header("壁が近付いたときのカメラ演出（FOV）")]
    public float normalFOV = 60f;
    public float panickedFOV = 75f;

    // 💡 調整しやすいように初期値を少し落としました（インスペクターで変更可能）
    public float fovSmoothSpeed = 2f;

    private Camera cam;
    private WallMover wallMover;

    void Start()
    {
        cam = GetComponent<Camera>();

        if (target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) target = player.transform;
        }

        wallMover = FindAnyObjectByType<WallMover>();
    }

    void LateUpdate()
    {
        if (target != null)
        {
            // --- 1. カメラ位置の追従処理 ---
            Vector3 desiredPosition = target.position + offset;
            transform.position = desiredPosition;

            // カメラの角度を固定
            transform.rotation = Quaternion.Euler(15f, 0f, 0f);

            // --- 2. 壁の接近に連動してカメラを引く演出 ---
            if (wallMover != null && cam != null)
            {
                float distanceToWall = target.position.z - wallMover.transform.position.z;

                // 💡 【ここを修正！】
                // 判定を開始する距離を「15f ➔ 30f」に広げました。
                // これにより、もっと遠く（足場1枚分先）に壁がいる段階から、ゆっくり時間をかけて引き始めます。
                float t = Mathf.InverseLerp(30f, 4f, distanceToWall);

                float targetFOV = Mathf.Lerp(normalFOV, panickedFOV, t);

                // 💡 【ここを修正！】
                // 変化がカクつかないように、前フレームのFOVから滑らかに補間させます。
                cam.fieldOfView = Mathf.MoveTowards(cam.fieldOfView, targetFOV, fovSmoothSpeed * Time.deltaTime * 10f);
            }
        }
        else
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) target = player.transform;
        }
    }
}