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
                    
                    // Mastery không còn tăng sát thương, chỉ để mở khóa Rèn
                    float masteryMultiplier = 1f;

                    // Lấy Sát thương Cốt lõi của Player (AD) + Sát thương RIÊNG của thanh kiếm này (có nhân Mastery)
                    float playerCorePhys = PlayerStat.Instance != null ? PlayerStat.Instance.GetPhysicalDamage() : 0f;
                    // Bỏ Core Magic theo quy tắc đánh thường: 100% AD, không dùng AP cho đánh thường cơ bản

                    float healthBefore = stat.currentHealth;

                    float finalPhys = 0f;
                    float finalMagic = 0f;
                    bool isCrit = false;

                    // Tính Crit cho sát thương vật lý
                    if (PlayerStat.Instance != null)
                    {
                        float critChance = PlayerStat.Instance.GetCritChance();
                        if (Random.Range(0f, 100f) <= critChance)
                        {
                            isCrit = true;
                        }
                    }

                    if (physicalDamage > 0 || playerCorePhys > 0)
                    {
                        finalPhys = (playerCorePhys + physicalDamage) * masteryMultiplier;
                        if (isCrit && PlayerStat.Instance != null)
                        {
                            finalPhys *= PlayerStat.Instance.GetCritDamage();
                        }
                    }

                    // Sát thương phép từ vũ khí (On-hit Magic Damage)
                    if (magicDamage > 0)
                    {
                        finalMagic = magicDamage * masteryMultiplier; // Không chí mạng
                    }

                    if (finalPhys > 0 || finalMagic > 0)
                    {
                        stat.TakeMixedDamage(finalPhys, finalMagic, 0f, 0f, isCrit);
                        Debug.Log($"[MeleeDamageComponent] Gây sát thương hỗn hợp lên {col.name}. (Phys: {finalPhys:F1}, Magic: {finalMagic:F1}, Crit: {isCrit})");
                    }

                    // Trick: Kiểm tra nếu nhát chém này đã kết liễu quái vật
                    if (healthBefore > 0 && stat.currentHealth <= 0)
                    {
                        // 1. Rơi đồ (Loot)
                        EnemyLootTable lootTable = col.GetComponentInParent<EnemyLootTable>();
                        if (lootTable != null)
                        {
                            lootTable.DropLoot(stat.transform.position);
                        }

                        // 2. Cộng điểm Mastery
                        if (controller.currentItemData != null && ForgeManager.Instance != null)
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
