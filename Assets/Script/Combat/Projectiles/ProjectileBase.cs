using UnityEngine;

public abstract class ProjectileBase : MonoBehaviour
{
    protected DamagePayload damagePayload;
    protected float speed = 20f;
    protected float lifeTime = 3f;
    protected bool isInitialized = false;

    public virtual void Initialize(float speed, float lifeTime, DamagePayload payload)
    {
        this.speed = speed;
        this.lifeTime = lifeTime;
        this.damagePayload = payload;
        this.isInitialized = true;
        
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        CancelInvoke(nameof(Despawn));
        Invoke(nameof(Despawn), lifeTime);
    }

    protected virtual void Start()
    {
        if (!isInitialized)
        {
            Invoke(nameof(Despawn), lifeTime);
        }
    }

    public virtual void Despawn()
    {
        isInitialized = false;
        if (ProjectilePool.Instance != null)
        {
            ProjectilePool.Instance.ReturnToPool(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    protected virtual void Update()
    {
        if (isInitialized)
        {
            Move();
        }
    }

    protected virtual void Move() { }

    protected virtual void HandleHit(Collider other)
    {
        // Kiểm tra tránh đánh trúng chính mình
        if (damagePayload.owner != null && other.transform.IsChildOf(damagePayload.owner)) return;

        if (other.CompareTag("Enemy"))
        {
            EnemyStat stat = other.GetComponentInParent<EnemyStat>();
            if (stat != null)
            {
                ApplyDamage(stat);
            }
        }
    }

    protected virtual void ApplyDamage(EnemyStat stat)
    {
        float healthBefore = stat.currentHealth;
        
        // TakeMixedDamage(physicalDmg, magicDmg, trueDmg, pureDmg, isCrit)
        stat.TakeMixedDamage(damagePayload.physicalDamage, damagePayload.magicDamage, 0f, 0f, damagePayload.isCrit);
        
        if (healthBefore > 0 && stat.currentHealth <= 0)
        {
            // Xử lý rơi đồ
            EnemyLootTable lootTable = stat.GetComponentInParent<EnemyLootTable>();
            if (lootTable != null)
            {
                lootTable.DropLoot(stat.transform.position);
            }

            // Xử lý điểm Mastery
            if (damagePayload.weaponSource != null && ForgeManager.Instance != null)
            {
                EnemyMasteryReward rewardComp = stat.GetComponentInParent<EnemyMasteryReward>();
                float masteryReward = rewardComp != null ? rewardComp.masteryGranted : stat.enemyLevel * 1f;
                ForgeManager.Instance.AddMasteryOnKill(damagePayload.weaponSource, masteryReward);
            }
        }
    }
}
