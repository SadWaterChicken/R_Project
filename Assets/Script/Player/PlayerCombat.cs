using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    public Animator animator;
    public PlayerStat playerStat;
    public float ComboDelay = 1f;
    private int numbClicks = 0;
    private float lastClickedTime = 3;
    public Transform attackPoint;
    public float weaponRange = 10;
    public LayerMask enemyLayers;
    public float damage;
    private bool isGuarding = false;
    private float guardDamageReduction = 0.5f; // Reduce damage by 50% when guarding

    void Update()
    {
        if(Time.time - lastClickedTime > ComboDelay)
        {
            numbClicks = 0;
        }
        if(Input.GetMouseButtonDown(0))
        {
            lastClickedTime = Time.time;
            numbClicks++;
            
            if(numbClicks == 1)
            {
                animator.SetTrigger("hit1");
            }
            numbClicks = Mathf.Clamp(numbClicks, 0, 2);
            
        }
    }
  

    public void Attack()
    {
        
        
        Collider[] enemies = Physics.OverlapSphere(attackPoint.position, weaponRange, enemyLayers);
        
        if(enemies.Length > 0)
        {
            EnemyStat stat = enemies[0].GetComponent<EnemyStat>();
            if(stat != null)
            {
                if(isGuarding)
                {
                    damage *= guardDamageReduction; // Reduce damage if guarding
                }
                stat.changeHealth(damage);
            }
        }
        
        
    }

    public void Combohit1Transition()
    {
        if(numbClicks >= 2)
        {
            animator.SetTrigger("hit2");
        }
    }
    


    public void GuardUp()
    {
        animator.SetBool("guardUp", true);    
        isGuarding = true;
        playerStat.speed = playerStat.baseSpeed * 0.2f; // Reduce the player's speed by 50% when guarding
    }
    public void GuardDown()
    {
        animator.SetBool("guardUp", false);
        isGuarding = false;
        playerStat.speed = playerStat.baseSpeed; // Restore the player's speed when not guarding
        
    }
    public bool IsGuarding()
    {
        return isGuarding;
    }

    public void resetAttack()
    {
        numbClicks = 0;
    }

}
