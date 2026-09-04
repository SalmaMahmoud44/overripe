using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerDeath : MonoBehaviour , IDamagable
{

    PlayerController playerController;
    RotTimer rotTimer;
    Animator animator;
    Rigidbody2D myrigidbody;
    bool isDead = false;
    PlayerAudio playerAudio;
    void Start()
    {
        rotTimer = GameObject.Find("RotTimerCanvas").GetComponent<RotTimer>();
        animator = GetComponentInChildren<Animator>();
        playerController = GetComponent<PlayerController>();
        myrigidbody = GetComponent<Rigidbody2D>();
        playerAudio = GetComponent<PlayerAudio>();
    }

    void Update()
    {
        if (rotTimer.currentTime <= 0 && !isDead)
        {
            Debug.Log("Player has died due to rot timer reaching zero.");
            Die();
        }
    }

   public void Die()
    {
        isDead = true;

        if(playerController != null)
            playerController.SetControlsLocked(true); // Lock player controls
        myrigidbody.linearVelocity = Vector2.zero; // Stop player movement
        myrigidbody.bodyType = RigidbodyType2D.Kinematic; // Make the player kinematic to prevent further physics interactions

        animator.SetTrigger("isDead");
        StartCoroutine(RestartLevel());
    }

    public new void TakeDamage(float damage)
    {
        Debug.Log("Player took damage: " + damage);
        rotTimer.AddTime(-damage); // Assuming damage reduces times
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Juice"))
        {
            playerAudio.PlayJuiceSound();
            Die();
        }
    }

    IEnumerator RestartLevel()
    {
        yield return new WaitForSeconds(1.5f); // Wait a few seconds before restarting
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

}
