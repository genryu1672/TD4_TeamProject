using UnityEngine;

public class SimpleCameraFollow : MonoBehaviour
{
    [Header("追従するターゲット（プレイヤー）")]
    public Transform target; // プレイヤーのTransformをここに指定します

    [Header("プレイヤーからのオフセット距離（X, Y, Z）")]
    public Vector3 offset = new Vector3(0, 3, -8); // 初期値を少し後ろ・少し上に設定します

    [Header("追従の滑らかさ（0だと追従しない、1だと即座に追従）")]
    [Range(0f, 1f)]
    public float smoothSpeed = 0.125f; // 滑らかな移動のための速度

    void LateUpdate()
    {
        if (target != null)
        {
            // カメラの理想的な目標位置を計算します
            Vector3 desiredPosition = target.position + offset;

            // 現在の位置と目標位置の間をスムーズに補間します (Lerp)
            Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);

            // カメラの位置を更新します
            transform.position = smoothedPosition;

            // 常にプレイヤーの方向を向くようにします (オプション)
            // transform.LookAt(target);
        }
    }
}