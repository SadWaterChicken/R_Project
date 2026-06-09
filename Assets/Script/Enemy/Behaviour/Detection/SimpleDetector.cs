using UnityEngine;

public class SimpleDetector : MonoBehaviour
{
    public Transform player;
    public float detectionRange = 5f;

    bool PlayerInRange()
    {
        return Vector3.Distance(transform.position, player.position) <= detectionRange;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}
