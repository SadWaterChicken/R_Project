using UnityEngine;
using System.Linq;

public abstract class BaseSkill : MonoBehaviour
{
    protected PlayerStat caster;
    protected float damageMultiplier = 1f;
    protected float weaponPhysicalDamage;

    public virtual void Initialize(PlayerStat casterStat, float multiplier, float weaponDamage)
    {
        caster = casterStat;
        damageMultiplier = Mathf.Max(0f, multiplier);
        weaponPhysicalDamage = Mathf.Max(0f, weaponDamage);
    }

    public abstract void ExecuteSkill();

    protected float CalculatePhysicalDamage()
    {
        float playerDamage = caster != null ? caster.GetPhysicalDamage() : 0f;
        return (playerDamage + weaponPhysicalDamage) * damageMultiplier;
    }

    protected void RewardMasteryOnSkillKill(GameObject enemyObj, CharacterStats enemyStat)
    {
        if (Inventory.Instance != null && ForgeManager.Instance != null)
        {
            var equippedWeapons = Inventory.Instance.ownedItems
                .Where(item => item.equipped && !string.IsNullOrEmpty(item.weaponClassName))
                .ToList();

            foreach (var weapon in equippedWeapons)
            {
                EnemyMasteryReward rewardComp = enemyObj.GetComponentInParent<EnemyMasteryReward>();
                float masteryReward = rewardComp != null ? rewardComp.masteryGranted : 1f;
                if (rewardComp == null && enemyStat is EnemyStat eStat)
                {
                    masteryReward = eStat.enemyLevel * 1f;
                }
                ForgeManager.Instance.AddMasteryOnKill(weapon, masteryReward);
            }
        }
    }
}
