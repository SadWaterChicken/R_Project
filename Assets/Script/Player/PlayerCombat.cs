using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    public Animator animator;
    public PlayerStat playerStat;
    public float ComboDelay = 0.5f;
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
        // Reset combo if too much time passed
        if(Time.time - lastClickedTime > ComboDelay)
        {
            if (numbClicks > 0)
            {
                numbClicks = 0;
                animator.ResetTrigger("hit1");
                animator.ResetTrigger("hit2");
            }
        }

        if(Input.GetMouseButtonDown(0))
        {
            lastClickedTime = Time.time;
            numbClicks++;
            
            if(numbClicks == 1)
            {
                animator.ResetTrigger("hit2"); // Clear any ghost triggers
                animator.SetTrigger("hit1");
            }
            else if (numbClicks >= 2)
            {
                // Queue hit2 instantly so you don't miss the animation event window
                animator.SetTrigger("hit2");
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
        playerStat.movementSpeed = playerStat.baseSpeed * 0.2f; // Reduce the player's speed by 80% when guarding
    }
    public void GuardDown()
    {
        animator.SetBool("guardUp", false);
        isGuarding = false;
        playerStat.movementSpeed = playerStat.baseSpeed; // Restore the player's speed when not guarding
        
    }
    public bool IsGuarding()
    {
        return isGuarding;
    }

    public void resetAttack()
    {
        numbClicks = 0;
        animator.ResetTrigger("hit1");
        animator.ResetTrigger("hit2");
    }

}
