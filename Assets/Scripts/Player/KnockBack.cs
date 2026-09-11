using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class KnockBack : MonoBehaviour
{
    [Header("Knockback Settings")]
    [SerializeField] float knockbackDuration = 0.2f;
    [SerializeField] float defaultVerticalBoost = 2f;
    [SerializeField] float decayRate = 5f;

    [Header("Animation")]
    [SerializeField] Animator animator;
    [SerializeField] string knockbackAnimTrigger = "Hit";

    [Header("Behaviour")]
    [SerializeField] bool canReceiveKnockback = true;

    Rigidbody2D rb;
    Coroutine knockbackRoutine;

    public bool IsKnockedBack { get; private set; }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        if (animator == null)
            animator = GetComponentInChildren<Animator>();
    }

    public void ApplyKnockback(Vector2 direction, float force)
    {
        ApplyKnockback(direction, force, defaultVerticalBoost);
    }

    public void ApplyKnockback(Vector2 direction,float horizontalForce,float verticalForce)
    {
        if (!canReceiveKnockback || rb == null)
            return;

        direction = direction.sqrMagnitude > 0.001f? direction.normalized: Vector2.right;

        if (knockbackRoutine != null)
        {
            StopCoroutine(knockbackRoutine);
            knockbackRoutine = null;
        }
         IsKnockedBack = false;

        knockbackRoutine = StartCoroutine( KnockbackRoutine(direction,horizontalForce,verticalForce));
    }

    public void ApplyKnockbackFromSource(Vector2 sourcePosition,float force)
    {
        Vector2 direction =(Vector2)transform.position - sourcePosition;

        ApplyKnockback(direction, force);
    }
    public void PlayHitAnimation()
    {
        if (animator != null &&
            !string.IsNullOrEmpty(knockbackAnimTrigger))
        {
            animator.SetTrigger(knockbackAnimTrigger);
        }
    }

    IEnumerator KnockbackRoutine(Vector2 direction,float horizontalForce,float verticalForce)
    {
        IsKnockedBack = true;

        PlayHitAnimation();

        Vector2 velocity =direction * horizontalForce +Vector2.up * verticalForce;

        float elapsed = 0f;

        while (elapsed < knockbackDuration)
        {
            rb.linearVelocity = velocity;

            velocity = Vector2.Lerp(velocity, Vector2.zero,decayRate * Time.deltaTime);

            elapsed += Time.deltaTime;

            yield return null;
        }

        rb.linearVelocity = Vector2.zero;

        IsKnockedBack = false;
        knockbackRoutine = null;
    }
}
