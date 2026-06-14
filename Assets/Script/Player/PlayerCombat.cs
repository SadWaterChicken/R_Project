using System.ComponentModel;
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
    public LayerMask Enemy;
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
        // Create a visual sphere in the actual game to see the hitbox
        GameObject debugSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        debugSphere.transform.position = attackPoint.position;
        // weaponRange is a radius, so we multiply by 2 for the diameter scale
        debugSphere.transform.localScale = new Vector3(weaponRange * 2, weaponRange * 2, weaponRange * 2);
        
        Destroy(debugSphere.GetComponent<Collider>()); // Prevent it from interfering with actual physics
        
        Renderer rend = debugSphere.GetComponent<Renderer>();
        if (rend != null) rend.material.color = new Color(1f, 0f, 0f, 0.5f); // Color it red
        
        Destroy(debugSphere, 0.3f); // Destroy it after 0.3 seconds
        
        // Find ALL colliders in range without using a layer mask
        Collider[] allHit = Physics.OverlapSphere(attackPoint.position, weaponRange);
        
        System.Collections.Generic.HashSet<EnemyStat> hitEnemies = new System.Collections.Generic.HashSet<EnemyStat>();

        foreach (Collider col in allHit)
        {
            // Only damage colliders that are explicitly tagged as "Enemy"
            if (col.CompareTag("Enemy"))
            {
                EnemyStat stat = col.GetComponentInParent<EnemyStat>();
                
                if (stat != null && !hitEnemies.Contains(stat))
                {
                    hitEnemies.Add(stat);

                    float appliedDamage = (playerStat != null) ? playerStat.GetPhysicalDamage() : damage;
                    
                    if(isGuarding)
                    {
                        appliedDamage *= guardDamageReduction;
                    }
                    
                    stat.TakePhysicalDamage(appliedDamage);
                }
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
