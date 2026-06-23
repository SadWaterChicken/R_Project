using System.ComponentModel;
using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    public Animator animator;
    public EquipmentManager equipmentManager;
    public PlayerStat playerStat;
    public float ComboDelay = 0.5f;
    private int numbClicks = 0;
    private float lastClickedTime = 3;
    public Transform attackPoint;
    public float weaponRange = 10;
    public LayerMask Enemy;
    public float damage;
    private bool isGuarding = false;
    public float guardDamageReduction = 0.5f; // Reduce damage by 50% when guarding

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
                if (equipmentManager != null) equipmentManager.TriggerMainHandAttack();
            }
            else if (numbClicks >= 2)
            {
                // Queue hit2 instantly so you don't miss the animation event window
                animator.SetTrigger("hit2");
                if (equipmentManager != null) equipmentManager.TriggerMainHandAttack();
            }
            
            numbClicks = Mathf.Clamp(numbClicks, 0, 2);
        }

        // Chuột Phải: Xử lý theo loại vũ khí tay trái
        if (Input.GetMouseButtonDown(1))
        {
            if (equipmentManager != null && equipmentManager.HasOffHandWeapon())
            {
                // Kiểm tra xem tay trái đang cầm Khiên (Defend) hay Vũ khí (Melee/Ranged)
                if (equipmentManager.GetOffHandCombatStyle() == CombatStyle.Defend)
                {
                    GuardUp();
                }
                else
                {
                    // Vung vũ khí tay trái (Tạm dùng hit2 cho tay trái)
                    animator.ResetTrigger("hit1");
                    animator.SetTrigger("hit2");
                    equipmentManager.TriggerOffHandAttack();
                }
            }
            else
            {
                // Tay không cũng đỡ đòn
                GuardUp();
            }
        }
        
        if (Input.GetMouseButtonUp(1))
        {
            if (equipmentManager == null || !equipmentManager.HasOffHandWeapon())
            {
                GuardDown();
            }
        }
    }
  

    public void Attack()
    {
        // [DEPRECATED] Logic gây sát thương cũ đã được gỡ bỏ.
        // Hàm này được giữ lại với nội dung rỗng để Animator cũ gọi vào không bị văng lỗi Missing Method.
        // Sát thương thực tế đã được chuyển sang Component mới.
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
