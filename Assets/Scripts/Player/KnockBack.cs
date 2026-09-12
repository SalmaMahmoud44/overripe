using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class KnockBack : MonoBehaviour
{
    [Header("Knockback Settings")]
    [SerializeField] float knockbackDuration = 0.45f;

    [Header("Animation")]
    [SerializeField] Animator animator;
    [SerializeField] string knockbackAnimTrigger = "Hit";

    [Header("Behaviour")]
    [SerializeField] bool canReceiveKnockback = true;

    Rigidbody2D rb;
    Coroutine knockbackRoutine;

    bool isFalling;

    public bool IsKnockedBack { get; private set; }
    public bool CanReceiveKnockback => canReceiveKnockback;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        if (animator == null)
            animator = GetComponentInChildren<Animator>();
    }

    public void ApplyKnockback(
        Vector2 direction,
        float horizontalForce,
        float verticalForce)
    {
        if (!canReceiveKnockback || rb == null)
            return;

        if (knockbackRoutine != null)
        {
            StopCoroutine(knockbackRoutine);
            knockbackRoutine = null;
        }

        direction = direction.sqrMagnitude > 0.001f
            ? direction.normalized
            : Vector2.right;

        knockbackRoutine = StartCoroutine(
            KnockbackRoutine(
                direction,
                horizontalForce,
                verticalForce
            )
        );
    }

    public void ApplyKnockback(Vector2 direction, float force)
    {
        ApplyKnockback(direction, force, force);
    }

    IEnumerator KnockbackRoutine(
        Vector2 direction,
        float horizontalForce,
        float verticalForce)
    {
        IsKnockedBack = true;
        isFalling = false;

        // Play the ONE Hit animation
        if (animator != null)
        {
            animator.ResetTrigger(knockbackAnimTrigger);
            animator.SetTrigger(knockbackAnimTrigger);
        }

        // Apply the knockback immediately
        rb.linearVelocity = new Vector2(
            direction.x * horizontalForce,
            verticalForce
        );

        // Wait for Animation Event
        yield return new WaitUntil(() => isFalling);

        // Give the last 3 frames time to play
        yield return new WaitForSeconds(0.15f);

        IsKnockedBack = false;
        knockbackRoutine = null;
    }

    // Animation Event placed after Frame 2
    public void OnKnockbackFall()
    {
        if (!IsKnockedBack)
            return;

        isFalling = true;
    }

    public void DisableKnockback()
    {
        canReceiveKnockback = false;
        IsKnockedBack = false;
        isFalling = false;

        if (knockbackRoutine != null)
        {
            StopCoroutine(knockbackRoutine);
            knockbackRoutine = null;
        }

        if (rb != null)
            rb.linearVelocity = Vector2.zero;
    }

    public void EnableKnockback()
    {
        canReceiveKnockback = true;
    }
}