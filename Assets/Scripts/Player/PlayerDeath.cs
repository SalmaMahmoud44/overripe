using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerDeath : MonoBehaviour , IDamagable
{

    PlayerController playerController;
    RotTimer rotTimer;
    Animator animator;
    Rigidbody2D myrigidbody;
    AudioManager playerAudio;

    bool isDead = false;
    public bool IsDead => isDead;

    void Start()
    {
        rotTimer = GameObject.Find("RotTimerCanvas").GetComponent<RotTimer>();
        animator = GetComponentInChildren<Animator>();
        playerController = GetComponent<PlayerController>();
        myrigidbody = GetComponent<Rigidbody2D>();
        playerAudio = GetComponent<AudioManager>();
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

        KnockBack knockBack = GetComponent<KnockBack>();

        if (knockBack != null)
            knockBack.DisableKnockback();

        if (playerController != null)
            playerController.SetControlsLocked(true); 

        myrigidbody.linearVelocity = Vector2.zero; 
        myrigidbody.bodyType = RigidbodyType2D.Kinematic; 

        animator.SetTrigger("isDead");
        StartCoroutine(RestartLevel());
    }

    public new void TakeDamage(float damage)
    {
        Debug.Log("Player took damage: " + damage);
        rotTimer.AddTime(-damage); 
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Juice"))
        {
            if (AudioManager.Instance != null)
                AudioManager.Instance.PlaySFX(AudioManager.Instance.juiceClip);
            Die();
        }
    }

    IEnumerator RestartLevel()
    {
        yield return new WaitForSeconds(1.5f); 
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

  
}
