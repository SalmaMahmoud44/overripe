using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ArtiifactCollect : MonoBehaviour
{
    [Header("Dialogue Settings")]
    [SerializeField] private DialougeManager dialougeManager;
    [SerializeField] private Message[] messages;
    [SerializeField] private Actor[] actors;

    [Header("Transition Settings")]
    [SerializeField] private float beforeTransitionDelay = 1.5f;

    private bool collected = false;
    private bool isTransitioning = false;

    private Collider2D artifactCollider;
    private SpriteRenderer artifactRenderer;

    private void Awake()
    {
        artifactCollider = GetComponent<Collider2D>();
        artifactRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collected || !collision.CompareTag("Player"))
            return;

        collected = true;


        if (artifactCollider != null)
            artifactCollider.enabled = false;

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX(
                AudioManager.Instance.artifactCollect
            );


        if (LevelManager.Instance != null &&
            (LevelManager.Instance.curreLevel == "Banana" ||
             LevelManager.Instance.currentLevelIndex == 1))
        {
 
            if (dialougeManager == null)
            {
                GameObject dialogueObject = GameObject.Find("DialougeManager");

                if (dialogueObject != null)
                    dialougeManager =
                        dialogueObject.GetComponent<DialougeManager>();
            }


            if (dialougeManager != null)
            {
                dialougeManager.OnDialougeFinished += GoToNextLevel;
                dialougeManager.OpenDialouge(messages, actors);
            }
            else
            {
 
                StartCoroutine(NextLevel());
            }
        }
        else
        {
            StartCoroutine(NextLevel());
        }
    }

    private void GoToNextLevel()
    {

        if (dialougeManager != null)
            dialougeManager.OnDialougeFinished -= GoToNextLevel;

        StartCoroutine(NextLevel());
    }

    private IEnumerator NextLevel()
    {

        if (isTransitioning)
            yield break;

        isTransitioning = true;


        if (artifactRenderer != null)
            artifactRenderer.enabled = false;
        yield return new WaitForSeconds(beforeTransitionDelay);

        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        int nextSceneIndex = currentSceneIndex + 1;

        if (nextSceneIndex >= SceneManager.sceneCountInBuildSettings)
        {
            Debug.LogWarning("No next scene available.");
            isTransitioning = false;
            yield break;
        }

        if (SceneTransition.Instance != null)
        {
            SceneTransition.Instance.LoadScene(nextSceneIndex);
        }
        else
        {
            Debug.LogWarning(
                "SceneTransition.Instance is null. Loading scene directly."
            );

            SceneManager.LoadScene(nextSceneIndex);
        }
    }
}