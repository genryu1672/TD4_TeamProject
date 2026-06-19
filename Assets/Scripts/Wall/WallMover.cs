using UnityEngine;

public class WallMover : MonoBehaviour
{
    [Header("壁の移動速度（プレイヤーより少し遅めか同じくらい）")]
    public float speed = 4.8f; // プレイヤーの速度(5)より少し遅くして、ジワジワ迫るようにします

    void Update()
    {
        // 毎フレーム、Z軸（奥）に向かって一定速度で進む
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }

    // プレイヤーが壁に飲み込まれた（触れた）ときの処理
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("ゲームオーバー！壁に追いつかれた！");
            // ここにゲームオーバーの演出やリスタートの処理を後々書きます
        }
    }
}