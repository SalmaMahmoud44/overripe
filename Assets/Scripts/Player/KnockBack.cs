using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class KnockBack : MonoBehaviour
{
    [Header("Knockback Settings")]
    [SerializeField] float knockbackDuration = 0.45f;

    [Header("Hit Pushback")]
    [SerializeField] float hitPushDuration = 0.12f;

    [Header("Animation")]
    [SerializeField] Animator animator;
    [SerializeField] string knockbackAnimTrigger = "Hit";

    [Header("Behaviour")]
    [SerializeField] bool canReceiveKnockback = true;

    Rigidbody2D rb;
    Coroutine knockbackRoutine;
    Coroutine hitPushRoutine;

    bool isFalling;

    public bool IsKnockedBack { get; private set; }
    public bool IsHitPushed { get; private set; }
    public bool CanReceiveKnockback => canReceiveKnockback;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        if (animator == null)
            animator = GetComponentInChildren<Animator>();
    }

    public void ApplyKnockback(Vector2 direction,float horizontalForce,float verticalForce)
    {
        if (!canReceiveKnockback || rb == null)
            return;

        StopHitPush();

        if (knockbackRoutine != null)
        {
            StopCoroutine(knockbackRoutine);
            knockbackRoutine = null;
        }

        direction = direction.sqrMagnitude > 0.001f? direction.normalized: Vector2.right;

        knockbackRoutine = StartCoroutine( KnockbackRoutine(direction,horizontalForce,verticalForce));
    }

    public void ApplyKnockback(Vector2 direction, float force)
    {
        ApplyKnockback(direction, force, force);
    }

    IEnumerator KnockbackRoutine(Vector2 direction,float horizontalForce,float verticalForce)
    {
        IsKnockedBack = true;
        isFalling = false;

        if (animator != null)
        {
            animator.ResetTrigger(knockbackAnimTrigger);
            animator.SetTrigger(knockbackAnimTrigger);
        }

        rb.linearVelocity = new Vector2(direction.x * horizontalForce, verticalForce);

        yield return new WaitUntil(() => isFalling);

        yield return new WaitForSeconds(0.15f);

        IsKnockedBack = false;
        knockbackRoutine = null;
    }
    public void ApplyHitPushback(Vector2 direction, float force)
    {
        if (!canReceiveKnockback || rb == null)
            return;

        if (knockbackRoutine != null)
        {
            StopCoroutine(knockbackRoutine);
            knockbackRoutine = null;
            IsKnockedBack = false;
        }

        StopHitPush();

        direction = direction.sqrMagnitude > 0.001f ? direction.normalized : Vector2.right;

        hitPushRoutine = StartCoroutine(HitPushRoutine(direction, force));
    }

    IEnumerator HitPushRoutine(Vector2 direction, float force)
    {
        IsHitPushed = true;

        Debug.Log("HIT PUSH START");
        float timer = 0f;

        while (timer < hitPushDuration)
        {
            rb.linearVelocity = new Vector2(direction.x * force,rb.linearVelocity.y);

            Debug.Log("Push velocity: " + rb.linearVelocity);

            timer += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }
        IsHitPushed = false;
        hitPushRoutine = null;

        Debug.Log("HIT PUSH END");
    }

    void StopHitPush()
    {
        if (hitPushRoutine != null)
        {
            StopCoroutine(hitPushRoutine);
            hitPushRoutine = null;
        }
        IsHitPushed= false;
    }
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

        StopHitPush();
        if (rb != null)
            rb.linearVelocity = Vector2.zero;
    }

    public void EnableKnockback()
    {
        canReceiveKnockback = true;
    }


}