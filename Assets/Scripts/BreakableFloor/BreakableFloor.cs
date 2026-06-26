using UnityEngine;

public class BreakableFloor : MonoBehaviour
{
    // この床がハズレ（壊れる床）かどうか
    public bool isTrap = false;

    // プレイヤーが上に乗った瞬間の判定
    private void OnTriggerEnter(Collider other)
    {
        // 触れてきたのがプレイヤー、かつハズレの床だった場合
        if (other.CompareTag("Player") && isTrap)
        {
            TriggerBreak();
        }
    }

    private void TriggerBreak()
    {
        Debug.Log("ハズレの床を踏んだ！ゲームオーバー！");

        // 💡 1. 床を消す（あるいは物理挙動で下に落とす）
        // とりあえず今回は床を非表示にして消します
        gameObject.SetActive(false);

        // 💡 2. プレイヤーをゲームオーバーにする処理を呼ぶ
        // あなたのプロジェクトにあるGameManagerなどを呼び出してください
        // 例: GameManager.Instance.GameOver();
    }
}