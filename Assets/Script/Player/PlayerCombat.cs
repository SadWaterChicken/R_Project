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
    public float damage;
    public int comboCounter;
    private bool isAnimationPlaying = false;
    private bool isGuarding = false;
    private float guardDamageReduction = 0.5f; // Reduce damage by 50% when guarding

    void Update()
    {
        if(Timer > 0)
        {
            Timer -= Time.deltaTime;
        }
    }
  

    public void Attack()
    {
        if(Timer <= 0 && !isAnimationPlaying)// Check if the attack is ready and no attack animation is currently playing
        {
            animator.SetBool("hit1", false);
            animator.SetBool("hit2", false);
            //count combo attacks here and set the correct animation bools
            comboCounter++;
            if (comboCounter > 2)
            {
                comboCounter = 1;
            }
            if (comboCounter == 1)
            {
                animator.SetBool("hit1", true);
            }
            else if (comboCounter == 2)
            {
                animator.SetBool("hit2", true);
            }

            isAnimationPlaying = true;
            Timer = Cooldown;
        
        Collider[] enemies = Physics.OverlapSphere(attackPoint.position, weaponRange, enemyLayers);
        
        if(enemies.Length > 0)
        {
            EnemyStat stat = enemies[0].GetComponent<EnemyStat>();
            if(stat != null)
            {
                stat.changeHealth(damage);
            }
        }
        
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


    public void FinishHit1Attack()
    {
        animator.SetBool("hit1", false);
        isAnimationPlaying = false;
    }

    public void FinishHit2Attack()
    {
        animator.SetBool("hit2", false);
        isAnimationPlaying = false;
    }


}
