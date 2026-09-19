using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;

public class FinalCutsceneController : MonoBehaviour
{
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private string nextScene = "Main Menu";
    [SerializeField] private int nexySceneIndex = 0;

    private void Start()
    {
        videoPlayer.loopPointReached += OnVideoFinished;
        videoPlayer.Play();
    }

    private void OnVideoFinished(VideoPlayer vp)
    {
        SceneManager.LoadScene(nexySceneIndex);
    }
}