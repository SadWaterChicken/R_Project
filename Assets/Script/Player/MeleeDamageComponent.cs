using System.Collections.Generic;
using UnityEngine;

public class MeleeDamageComponent : WeaponComponent
{
    [Header("Hitbox Settings")]
    public Transform attackPoint;
    public float weaponRange = 1.5f; // Giá trị fallback
    
    private float physicalDamage;

    public override void Initialize(WeaponController weaponController)
    {
        base.Initialize(weaponController);

        // Nạp sát thương từ thẻ bài (đã cộng dồn các dòng modify)
        physicalDamage = 0f;
        if (controller.currentItemData != null)
        {
            foreach (var mod in controller.currentItemData.modifiers)
            {
                if (mod.stat == "physicalDamage") physicalDamage += mod.value;
                if (mod.stat == "weaponRange") this.weaponRange = mod.value;
            }
        }
        
        // Cố gắng tìm attackPoint tự động nếu chưa gán
        if (attackPoint == null)
        {
            attackPoint = this.transform; 
        }
    }

    public override void OnAnimationEvent(string eventName)
    {
        if (eventName == "HitFrame")
        {
            ExecuteDamage();
        }
    }

    private void ExecuteDamage()
    {
        Collider[] allHit = Physics.OverlapSphere(attackPoint.position, weaponRange);
        HashSet<EnemyStat> hitEnemies = new HashSet<EnemyStat>();

        foreach (Collider col in allHit)
        {
            if (col.CompareTag("Enemy"))
            {
                EnemyStat stat = col.GetComponentInParent<EnemyStat>();
                if (stat != null && !hitEnemies.Contains(stat))
                {
                    hitEnemies.Add(stat);
                    stat.TakePhysicalDamage(physicalDamage);
                    Debug.Log($"[MeleeDamageComponent] Gây {physicalDamage} sát thương lên {col.name}");
                }
            }
        }
    }
}
