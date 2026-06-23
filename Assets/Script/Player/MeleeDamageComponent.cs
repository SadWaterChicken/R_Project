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
                    
                    // Tính hệ số sức mạnh từ Mastery (Ví dụ: 1000 Mastery = Tăng 100% sát thương)
                    float masteryMultiplier = 1f;
                    if (controller.currentItemData != null)
                    {
                        masteryMultiplier += (controller.currentItemData.weaponMastery / 1000f);
                    }

                    // Lấy Sát thương Cốt lõi của Player + Sát thương RIÊNG của thanh kiếm này (có nhân Mastery)
                    float playerCorePhys = PlayerStat.Instance != null ? PlayerStat.Instance.GetPhysicalDamage() : 0f;
                    float playerCoreMagic = PlayerStat.Instance != null ? PlayerStat.Instance.GetMagicDamage() : 0f;

                    float healthBefore = stat.currentHealth;

                    if (physicalDamage > 0 || playerCorePhys > 0)
                    {
                        float finalPhys = (playerCorePhys + physicalDamage) * masteryMultiplier;
                        stat.TakePhysicalDamage(finalPhys);
                        Debug.Log($"[MeleeDamageComponent] Gây {finalPhys} (Kiếm: {physicalDamage} + Core: {playerCorePhys} x Mastery: {masteryMultiplier:F2}) sát thương vật lý lên {col.name}");
                    }

                    if (magicDamage > 0 || playerCoreMagic > 0)
                    {
                        float finalMagic = (playerCoreMagic + magicDamage) * masteryMultiplier;
                        stat.TakeMagicDamage(finalMagic);
                        Debug.Log($"[MeleeDamageComponent] Gây {finalMagic} (Kiếm: {magicDamage} + Core: {playerCoreMagic} x Mastery: {masteryMultiplier:F2}) sát thương phép lên {col.name}");
                    }

                    // Trick: Kiểm tra nếu nhát chém này đã kết liễu quái vật
                    if (healthBefore > 0 && stat.currentHealth <= 0 && controller.currentItemData != null)
                    {
                        if (ForgeManager.Instance != null)
                        {
                            // Tìm component Reward riêng, nếu không có thì lấy mặc định theo Level quái
                            EnemyMasteryReward rewardComp = col.GetComponentInParent<EnemyMasteryReward>();
                            float masteryReward = rewardComp != null ? rewardComp.masteryGranted : stat.enemyLevel * 1f;
                            
                            ForgeManager.Instance.AddMasteryOnKill(controller.currentItemData, masteryReward);
                        }
                    }
                }
            }
        }
    }
}
