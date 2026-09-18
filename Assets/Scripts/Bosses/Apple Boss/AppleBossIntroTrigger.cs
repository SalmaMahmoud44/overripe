using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class AppleBossIntroTrigger : MonoBehaviour
{
    [SerializeField] private AppleBossIntro appleBossIntro;

    private bool triggered;

    private void Reset()
    {
        Collider2D col = GetComponent<Collider2D>();

        if (col != null)
            col.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (triggered)
            return;

        if (!collision.CompareTag("Player"))
            return;

        triggered = true;

        if (appleBossIntro != null)
        {
            appleBossIntro.StartIntro();
        }
    }
}
