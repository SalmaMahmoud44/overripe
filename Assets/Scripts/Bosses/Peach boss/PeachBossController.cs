using System.Collections;
using UnityEngine;

public class PeachBossController : MonoBehaviour
{
    public enum BossState { Idle, RollIntro, Rolling, RollOutro }

    [Header("References")]
    [SerializeField] Transform player;
    [SerializeField] Animator animator;
    [SerializeField] Rigidbody2D peachRigidbody2D;
    [SerializeField] Collider2D roomBounds;

    [Header("Phase 1 - Roll Settings")]
    [SerializeField] float rollIntroDelay = 1f;
    [SerializeField] float rollSpeed = 8f;
    [SerializeField] int bounceCount = 4;
    [SerializeField] float rollOutroDuration = 0.4f; // TODO: نستبدلها بطول الـ outro clip الفعلي

    [Header("Animator Triggers")]
    [SerializeField] string rollIntroTrigger = "RollIntro";
    [SerializeField] string rollLoopBool = "IsRolling";
    [SerializeField] string rollOutroTrigger = "RollOutro";

    public BossState currentState { get; private set; } = BossState.Idle;

    bool fightStarted = false;
    int rollDirection = 1;

    void Awake()
    {
        if (animator == null)
            animator = GetComponent<Animator>();

        if (peachRigidbody2D == null)
            peachRigidbody2D = GetComponent<Rigidbody2D>();
    }

    public void BeginFight()
    {
        if (fightStarted) return;

        fightStarted = true;
        rollDirection = (player.position.x > transform.position.x) ? 1 : -1;
        StartCoroutine(RollIntroSequence());
    }

    IEnumerator RollIntroSequence()
    {
        currentState = BossState.Idle;

        yield return new WaitForSeconds(rollIntroDelay);

        currentState = BossState.RollIntro;

        if (animator != null)
            animator.SetTrigger(rollIntroTrigger);

        Debug.Log("Peach Boss: Roll Intro started");

        // TODO: نستبدل الرقم دا بطول الـ intro clip الفعلي
        yield return new WaitForSeconds(0.3f);

        yield return StartCoroutine(RollSequence());
    }

    IEnumerator RollSequence()
    {
        currentState = BossState.Rolling;

        if (animator != null)
            animator.SetBool(rollLoopBool, true);

        Debug.Log("Peach Boss: Rolling started");

        int bounces = 0;

        while (bounces < bounceCount)
        {
            bool didBounce = Roll();
            if (didBounce)
                bounces++;

            yield return null;
        }

        peachRigidbody2D.linearVelocity = Vector2.zero;

        if (animator != null)
            animator.SetBool(rollLoopBool, false);

        currentState = BossState.RollOutro;

        if (animator != null)
            animator.SetTrigger(rollOutroTrigger);

        Debug.Log("Peach Boss: Roll Outro started");

        yield return new WaitForSeconds(rollOutroDuration);

        FaceDirection(player.position.x > transform.position.x ? 1 : -1);

        currentState = BossState.Idle;

        Debug.Log("Peach Boss: Rolling finished, now Idle");
    }

    bool Roll()
    {
        if (peachRigidbody2D == null || roomBounds == null) return false;

        peachRigidbody2D.linearVelocity = new Vector2(rollDirection * rollSpeed, peachRigidbody2D.linearVelocity.y);

        Bounds bounds = roomBounds.bounds;
        bool bounced = false;

        if (transform.position.x >= bounds.max.x)
        {
            rollDirection = -1;
            bounced = true;
        }
        else if (transform.position.x <= bounds.min.x)
        {
            rollDirection = 1;
            bounced = true;
        }

        return bounced;
    }

    void FaceDirection(int direction)
    {
        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * direction;
        transform.localScale = scale;
    }
}