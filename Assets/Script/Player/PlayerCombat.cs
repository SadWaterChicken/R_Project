using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    public Animator animator;
    public PlayerStat playerStat;
    public float Cooldown = 2f;
    private float Timer;
    public Transform attackPoint;
    public float weaponRange = 10;
    public LayerMask enemyLayers;

    void Update()
    {
        if(Timer > 0)
        {
            Timer -= Time.deltaTime;
        }
    }


    public void Attack()
    {
        animator.SetBool("isAttacking", true);
        Collider[] enemies = Physics.OverlapSphere(attackPoint.position, weaponRange, enemyLayers);

        if(enemies.Length > 0)
        {
            EnemyStat stat = enemies[0].GetComponent<EnemyStat>();
            if(stat != null)
            {
                // Use GetPhysicalDamage() to include equipment bonuses
                float damage = playerStat != null ? playerStat.GetPhysicalDamage() : 10f;
                stat.changeHealth(damage);
            }
        }
        Timer = Cooldown;
        Invoke(nameof(ResetAttack), 0.6f);  
    }

    private void ResetAttack()
    {
        animator.SetBool("isAttacking", false);
    }

    public bool IsAttackReady()
    {
        return Timer <= 0;
    }
}
