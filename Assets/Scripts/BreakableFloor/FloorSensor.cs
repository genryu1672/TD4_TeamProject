using UnityEngine;

public class FloorSensor : MonoBehaviour
{
    public GameObject dummyFloor;
    private bool isTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        // 🚀【超重要】触れてきた相手のタグが「Player」じゃなければ1ミリも処理せず無視する！
        if (!other.CompareTag("Player")) return;

        // すでに起動済みなら無視
        if (isTriggered) return;
        isTriggered = true;

        // ダミーを消す（使っていなければスルーされます）
        if (dummyFloor != null) dummyFloor.SetActive(false);

        // 親、または自分自身から確実にQuizFloorControllerを見つける
        QuizFloorController quiz = GetComponentInParent<QuizFloorController>();
        if (quiz != null)
        {
            // クイズを起動
            quiz.StartQuiz(other.gameObject);
        }
    }
}