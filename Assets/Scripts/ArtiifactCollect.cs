using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ArtiifactCollect : MonoBehaviour
{
    [Header("Dialouge Settings")]
    [SerializeField] private DialougeManager dialougeManager;
    [SerializeField] private Message[] messages;
    [SerializeField] private Actor[] actors;

    bool collected = false;

    Collider2D artifactCollider;
    SpriteRenderer artifactRenderer;

    private void Awake()
    {
        artifactCollider = GetComponent<Collider2D>();
        artifactRenderer = GetComponentInChildren<SpriteRenderer>();
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collected || !collision.CompareTag("Player")) return;

        collected = true;
        
       artifactCollider.enabled = false;

        if (AudioManager.Instance != null)AudioManager.Instance.PlaySFX(AudioManager.Instance.artifactCollect);

        if(LevelManager.Instance.curreLevel == "Banana" || LevelManager.Instance.currentLevelIndex == 1)
        {
            if (dialougeManager == null)
                dialougeManager = GameObject.Find("DialougeManager").GetComponent<DialougeManager>();
            dialougeManager.OnDialougeFinished += GoToNextLevel;
            dialougeManager.OpenDialouge(messages, actors);
        }
        else
        {
            StartCoroutine(NextLevel());
        }
       
    }

    private void GoToNextLevel()
    {
        dialougeManager.OnDialougeFinished-=GoToNextLevel;
        StartCoroutine(NextLevel());
    }
    IEnumerator NextLevel()
    {
        if (artifactRenderer != null)
        {
            artifactRenderer.enabled = false; 
        }
        yield return new WaitForSeconds(1.5f);
        Destroy(gameObject);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
    
   
}
