using System;
using UnityEngine;

public class OrangeSolider : MonoBehaviour
{
    [SerializeField] float moveSpeed = 3f;
    [SerializeField] float damage = 5f;

    Transform target;
    public event Action OnDied;

    public void SetTarget(Transform playerTransform)
    {
        target = playerTransform;
    }

    void Update()
    {
        if (target == null) return;

        Vector2 direction = ((Vector2)target.position - (Vector2)transform.position).normalized;
        transform.position = Vector2.MoveTowards(transform.position, target.position, moveSpeed * Time.deltaTime);
    }

    public void Die()
    {
        OnDied?.Invoke();
        Destroy(gameObject);
    }

}
