using UnityEngine;

public class DontDestroyBGM : MonoBehaviour
{
    private static DontDestroyBGM instance;

    void Awake()
    {
        // すでに同じ名前のBGMマネージャーがいる場合は、重複しないように自分を消す
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        // 🌟 このオブジェクトをシーン切り替えで破壊されないようにする！
        DontDestroyOnLoad(gameObject);
    }
}