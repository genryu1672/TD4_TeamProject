using UnityEngine;
using UnityEngine.SceneManagement; // ✨シーン切り替えに必須のライブラリ

public class SceneSelector : MonoBehaviour
{
    // 指定した名前のシーンに切り替える関数
    public void ChangeScene(string sceneName)
    {
        // 💡 ゲームオーバー時などに時間が止まっている（Time.timeScale = 0）可能性があるので、1に戻しておく
        Time.timeScale = 1f;

        // シーンを読み込む
        SceneManager.LoadScene(sceneName);
    }
}