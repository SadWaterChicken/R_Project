using UnityEngine;
using Unity.Behavior;

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

    [Header("Lunge Settings")]
    public Collider mainCollider;
    public BehaviorGraphAgent enemyBehaviorAgent;
    
    // To keep track of if we already dealt damage during the current lunge
    private bool hasDealtLungeDamage = false;
    private bool wasLunging = false;

    private void Update()
    {
        if (enemyBehaviorAgent != null && enemyBehaviorAgent.BlackboardReference.GetVariableValue("IsLunging", out bool isLunging))
        {
            if (isLunging && !wasLunging)
            {
                // We just started lunging
                wasLunging = true;
                hasDealtLungeDamage = false; // Reset damage flag
                
                // Ignore collision with the Player layer so the enemy can pass through
                int playerLayer = LayerMask.NameToLayer("Player");
                if (playerLayer != -1)
                {
                    Physics.IgnoreLayerCollision(gameObject.layer, playerLayer, true);
                }
            }
            else if (!isLunging && wasLunging)
            {
                // We just stopped lunging
                wasLunging = false;
                
                // Restore collision with the Player layer
                int playerLayer = LayerMask.NameToLayer("Player");
                if (playerLayer != -1)
                {
                    Physics.IgnoreLayerCollision(gameObject.layer, playerLayer, false);
                }
            }
            
            // Note: Since it's a trigger, checking overlap frame by frame or using OnTriggerEnter is needed.
            // Using OverlapSphere here to check for the player while lunging in Update:
            if (isLunging && !hasDealtLungeDamage)
            {
                CheckLungeDamage();
            }
        }
    }

    private void CheckLungeDamage()
    {
        if (mainCollider == null || enemyStat == null) return;

        // Use the collider's bounds to check for the player
        Collider[] hitTargets = Physics.OverlapBox(mainCollider.bounds.center, mainCollider.bounds.extents, mainCollider.transform.rotation, targetLayer);
        
        foreach (Collider hitTarget in hitTargets)
        {
            CharacterStats targetStats = hitTarget.GetComponentInParent<CharacterStats>();

            if (targetStats != null)
            {
                float damage = enemyStat.GetPhysicalDamage();
                targetStats.TakePhysicalDamage(damage);
                
                hasDealtLungeDamage = true; // Only deal damage once per lunge
                break; // Stop after hitting the first target (usually the player)
            }
        }
    }
}
