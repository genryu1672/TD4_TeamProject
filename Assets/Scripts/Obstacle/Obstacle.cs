using UnityEngine;

public class Obstacle : MonoBehaviour
{
    private bool hasTriggered = false; // 多段ヒット防止フラグ

    private void OnTriggerEnter(Collider other)
    {
        // まだ未反応、かつ相手がプレイヤーの場合のみ処理する
        if (!hasTriggered && other.CompareTag("Player"))
        {
            hasTriggered = true; // この障害物は使用済みにする（すり抜け時のログ連打を防ぐ）

            // 壁（WallMover）を検知してペナルティを命令
            if (FindAnyObjectByType<WallMover>() is WallMover wall)
            {
                wall.HandleObstacleHit();
            }
        }
    }
}