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
    public float fovSmoothSpeed = 5f;

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
            // ⬇️ 【ここを書き換えます！】 ⬇️
            // --- 1. カメラ位置の追従処理 ---
            // X, Y, Z すべての軸の滑らかさ（Lerp）を廃止し、プレイヤーに完全同期させます。
            Vector3 desiredPosition = target.position + offset;

            // これにより、時速180km（速度50）でもカメラが1フレームも遅れずに真後ろに固定されます！
            transform.position = desiredPosition;


            // カメラの角度を完全にまっすぐ（正面）でロックします。角度は一切ガタつきません。
            transform.rotation = Quaternion.Euler(15f, 0f, 0f);


            // --- 2. 壁の接近に連動してカメラを引く演出 ---
            if (wallMover != null && cam != null)
            {
                float distanceToWall = target.position.z - wallMover.transform.position.z;
                float t = Mathf.InverseLerp(15f, 4f, distanceToWall);
                float targetFOV = Mathf.Lerp(normalFOV, panickedFOV, t);
                cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, targetFOV, Time.deltaTime * fovSmoothSpeed);
            }
        }
        else
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) target = player.transform;
        }
    }
}