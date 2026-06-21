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

    [SerializeField] private EnemyStat enemyStats;
    private float previousHealth;
    public bool HasTakenDamage { get; private set; }
    private void Awake()
    {
        myCollider = GetComponent<Collider>();
        
        // Auto-assign the player if it's not set in the inspector
        if (player == null)
        {
            // Find all active GameObjects in the scene
            GameObject[] allObjects = FindObjectsByType<GameObject>(FindObjectsInactive.Exclude);
            foreach (GameObject obj in allObjects)
            {
                // Check if this object's layer is part of the detectionLayer mask
                if ((detectionLayer.value & (1 << obj.layer)) != 0)
                {
                    player = obj.transform;
                    break;
                }
            }
            
            if (player == null)
            {
                Debug.LogWarning("SimpleDetector: Could not automatically find an object within the detectionLayer.");
            }

            if (enemyStats != null)
            {
                previousHealth = enemyStats.currentHealth;
            }
        }
    }

    private void Update()
    {
        DetectTarget();
        CheckHealthLoss();
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


    public GameObject DetectingSight(GameObject potentialTarget)
    {
        RaycastHit hit;
        Vector3 direction =
    (potentialTarget.transform.position -
     (transform.position + Vector3.up * detectionHeight)).normalized;
        Physics.Raycast(transform.position + Vector3.up * detectionHeight, direction, out hit, detectionRange, detectionLayer);

        if (hit.collider != null && hit.collider.gameObject == potentialTarget)
        {
            Debug.DrawLine(transform.position + Vector3.up * detectionHeight, potentialTarget.transform.position, Color.green);
            return hit.collider.gameObject;
        }
        else
        {
            return null;
        }
    }

    private void CheckHealthLoss()
    {
        if (enemyStats == null) return;

        if (enemyStats.currentHealth < previousHealth)
        {
            HasTakenDamage = true;
        }
        else
        {
            HasTakenDamage = false;
        }

        previousHealth = enemyStats.currentHealth;
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
