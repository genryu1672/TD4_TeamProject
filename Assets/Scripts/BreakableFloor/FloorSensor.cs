using UnityEngine;

public class FloorSensor : MonoBehaviour
{
    [Header("近づいたら消すダミーの床")]
    public GameObject dummyFloor;

    private void OnTriggerEnter(Collider other)
    {
        // プレイヤーがセンサーに触れたら
        if (other.CompareTag("Player"))
        {
            if (dummyFloor != null)
            {
                // ダミーの床を消して、下の3択床を露出させる！
                dummyFloor.SetActive(false);

                // 💡 ここで「ガシャーン」と床が割れる音やエフェクトを出すとさらに雰囲気が出ます！
            }
        }
    }
}