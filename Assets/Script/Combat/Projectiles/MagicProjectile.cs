using UnityEngine;
using System.Collections.Generic;

public class MagicProjectile : ProjectileBase
{
    [Header("Magic Settings")]
    public bool isHoming = false;
    public float homingTurnSpeed = 5f;
    public float homingDetectRadius = 15f;
    
    [Header("Explosion Settings")]
    public float aoeRadius = 2f;
    public GameObject explosionVFXPrefab;

    private Transform target;

    protected override void Start()
    {
        base.Start();
        if (isHoming)
        {
            FindTarget();
        }
    }

    private void FindTarget()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, homingDetectRadius);
        float closestDistance = Mathf.Infinity;

        foreach (var hit in hits)
        {
            if (hit.CompareTag("Enemy"))
            {
                float dist = Vector3.Distance(transform.position, hit.transform.position);
                if (dist < closestDistance)
                {
                    closestDistance = dist;
                    // Lấy vị trí trung tâm của quái thay vì chân
                    target = hit.transform;
                }
            }
        }
    }

    protected override void Move()
    {
        if (isHoming && target != null)
        {
            // Hướng dần về phía mục tiêu
            Vector3 direction = (target.position - transform.position).normalized;
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * homingTurnSpeed);
        }

        transform.Translate(Vector3.forward * speed * Time.deltaTime, Space.Self);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (damagePayload.owner != null && other.transform.IsChildOf(damagePayload.owner)) return;

        // Nếu va chạm vật cản hoặc mục tiêu
        if (!other.isTrigger || other.CompareTag("Enemy"))
        {
            if (aoeRadius > 0f)
            {
                Explode();
            }
            else
            {
                HandleHit(other);
                if (explosionVFXPrefab != null && other.CompareTag("Enemy"))
                {
                    Instantiate(explosionVFXPrefab, transform.position, Quaternion.identity);
                }
            }

            Despawn();
        }
    }

    private void Explode()
    {
        if (explosionVFXPrefab != null)
        {
            Instantiate(explosionVFXPrefab, transform.position, Quaternion.identity);
        }

        Collider[] hits = Physics.OverlapSphere(transform.position, aoeRadius);
        HashSet<EnemyStat> hitEnemies = new HashSet<EnemyStat>();

        foreach (var hit in hits)
        {
            if (hit.CompareTag("Enemy"))
            {
                EnemyStat stat = hit.GetComponentInParent<EnemyStat>();
                if (stat != null && !hitEnemies.Contains(stat))
                {
                    hitEnemies.Add(stat);
                    ApplyDamage(stat); // Gọi hàm tính sát thương từ ProjectileBase
                }
            }
        }
    }
}
