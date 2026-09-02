using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }
    public string curreLevel { get; private set; }
    public int currentLevelIndex { get; private set; }


    private void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(this.gameObject);
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log("Entered Scene: " + scene.name);

        SetCurrentLevel();
    }
    void SetCurrentLevel()
    {
        curreLevel = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        currentLevelIndex = UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex;
        Debug.Log("Current Level: " + curreLevel + " Index: " + currentLevelIndex);
    }

}
