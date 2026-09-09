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

    [SerializeField] LayerMask laserHitLayer;

    [Header("Attack Movement")]
    [SerializeField] Transform attackPos;

    [SerializeField] float moveForwardTime = 0.15f;
    [SerializeField] float moveBackTime = 0.15f;

     private float laserTimer;
     private float laserElapsed;

     private Vector3 followPositionBeforeAttack;

     public bool IsAttacking { get; private set; }
    


    public float StaminaProgress
    { 
        get
        {
            if (IsAttacking)
                return 1f - Mathf.Clamp01(laserElapsed/laserTime);

            if (laserCooldown <= 0f)
                return 1f;

            return 1f - Mathf.Clamp01(laserTimer / laserCooldown);
        } 
    }

    public bool CanShoot
    {
        get
        { 
            return laserTimer <= 0f && !IsAttacking;
        }

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
            {
                laserTimer = 0f;
            }
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

        direction.Normalize();

        followPositionBeforeAttack = transform.position;


        StartCoroutine(LaserAttack(direction));


     
        
    }

    private IEnumerator LaserAttack(Vector2 direction)
    {
        IsAttacking = true;

        laserElapsed = 0f;
        Rigidbody2D rigidbody2D = GetComponent<Rigidbody2D>();

        if (rigidbody2D != null) 
            rigidbody2D.linearVelocity = Vector2.zero;

       

        Vector3 startPosition = transform.position;

       
        float elapsed = 0f;

        while (elapsed < moveForwardTime)
        {
            elapsed += Time.deltaTime;

            float t = elapsed / moveForwardTime;

            transform.position = Vector3.Lerp(startPosition, attackPos.position, t);

            yield return null;
        }

        transform.position = attackPos.position;

        FireLaser(direction);

        laserElapsed = 0f;
        while(laserElapsed < laserTime)
        {
            laserElapsed += Time.deltaTime;
            yield return null;
        }

        if(laserLine != null)
        {
            laserLine.enabled = false;
            laserElapsed = 0f;
        }

        elapsed = 0f;

        startPosition = transform.position;

        while (elapsed < moveBackTime)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / moveBackTime;
            transform.position = Vector3.Lerp(startPosition, followPositionBeforeAttack, t);

            yield return null;
        }
        
        transform.position = followPositionBeforeAttack;

        laserTimer = laserCooldown;

        IsAttacking = false;
    }

    void FireLaser(Vector2 direction)
    {
        Debug.Log("Shooting laser in direction: " + direction);
        Vector2 startPosition = laserSpawnPoint.position;

        RaycastHit2D hit = Physics2D.Raycast(laserSpawnPoint.position, direction, laserRange, laserHitLayer);

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


        Debug.Log("Laser Start: " + startPosition);
        Debug.Log("Laser End: " + endPosition);
        Debug.Log("Laser Time: " + laserTime);
        Debug.Log("Laser Range: " + laserRange);

    }
        
  
}
