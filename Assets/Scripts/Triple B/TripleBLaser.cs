using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UIElements;

public class TripleBLaser : MonoBehaviour
{
    [Header("Laser Settings")]
    [SerializeField] Transform laserSpawnPoint;
    [SerializeField] LineRenderer laserLine;

    [SerializeField] float laserRange = 10f;
    [SerializeField] float laserCooldown = 3f;
    [SerializeField] float laserDamage = 20f;
    [SerializeField] float laserTime = 1f;
    [SerializeField] float laserWidth = 0.2f;

    [SerializeField] LayerMask[] laserHitLayers;

    [SerializeField] RotTimer rotTimer;
    [SerializeField] float laserTimeCost = 3f;

    [Header("Target Selection")]
    [SerializeField] float cursorTargetRadius = 0.15f;

    [Header("Attack Movement")]
    [SerializeField] float standOffDistance = 1.2f;
    [SerializeField] float attackMoveSpeed = 10f;
    [SerializeField] float targetStopDistance = 0.05f;
    [SerializeField] float moveBackTime = 0.25f;

    [Header("Attack Collision")]
    [SerializeField] private LayerMask attackBlockerLayers;
    [SerializeField] private float attackCollisionBuffer = 0.05f;

    [Header("Animation")]
    [SerializeField] Animator animator;
    [SerializeField] string laserTrigger = "StartLaser";
    [SerializeField] string floatingClipName = "TripleBFloat";

    private float laserTimer;
    private float laserElapsed;

    private Vector3 followPositionBeforeAttack;
    private Vector2 currentLaserDir;

    private Rigidbody2D rigidbody2;


    private IDamagable currentDamageable;
    private ILaserStunnable currentLaserStunnable;

    private Transform currentTarget;
    private Collider2D currentTargetCollider;
    private Collider2D tripleBCollider;
    private ContactFilter2D attackContactFilter;
    private RaycastHit2D[] attackCastResults = new RaycastHit2D[10];

    private bool laserFired;
    private bool returning;
    private bool animationHeld;
    public bool IsAttacking { get; private set; }

    private float staminaProgress = 1f;

    public float StaminaProgress => staminaProgress;

    public event Action OnLaserUsed;
    public event Action OnLaserFinished;
   

    public bool CanShoot
    {
        get
        {
            bool hasTime = rotTimer == null || rotTimer.currentTime >= laserTimeCost;
            return laserTimer <= 0f && !IsAttacking && hasTime;
        }

    }

    private void Awake()
    {
        tripleBCollider = GetComponent<Collider2D>();

        attackContactFilter = new ContactFilter2D();
        attackContactFilter.SetLayerMask(attackBlockerLayers);
        attackContactFilter.useTriggers = true;

        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        rigidbody2 = GetComponent<Rigidbody2D>();
    }
    private void Start()
    {
        if (laserLine != null)
            laserLine.enabled = false;
    }
    void Update()
    {
        if (laserTimer > 0f)
        {
            laserTimer -= Time.deltaTime;
            if (laserTimer < 0f)
                laserTimer = 0f;

            if (laserCooldown > 0f)
                staminaProgress = 1f - (laserTimer / laserCooldown);
        }
    }

    public void ShootLaser(Vector2 direction, Vector2 mouseWorldPosision)
    {

        if (!CanShoot) return;


        if (laserSpawnPoint == null || laserLine == null)
        {
            Debug.LogWarning("Laser Spawn Point or Laser Line is not assigned.");
            return;
        }


        currentLaserDir = direction.normalized;

        followPositionBeforeAttack = transform.position;

        if (!FindTargetUnderCursor(mouseWorldPosision))
        {
            Debug.Log("No Valid IDamagable under cursor");
            return;
        }


        StartCoroutine(LaserAttack());
        OnLaserUsed?.Invoke();

    }

    private IEnumerator LaserAttack()
    {
        IsAttacking = true;

        returning = false;
        laserFired = false;
        animationHeld = false;

        staminaProgress = 1f;

        if (rigidbody2 != null)
        {
            rigidbody2.linearVelocity = Vector2.zero;
        }



        while (IsTargetValid())
        {
            Vector3 targetPosition = CalculateStandOffPosition();

            Vector2 currentPosition = rigidbody2 != null ? rigidbody2.position : (Vector2)transform.position;

            Vector2 offset = (Vector2)targetPosition - currentPosition;

            float distance = offset.magnitude;

            if (distance <= targetStopDistance)
            {
                if (rigidbody2 != null)
                    rigidbody2.linearVelocity = Vector2.zero;

                break;
            }

            Vector2 desiredVelocity = offset.normalized * attackMoveSpeed;

            Vector2 safeVelocity = GetSafeAttackVelocity(desiredVelocity);

            if (rigidbody2 != null)
            {
                rigidbody2.MovePosition(
                    rigidbody2.position + safeVelocity * Time.fixedDeltaTime
                );
            }
            else
            {
                transform.position +=
                    (Vector3)(safeVelocity * Time.deltaTime);
            }

            yield return new WaitForFixedUpdate();
        }

        if (!IsTargetValid())
        {
            CancelLaserAttack();
            yield break;
        }

       

        if (rigidbody2 != null)
        {
            rigidbody2.linearVelocity = Vector2.zero;
        }



        if (animator != null)
        {
            animator.speed = 1f;

            animator.ResetTrigger(laserTrigger);

            animator.SetTrigger(laserTrigger);
        }

        while (!laserFired)
        {
            if (!IsTargetValid())
            {
                CancelLaserAttack();
                yield break;
            }

            yield return null;
        }



        laserElapsed = 0f;

        while (laserElapsed < laserTime)
        {
            float deltaTime = Time.deltaTime;

            laserElapsed += deltaTime;

            staminaProgress = 1f - Mathf.Clamp01(laserElapsed / laserTime);

            if (IsTargetValid() && currentDamageable != null)
            {
                float damageThisFrame = (laserDamage / laserTime) * deltaTime;

                currentDamageable.TakeDamage(damageThisFrame);
            }

            yield return null;
        }


        EndLaserHit();

        if (laserLine != null)
            laserLine.enabled = false;

        laserFired = false;
        laserElapsed = 0f;

        if (animator != null)
            animator.speed = 1f;


        while (!animationHeld)
        {
            yield return null;
        }


        returning = true;

        Vector3 startPosition = transform.position;

        float elapsed = 0f;

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
            animator.speed = 1f;

            animator.Play(floatingClipName, 0, 0f);
        }

        returning = false;


        laserTimer = laserCooldown;

        staminaProgress = 0f;

        IsAttacking = false;

        ClearTarget();

        OnLaserFinished?.Invoke();
    }

    private bool FindTargetUnderCursor(Vector2 mouseWorldPosition)
    {
        ClearTarget();

        int combinedLayerMask = GetCombinedLayerMask();

        if (combinedLayerMask == 0)
        {
            Debug.LogWarning("Triple B Laser: No Laser Hit Layers assigned.");

            return false;
        }


        Collider2D[] colliders = Physics2D.OverlapPointAll(mouseWorldPosition, combinedLayerMask);

        IDamagable bestDamageable = null;
        Collider2D bestCollider = null;

        float bestDistance = float.MaxValue;

        foreach (Collider2D collider in colliders)
        {
            if (collider == null)
                continue;

            if (TryGetDamageable(collider, out IDamagable damageable, out Transform targetTransform))
            {
                float distance = Vector2.Distance(mouseWorldPosition, collider.ClosestPoint(mouseWorldPosition));

                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    bestDamageable = damageable;
                    bestCollider = collider;
                }
            }
        }



        if (bestDamageable == null && cursorTargetRadius > 0f)
        {
            Collider2D[] nearbyColliders = Physics2D.OverlapCircleAll(mouseWorldPosition, cursorTargetRadius, combinedLayerMask);

            foreach (Collider2D collider in nearbyColliders)
            {
                if (collider == null)
                    continue;

                if (TryGetDamageable(collider, out IDamagable damageable, out Transform targetTransform))
                {
                    float distance = Vector2.Distance(mouseWorldPosition, collider.ClosestPoint(mouseWorldPosition));

                    if (distance < bestDistance)
                    {
                        bestDistance = distance;
                        bestDamageable = damageable;
                        bestCollider = collider;
                    }
                }
            }
        }

        if (bestDamageable == null)
            return false;

        currentDamageable = bestDamageable;

        currentTargetCollider = bestCollider;

        if (!TryGetDamageable(bestCollider,out IDamagable finalDamageable,out Transform finalTargetTransform))
        {
            ClearTarget();
            return false;
        }

        currentDamageable = finalDamageable;
        currentTarget = finalTargetTransform;
        currentLaserStunnable = GetLaserStunnable(bestCollider);


        return currentTarget != null;
    }

    private bool TryGetDamageable(Collider2D collider, out IDamagable damageable, out Transform targetTransform)
    {
        damageable = null;
        targetTransform = null;

        if (collider == null)
            return false;


        damageable = collider.GetComponent<IDamagable>();

        if (damageable != null)
        {
            Component component = damageable as Component;

            if (component != null)
            {
                targetTransform = component.transform;

                return true;
            }
        }

        damageable = collider.GetComponentInParent<IDamagable>();

        if (damageable != null)
        {
            Component component = damageable as Component;

            if (component != null)
            {
                targetTransform = component.transform;

                return true;
            }
        }


        damageable = collider.GetComponentInChildren<IDamagable>();

        if (damageable != null)
        {
            Component component = damageable as Component;

            if (component != null)
            {
                targetTransform = component.transform;

                return true;
            }
        }

        return false;
    }


    private ILaserStunnable GetLaserStunnable(Collider2D collider)
    {
        if (collider == null) return null;

        ILaserStunnable stunnable = collider.GetComponent<ILaserStunnable>();

        if (stunnable != null)
            return stunnable;

        stunnable = collider.GetComponentInParent<ILaserStunnable>();

        if(stunnable != null)
            return stunnable;

        stunnable = collider .GetComponentInChildren<ILaserStunnable>();

        return stunnable;
    }
    private Vector3 CalculateStandOffPosition()
    {
        if (currentTarget == null || currentTargetCollider == null
        )
        {
            return transform.position;
        }

        Vector2 targetCenter = currentTarget.position;

        Vector2 tripleBPosition = transform.position;

        Vector2 closestPoint = currentTargetCollider.ClosestPoint(tripleBPosition);

        Vector2 direction = tripleBPosition - targetCenter;

        if (direction.sqrMagnitude < 0.0001f)
        {
            direction = Vector2.right;
        }
        else
        {
            direction.Normalize();
        }

        Vector2 desiredPosition = closestPoint + direction * standOffDistance;

        return new Vector3(desiredPosition.x, desiredPosition.y, transform.position.z);
    }

    public void AnimationFireLaser()
    {
        if (!IsAttacking) return;
        if (laserFired) return;

        laserFired = true;

        UpdateLaserDirection();

        FireLaser(currentLaserDir);

        if (animator != null)
            animator.speed = 0f;
    }


    public void HoldAttackAnimation()
    {
        if (!IsAttacking)
            return;

        animationHeld = true;

        if (animator != null)
            animator.speed = 0f;
    }

    private void UpdateLaserDirection()
    {
        if (laserSpawnPoint == null || currentTarget == null)
            return;

        Vector2 direction = (Vector2)currentTarget.position - (Vector2)laserSpawnPoint.position;

        if (direction.sqrMagnitude > 0.001f)
        {
            currentLaserDir = direction.normalized;
        }
    }

    void FireLaser(Vector2 direction)
    {
        if (laserSpawnPoint == null || laserLine == null)
            return;


        Vector2 startPosition = laserSpawnPoint.position;


        Vector2 endPosition;


        if (IsTargetValid())
        {
            direction = ((Vector2)currentTarget.position - startPosition).normalized;

            currentLaserDir = direction;

            Vector2 targetPoint = currentTargetCollider.ClosestPoint(startPosition);

            float distance = Vector2.Distance(startPosition, targetPoint);

            if (distance > laserRange)
            {
                endPosition = startPosition + direction * laserRange;
            }
            else
            {
                endPosition = targetPoint;
            }

           if(currentLaserStunnable != null)
           {
                currentLaserStunnable.StartLaserStun();
           }
        }
        else
        {
            endPosition = startPosition + direction * laserRange;
            currentDamageable = null;
        }


        laserLine.positionCount = 2;
        laserLine.useWorldSpace = true;

        laserLine.startWidth = laserWidth;
        laserLine.endWidth = laserWidth;

        laserLine.SetPosition(0, startPosition);
        laserLine.SetPosition(1, endPosition);

        laserLine.enabled = true;

        if (rotTimer != null)
        {
            rotTimer.AddTime(-laserTimeCost);
        }

        Debug.Log("Laser Start: " + startPosition);
        Debug.Log("Laser End: " + endPosition);
        Debug.Log("Laser Time: " + laserTime);
        Debug.Log("Laser Range: " + laserRange);

    }

    private void EndLaserHit()
    {
        if(currentLaserStunnable != null)
        {
            currentLaserStunnable.EndLaserStun();
            currentLaserStunnable = null;
        }
    }

    private bool IsTargetValid()
    {
        if (currentTarget == null)
            return false;

        if (currentTargetCollider == null)
            return false;

        if (currentDamageable == null)
            return false;

        return true;
    }

    private void CancelLaserAttack()
    {
        EndLaserHit();

        if (laserLine != null)
            laserLine.enabled = false;

        if (animator != null)
        {
            animator.speed = 1f;
            animator.Play(floatingClipName, 0, 0f);
        }
        IsAttacking = false;
        returning = false;
        laserFired = false;
        animationHeld = false;

        laserElapsed = 0f;
        staminaProgress = 1f;

        ClearTarget();
    }

    private void ClearTarget()
    {
        currentTarget = null;
        currentTargetCollider = null;
        currentDamageable = null;
        currentLaserStunnable = null ;
    }

    private int GetCombinedLayerMask()
    {
        int combinedLayerMask = 0;

        foreach(LayerMask layer in laserHitLayers)
        {
            combinedLayerMask |= layer.value;
        }

        return combinedLayerMask;
    }

    private Vector2 GetSafeAttackVelocity(Vector2 desiredVelocity)
    {
        if (tripleBCollider == null || desiredVelocity.sqrMagnitude < Mathf.Epsilon)
            return desiredVelocity;

        Vector2 direction = desiredVelocity.normalized;

        float distance = desiredVelocity.magnitude * Time.deltaTime;

        int hitCount = tripleBCollider.Cast(direction,attackContactFilter,attackCastResults,distance + attackCollisionBuffer);

        if (hitCount == 0)
            return desiredVelocity;

        float closestDistance = distance;

        for (int i = 0; i < hitCount; i++)
        {
            if (attackCastResults[i].collider == null)
                continue;

            if (attackCastResults[i].collider == currentTargetCollider)
                continue;

            closestDistance = Mathf.Min(closestDistance,attackCastResults[i].distance);
        }

        if (closestDistance <= attackCollisionBuffer)
            return Vector2.zero;

        float safeSpeed =(closestDistance - attackCollisionBuffer) / Time.deltaTime;

        return direction * Mathf.Min(desiredVelocity.magnitude,safeSpeed);
    }
   
    private void OnDrawGizmos()
    {
        if(laserSpawnPoint == null) return;

        Gizmos.color = Color.yellow;

        Gizmos.DrawWireSphere(laserSpawnPoint.position,standOffDistance);
    }
}
