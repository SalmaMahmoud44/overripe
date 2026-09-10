using System.Collections;
using UnityEngine;

public class TripleBLaser : MonoBehaviour
{
    [Header("Laser Settings")]
    [SerializeField] Transform laserSpawnPoint;
    [SerializeField] LineRenderer laserLine;

    [SerializeField] float laserRange = 8f;
    [SerializeField] float laserCooldown = 3f;
    [SerializeField] float laserDamage = 20f;
    [SerializeField] float laserTime = 0.15f;
    [SerializeField] float laserWidth = 0.2f;

    [SerializeField] LayerMask[] laserHitLayers;

    [SerializeField] RotTimer rotTimer;
    [SerializeField] float laserTimeCost = 3f;

    [Header("Attack Movement")]
    [SerializeField] Transform attackPos;

    [SerializeField] float moveForwardTime = 0.15f;
    [SerializeField] float moveBackTime = 0.15f;

    [Header("Animation")]
    [SerializeField] Animator animator;
    [SerializeField] string LaserTrigger = "StartLaser";
    [SerializeField] string floatingClipName = "TripleBFloat";
    
    private float laserTimer;
    private float laserElapsed;

    private Vector3 followPositionBeforeAttack;
    private Vector2 currentLaserDir;

    private Rigidbody2D rigidbody2;

    private bool laserFired;
    private bool returning;
    private bool animationHeld;
    public bool IsAttacking { get; private set; }

    private float staminaProgress = 1f;

    public float StaminaProgress => staminaProgress;

    public bool CanShoot
    {
        get
        { 
            return laserTimer <= 0f && !IsAttacking;
        }

    }

    private void Awake()
    {
        if(animator == null) 
             animator = GetComponentInChildren<Animator>();

        rigidbody2 = GetComponent<Rigidbody2D>();
    }
    private void Start()
    {
        if(laserLine != null)
            laserLine.enabled = false;
    }
    void Update()
    {
        if(laserTimer > 0f)
        {
            laserTimer -= Time.deltaTime;
            if(laserTimer < 0f)
                laserTimer = 0f;

            if(laserCooldown > 0f)
                staminaProgress = 1f - (laserTimer / laserCooldown);
        }
    }

    public void ShootLaser(Vector2 direction)
    {

        if(!CanShoot) return;


        if (laserSpawnPoint == null || laserLine == null)
        {
            Debug.LogWarning("Laser Spawn Point or Laser Line is not assigned.");
            return;
        }

        if (attackPos == null)
        {
            return;
        }

        currentLaserDir = direction.normalized;

        followPositionBeforeAttack = transform.position;


        StartCoroutine(LaserAttack());

        
    }

    private IEnumerator LaserAttack()
    {
        IsAttacking = true;
        returning = false;
        laserFired = false;
        animationHeld = false;

        staminaProgress = 1f;


        if (rigidbody2 != null) 
            rigidbody2.linearVelocity = Vector2.zero;


        Vector3 startPosition = transform.position;

       
        float elapsed = 0f;

        while (elapsed < moveForwardTime)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(elapsed / moveForwardTime);

            transform.position = Vector3.Lerp(startPosition, attackPos.position, t);

            yield return null;
        }

        transform.position = attackPos.position;

        if (rigidbody2 != null)
            rigidbody2.linearVelocity = Vector2.zero;

        if (animator != null)
        {
            animator.speed = 1f;
            animator.ResetTrigger(LaserTrigger);
            animator.SetTrigger(LaserTrigger);
        }

        while (!laserFired)
        {
            yield return null;
        }

        laserElapsed = 0f;
        while (laserElapsed < laserTime)
        {
            laserElapsed += Time.deltaTime;
            staminaProgress = 1f - Mathf.Clamp01(laserElapsed / laserTime);
            yield return null;
        }
        if(laserLine != null)
        {
            laserLine.enabled = false;
        }

        laserFired = false;
        laserElapsed = 0f;

        if (animator != null)
            animator.speed = 1f;

        while (!animationHeld)
        {
            yield return null;
        }

    

        returning = true;   

        startPosition = transform.position;
        elapsed = 0f;

        while (elapsed < moveBackTime)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / moveBackTime);
            transform.position = Vector3.Lerp(startPosition, followPositionBeforeAttack, t);

            yield return null;
        }
        
        transform.position = followPositionBeforeAttack;

        if (animator != null)
        {
            animator.speed = 1;
            animator.Play(floatingClipName, 0, 0f);
        }

        returning = false;


        laserTimer = laserCooldown;
        staminaProgress = 0f;
        IsAttacking = false;
    }

    public void AnimationFireLaser()
    {
        if(!IsAttacking) return;
        if(laserFired) return;
        laserFired = true;
        FireLaser(currentLaserDir);

        if (animator != null)
            animator.speed = 0f;
    }


    public void HoldAttackAnimation()
    {
        animationHeld = true;
        if (animator != null)
            animator.speed = 0f;
    }


    void FireLaser(Vector2 direction)
    {
        Debug.Log("Shooting laser in direction: " + direction);
        Vector2 startPosition = laserSpawnPoint.position;

        int combinedLayerMask =0;
        foreach (LayerMask layer in laserHitLayers)
        {
            combinedLayerMask |= layer.value;
        }
        RaycastHit2D hit = Physics2D.Raycast(laserSpawnPoint.position, direction, laserRange, combinedLayerMask);

        Vector2 endPosition;

        if (hit.collider != null)
        {
            endPosition = hit.point;

            IDamagable damagable = hit.collider.GetComponent<EnemyHealth>();

            if (damagable != null)
            {
                damagable.TakeDamage(laserDamage);
            }
        }
        else
        {
            endPosition = startPosition + direction * laserRange;
        }


        laserLine.positionCount = 2;
        laserLine.useWorldSpace = true;
        laserLine.startWidth = laserWidth;
        laserLine.endWidth = laserWidth;

        laserLine.SetPosition(0, startPosition);
        laserLine.SetPosition(1, endPosition);

        laserLine.enabled = true;
        if(rotTimer != null)
        {
            rotTimer.AddTime(-laserTimeCost);
        }

        Debug.Log("Laser Start: " + startPosition);
        Debug.Log("Laser End: " + endPosition);
        Debug.Log("Laser Time: " + laserTime);
        Debug.Log("Laser Range: " + laserRange);

    }
        
  
}
