using UnityEngine;

public class FloorSensor : MonoBehaviour
{
    public GameObject dummyFloor;
    private bool isTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (isTriggered) return;

        if (other.CompareTag("Player"))
        {
            isTriggered = true;

            // ダミーを消す
            if (dummyFloor != null) dummyFloor.SetActive(false);

            // 親についているクイズコントローラーを見つけて起動！
            QuizFloorController quiz = transform.parent.GetComponent<QuizFloorController>();
            if (quiz != null)
            {
                quiz.StartQuiz(other.gameObject);
            }
        }
    }
}