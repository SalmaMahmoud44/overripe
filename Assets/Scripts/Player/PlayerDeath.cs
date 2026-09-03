using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerDeath : MonoBehaviour , IDamagable
{
    RotTimer rotTimer;
    Animator animator;
    bool isDead = false;
    void Start()
    {
        rotTimer = GameObject.Find("RotTimerCanvas").GetComponent<RotTimer>();
        animator = GetComponentInChildren<Animator>();
    }

    void Update()
    {
        if (rotTimer.currentTime <= 0 && !isDead)
        {
            Debug.Log("Player has died due to rot timer reaching zero.");
            Die();
        }
    }

    void Die()
    {
        isDead = true;
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
           Die();
        }
    }

    IEnumerator RestartLevel()
    {
        yield return new WaitForSeconds(1.5f); // Wait a few seconds before restarting
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

}
