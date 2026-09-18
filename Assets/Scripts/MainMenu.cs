using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [Header("Credits Panel")]
    [SerializeField] private GameObject creditsPanel;

    [Header("Options Panel")]
    [SerializeField] private GameObject optionsPanel;

    [Header("Main Menu Buttons")]
    [SerializeField] private GameObject mainMenuButtons;

    private void Start()
    {
        if (creditsPanel != null)
            creditsPanel.SetActive(false);

        if (optionsPanel != null)
            optionsPanel.SetActive(false);
    }

    public void OnStartClick()
    {
        SceneManager.LoadScene("First Cutscene");
    }

    public void OnCreditsClick()
    {
        creditsPanel.SetActive(true);
        mainMenuButtons.SetActive(false);
    }

    public void OnBackClick()
    {
        creditsPanel.SetActive(false);
        optionsPanel.SetActive(false);
        mainMenuButtons.SetActive(true);
    }

    public void OnOptionsClick()
    {
        optionsPanel.SetActive(true);
        mainMenuButtons.SetActive(false);
    }

    public void OnExitClick()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
        Application.Quit();
    }
}