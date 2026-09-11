using UnityEngine;

public class LevelMusic : MonoBehaviour
{
    private void Start()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayLevelMusic();
    }
}
