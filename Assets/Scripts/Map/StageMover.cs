using UnityEngine;

public class StageMover : MonoBehaviour
{
    // キャッシュ用（毎フレーム Find すると重いので保持する）
    private PlayerController playerController;

    void Update()
    {
        if (Time.timeScale == 0f) return;

        // 🚀 Instance が無い場合、直接シーン内から PlayerController を探す
        if (playerController == null)
        {
            if (PlayerController.Instance != null)
            {
                playerController = PlayerController.Instance;
            }
            else
            {
                // Instance がダメならタグで探す
                GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
                if (playerObj != null)
                {
                    playerController = playerObj.GetComponent<PlayerController>();
                }
            }
        }

        // それでもプレイヤーが見つからない場合は動かさない
        if (playerController == null) return;

        // 💡 プレイヤーの速度に合わせて移動
        float speed = playerController.forwardSpeed;
        transform.Translate(Vector3.back * speed * Time.deltaTime, Space.World);
    }
}