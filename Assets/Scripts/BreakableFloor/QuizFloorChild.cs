using UnityEngine;

public class QuizFloorChild : MonoBehaviour
{
    private QuizFloorController parentController;
    private bool hasReported = false;

    void Start()
    {
        parentController = GetComponentInParent<QuizFloorController>();
    }

    void OnEnable()
    {
        hasReported = false;
    }

    // 💡【大修正】Trigger（センサー）ではなく、Collision（物理的な衝突）で検知する
    void OnCollisionEnter(Collision collision)
    {
        if (hasReported) return;

        // ぶつかってきたオブジェクトのタグが「Player」なら親に報告
        if (collision.gameObject.CompareTag("Player") && parentController != null)
        {
            hasReported = true;
            parentController.OnPlayerEnterFloor(this.gameObject);
        }
    }
}