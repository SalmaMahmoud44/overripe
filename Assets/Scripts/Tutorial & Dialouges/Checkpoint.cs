using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    [SerializeField] private string checkpointID;

    public void Activate()
    {
        if (CheckpointManager.Instance == null)
            return;
  
        CheckpointManager.Instance.SetCheckpoint( checkpointID, transform.position);
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, 0.3f);
    }
}