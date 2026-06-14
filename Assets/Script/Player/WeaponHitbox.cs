using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponHitbox : MonoBehaviour
{
    public PlayerStat playerStat;
    public float baseDamage = 10f;
    
    // Lưu những kẻ địch đã chém trúng trong nhát chém này để không trừ máu nhiều lần
    private HashSet<EnemyStat> hitEnemies = new HashSet<EnemyStat>();

    private void OnEnable()
    {
        ResetHit();
    }

    // Hàm này được gọi mỗi lần người chơi bấm chuột chém
    public void ResetHit()
    {
        hitEnemies.Clear();
    }

    private void OnTriggerEnter(Collider other)
    {
        ProcessHit(other);
    }

    private void OnTriggerStay(Collider other)
    {
        ProcessHit(other);
    }

    private void ProcessHit(Collider other)
    {
        // Chạm vào Enemy
        EnemyStat enemy = other.GetComponent<EnemyStat>();
        
        // Nếu là enemy và chưa bị chém trúng trong nhát chém này
        if (enemy != null && !hitEnemies.Contains(enemy))
        {
            hitEnemies.Add(enemy);
            
            // Tính toán sát thương
            float appliedDamage = (playerStat != null) ? playerStat.GetPhysicalDamage() : baseDamage;
            
            // Gọi player combat để xem có đang đỡ đòn không (nếu có thì giảm dame)
            PlayerCombat combat = GetComponentInParent<PlayerCombat>();
            if (combat != null && combat.IsGuarding())
            {
                appliedDamage *= 0.5f; // Giảm 50% dame nếu đang thủ (như logic cũ)
            }

            // Trừ máu quái
            enemy.TakePhysicalDamage(appliedDamage);
        }
    }
}
