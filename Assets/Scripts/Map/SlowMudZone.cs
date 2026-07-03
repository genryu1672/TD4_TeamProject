using UnityEngine;

public class SlowMudZone : MonoBehaviour
{
    [Header("沼に乗っている時の前進スピード")]
    public float slowForwardSpeed = 30.0f;

    private float originalSpeedCache = 15f;
    private PlayerController playerController;
    private bool isPlayerOnMud = false;

    void Start()
    {
        // シーン内のプレイヤーを安全に取得
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj == null) playerObj = GameObject.Find("Player(Clone)");
        if (playerObj != null)
        {
            playerController = playerObj.GetComponent<PlayerController>();
        }
    }

    // 🚀 クイズ床から呼ばれる（減速開始）
    public void OnPlayerStepOn()
    {
        if (playerController == null || isPlayerOnMud) return;

        isPlayerOnMud = true;

        // 現在のスピードが減速速度より速ければ、元の速度として記憶する
        if (playerController.forwardSpeed > slowForwardSpeed)
        {
            originalSpeedCache = playerController.forwardSpeed;
        }

        playerController.forwardSpeed = slowForwardSpeed;
        Debug.Log("🐌 沼：プレイヤーが乗ったので減速させました！");
    }

    // 🚀 クイズ床から呼ばれる（減速解除）
    public void OnPlayerStepOff()
    {
        if (playerController == null || !isPlayerOnMud) return;

        isPlayerOnMud = false;

        // 記憶していた元の速度に戻す
        playerController.forwardSpeed = originalSpeedCache;
        Debug.Log("🟢 沼：プレイヤーが脱出したので速度を戻しました！");
    }

    // 床が消滅するときに、万が一減速したままだったら元の速度に戻す安全装置
    void OnDestroy()
    {
        if (isPlayerOnMud && playerController != null)
        {
            playerController.forwardSpeed = originalSpeedCache;
        }
    }
}