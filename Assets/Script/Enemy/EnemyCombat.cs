using UnityEngine;

public class EnemyCombat : MonoBehaviour
{
    [Header("References")]
    public Animator animator;
    public EnemyStat enemyStat;
    public Transform attackPoint;

    [Header("Combat Settings")]
    public LayerMask targetLayer;
    
    private float nextAttackTime = 0f;

    private void Awake()
    {
        if (enemyStat == null)
        {
            enemyStat = GetComponent<EnemyStat>();
        }
        
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }
    }

    public bool CanAttack()
    {
        return Time.time >= nextAttackTime;
    }

    public void TriggerAttack()
    {
        if (CanAttack())
        {
            if (animator != null)
            {
                // Trigger an attack animation
                // Ensure your Animator has a Trigger parameter named "Attack"
                animator.SetTrigger("Attack");
            }
            else
            {
                // Fallback if no Animator is assigned
                Attack();
            }

            if (enemyStat != null)
            {
                nextAttackTime = Time.time + enemyStat.attackCooldown;
            }
        }
    }

    // This method should ideally be called via an Animation Event during the attack animation
    public void Attack()
    {
        if (attackPoint == null || enemyStat == null) return;

        Collider[] hitTargets = Physics.OverlapSphere(attackPoint.position, enemyStat.attackRange, targetLayer);
        System.Collections.Generic.HashSet<CharacterStats> hitStats = new System.Collections.Generic.HashSet<CharacterStats>();

        foreach (Collider hitTarget in hitTargets)
        {
            // You can use a specific tag check if necessary, e.g. hitTarget.CompareTag("Player")
            CharacterStats targetStats = hitTarget.GetComponentInParent<CharacterStats>();
            
            if (targetStats != null && !hitStats.Contains(targetStats))
            {
                hitStats.Add(targetStats);
                float damage = enemyStat.GetPhysicalDamage();
                
                // Enemy chỉ dùng sát thương vật lý cơ bản cho đòn đánh thường, giống Player
                targetStats.TakePhysicalDamage(damage);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null || enemyStat == null) return;
        
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, enemyStat.attackRange);
    }
}
