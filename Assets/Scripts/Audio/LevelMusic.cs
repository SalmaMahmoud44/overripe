using UnityEngine;

public class LevelMusic : MonoBehaviour
{
    [SerializeField] private AudioClip customClip; 

    private void Start()
    {
        if (AudioManager.Instance == null) return;

        if (customClip != null)
            AudioManager.Instance.PlayMusic(customClip);
        else
            AudioManager.Instance.PlayLevelMusic();
    }
}