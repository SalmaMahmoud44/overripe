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
    private void OnTriggerEnter2D(Collider2D collision)
    {
       if(collected || !collision.CompareTag("Player")) return;

       collected = true;
       GetComponent<Collider2D>().enabled = false; // Disable the collider to prevent further triggers
       
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
        yield return new WaitForSeconds(2f);
        Destroy(gameObject);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
    
   
}
