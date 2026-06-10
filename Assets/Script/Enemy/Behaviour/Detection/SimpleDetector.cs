using System.Collections.Generic;
using UnityEngine;

public class SimpleDetector : MonoBehaviour
{
    public Transform player;
    public float detectionRange = 5f;
    [SerializeField] private float detectionRadius = 5f;
    [SerializeField] private LayerMask playerLayer;


    private Collider myCollider;
    private List<GameObject> detectedTargets = new();
    public Transform currentTarget { get; private set; }

    private void Awake()
    {
        myCollider = GetComponent<Collider>();
    }

    private void Update()
    {
        DetectTarget();
    }
    bool PlayerInRange()
    {
        return Vector3.Distance(transform.position, player.position) <= detectionRange;
    }

    private void DetectTarget()
    {
        currentTarget = null;

        Collider[] hits = Physics.OverlapSphere(
            transform.position,
            detectionRadius,
            playerLayer);

        float closestDistance = float.MaxValue;

        foreach (Collider hit in hits)
        {
            float distance = Vector3.Distance(
                transform.position,
                hit.transform.position);

            if (distance < closestDistance)
            {
                closestDistance = distance;
                currentTarget = hit.transform;
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}
