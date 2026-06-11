using System.Collections.Generic;
using UnityEngine;

public class SimpleDetector : MonoBehaviour
{
    public Transform player;
    [SerializeField] private float detectionRadius = 5f;
    [SerializeField] private LayerMask detectionLayer;

    [SerializeField] private float detectionHeight = 5f;
    [SerializeField] private float detectionRange = 5f;
    private Collider myCollider;

    public GameObject detectedTargets { get; set; }
     
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
            detectionLayer);

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


    public GameObject DetectingRange(GameObject potentialTarget)
    {
        RaycastHit hit;
        Vector3 direction = potentialTarget.transform.position;
        Physics.Raycast(transform.position + Vector3.up * detectionHeight, direction, out hit, detectionRange, detectionLayer);

        if (hit.collider != null && hit.collider.gameObject == potentialTarget)
        {
            return hit.collider.gameObject;
        }
        else
        {
            return null;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = currentTarget != null ? Color.green : Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
    public GameObject UpdateDetector()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, detectionRadius, detectionLayer);

        if(colliders.Length > 0) 
        {   
            detectedTargets = colliders[0].gameObject;        
        }
        else
        {
            detectedTargets = null;
        }
        return detectedTargets;
    }
}
