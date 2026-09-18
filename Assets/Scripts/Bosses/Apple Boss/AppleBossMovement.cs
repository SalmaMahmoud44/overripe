using System.Collections;
using UnityEngine;

public class AppleBossMovement : MonoBehaviour
{
    [Header("Movement Points")]
    [SerializeField] Transform leftPoint;
    [SerializeField] Transform centerPoint;
    [SerializeField] Transform rightPoint;

    [Header("Movement")]
    [SerializeField] float moveSpeed = 3f;
    [SerializeField] float stopDistance = 0.05f;

    public bool IsMoving { get; private set; }

    public Transform CurrentPoint { get; private set; }

    private Coroutine moveRoutine;

    private void Start()
    {
        CurrentPoint = centerPoint;
    }

    public void MoveToLeft()
    {
        MoveTo(leftPoint);
    }

    public void MoveToRight() 
    {
        MoveTo(rightPoint);
    }
    public void MoveToCenter() 
    {
        MoveTo(centerPoint);
    }

    public void MoveTo(Transform targetPoint)
    {
        if(targetPoint == null) return; 

        if(moveRoutine != null) 
            StopCoroutine(moveRoutine);

        moveRoutine = StartCoroutine(MoveRoutine(targetPoint));
    }

    IEnumerator MoveRoutine(Transform targetPoint)
    {
        IsMoving = true;

        while(Mathf.Abs(transform.position.x - targetPoint.position.x) > stopDistance)
        {
            Vector3 targetPos = new Vector3(targetPoint.position.x, transform.position.y, transform.position.z);

            transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
            yield return null;
        }

        transform.position = new Vector3(targetPoint.position.x, transform.position.y, transform.position.z);

        CurrentPoint = targetPoint;
        IsMoving = false;
        moveRoutine = null;
    }

    public void Stop()
    {
        if(moveRoutine != null)
        {
            StopCoroutine(moveRoutine);
            moveRoutine = null;
        }
        IsMoving = false;
    }

}
