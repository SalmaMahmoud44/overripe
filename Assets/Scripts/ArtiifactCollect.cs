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
       
        if (dialougeManager == null)
            dialougeManager = GameObject.Find("DialougeManager").GetComponent<DialougeManager>();
        dialougeManager.OnDialougeFinished += GoToNextLevel;
        dialougeManager.OpenDialouge(messages, actors);
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
