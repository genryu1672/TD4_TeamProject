using UnityEngine;

// 🚀 誤作動を防ぐために、このスクリプトの自爆機能を完全に無効化しました
public class BreakableFloor : MonoBehaviour
{
    public bool isTrap = false;

    private void OnTriggerEnter(Collider other)
    {
        // 何もさせない（QuizFloorControllerにすべて任せる）
    }
}