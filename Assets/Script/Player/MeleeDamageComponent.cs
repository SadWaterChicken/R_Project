using System.Collections.Generic;
using UnityEngine;

public class MeleeDamageComponent : WeaponComponent
{
    [Header("Hitbox Settings")]
    public Transform attackPoint;
    public float weaponRange = 1.5f; // Giá trị fallback
    
    private float physicalDamage;
    private float magicDamage;

    public override void Initialize(WeaponController weaponController)
    {
        base.Initialize(weaponController);

        // Nạp sát thương từ thẻ bài (đã cộng dồn các dòng modify)
        physicalDamage = 0f;
        magicDamage = 0f;
        if (controller.currentItemData != null)
        {
            foreach (var mod in controller.currentItemData.modifiers)
            {
                string statLower = mod.stat.ToLower();
                if (statLower == "physical damage" || statLower == "physicaldamage") 
                    physicalDamage += mod.value;
                else if (statLower == "magic damage" || statLower == "magicdamage") 
                    magicDamage += mod.value;
                else if (statLower == "weapon range" || statLower == "weaponrange" || statLower == "range") 
                    this.weaponRange = mod.value;
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
                    
                    // Lấy Sát thương Cốt lõi của Player + Sát thương RIÊNG của thanh kiếm này
                    float playerCorePhys = PlayerStat.Instance != null ? PlayerStat.Instance.GetPhysicalDamage() : 0f;
                    float playerCoreMagic = PlayerStat.Instance != null ? PlayerStat.Instance.GetMagicDamage() : 0f;

                    if (physicalDamage > 0 || playerCorePhys > 0)
                    {
                        float finalPhys = playerCorePhys + physicalDamage;
                        stat.TakePhysicalDamage(finalPhys);
                        Debug.Log($"[MeleeDamageComponent] Gây {finalPhys} (Kiếm: {physicalDamage} + Core: {playerCorePhys}) sát thương vật lý lên {col.name}");
                    }

                    if (magicDamage > 0 || playerCoreMagic > 0)
                    {
                        float finalMagic = playerCoreMagic + magicDamage;
                        stat.TakeMagicDamage(finalMagic);
                        Debug.Log($"[MeleeDamageComponent] Gây {finalMagic} (Kiếm: {magicDamage} + Core: {playerCoreMagic}) sát thương phép lên {col.name}");
                    }
                }
            }
        }
    }
}
