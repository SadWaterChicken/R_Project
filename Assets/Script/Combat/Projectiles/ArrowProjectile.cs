using UnityEngine;

public class ArrowProjectile : ProjectileBase
{
    public GameObject hitVFXPrefab;

    private Rigidbody rb;

    public override void Initialize(float speed, float lifeTime, DamagePayload payload)
    {
        base.Initialize(speed, lifeTime, payload);
        
        rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = transform.forward * speed;
        }
    }

    protected override void Move()
    {
        if (rb != null && rb.linearVelocity.sqrMagnitude > 0.1f)
        {
            // Ép đầu mũi tên luôn chúi theo hướng di chuyển thực tế (Parabol)
            transform.forward = rb.linearVelocity.normalized;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (damagePayload.owner != null && other.transform.IsChildOf(damagePayload.owner)) return;

        HandleHit(other);

        if (hitVFXPrefab != null && other.CompareTag("Enemy"))
        {
            Instantiate(hitVFXPrefab, transform.position, Quaternion.identity);
        }

        if (!other.isTrigger || other.CompareTag("Enemy"))
        {
            Despawn();
        }
    }
}
