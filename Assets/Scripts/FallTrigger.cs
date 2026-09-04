using UnityEngine;

public class FallTrigger : MonoBehaviour
{
    [SerializeField] private string playerTag = "Player";
    PlayerDeath PlayerDeath;

    private void Awake()
    {
        PlayerDeath = GameObject.Find("Player").GetComponent<PlayerDeath>();
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag(playerTag))
        {
            PlayerDeath = collision.GetComponent<PlayerDeath>();
            if (PlayerDeath != null)
            {
                PlayerDeath.Die();
            }
        }   
    }
}
