using UnityEngine;

public class TitleBGMManager : MonoBehaviour
{
    [Header("🎵 タイトル画面のBGM")]
    public AudioClip titleBGM;

    void Start()
    {
        // 💡 不死身化して戻ってきた「BGM_Manager」を探す
        GameObject playBgmObj = GameObject.Find("BGM_Manager");
        if (playBgmObj != null)
        {
            AudioSource audioSource = playBgmObj.GetComponent<AudioSource>();
            if (audioSource != null && titleBGM != null)
            {
                // 💡 もし今の曲がタイトル曲じゃないなら、タイトル曲にリセットして再生！
                if (audioSource.clip != titleBGM)
                {
                    audioSource.Stop();
                    audioSource.clip = titleBGM;
                    audioSource.loop = true;
                    audioSource.Play();
                }
            }
        }
    }
}