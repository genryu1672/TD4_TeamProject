using UnityEngine;

public class StageMover : MonoBehaviour
{
    void Update()
    {
        if (Time.timeScale == 0f || PlayerController.Instance == null) return;

        // 💡 プレイヤーの速度に合わせて、床そのものを手前に並行移動させる
        // Space.World を指定することで、子オブジェクト（障害物）も一緒に綺麗にくっついて移動します
        float speed = PlayerController.Instance.forwardSpeed;
        transform.Translate(Vector3.back * speed * Time.deltaTime, Space.World);
    }
}