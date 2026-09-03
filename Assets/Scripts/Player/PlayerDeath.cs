using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerDeath : MonoBehaviour , IDamagable
{
    RotTimer rotTimer;
    void Start()
    {
        rotTimer = GameObject.Find("RotTimerCanvas").GetComponent<RotTimer>();
    }

    void Update()
    {
        Die();
    }

    void Die()
    {
        if (rotTimer.currentTime <= 0 )
        {
            Debug.Log("Player has died due to rot timer reaching zero.");
            // Add death logic here (e.g., reload scene, show game over screen, etc.)
            Destroy(gameObject); // Example: Destroy the player object
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex); // Reload the current scene

        }
    }

    public new void TakeDamage(float damage)
    {
        // Implement player damage logic here if needed
        Debug.Log("Player took damage: " + damage);
        rotTimer.AddTime(-damage); // Assuming damage reduces times
    }

}
