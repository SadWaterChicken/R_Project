using UnityEngine;
using System.Linq;

/// <summary>
/// Gắn vào enemy khi bị dính Fire Blade Slash. Tự gây DoT và tự hủy sau duration.
/// </summary>
public class FireBurnDoT : MonoBehaviour
{
    private float damagePerSecond;
    private float duration;
    private float elapsed;
    private float tickInterval = 0.5f;
    private float tickTimer;

    private CharacterStats targetStats;

    public void Initialize(CharacterStats target, float dps, float dur)
    {
        targetStats = target;
        damagePerSecond = dps;
        duration = dur;
        elapsed = 0f;
        tickTimer = 0f;
    }

    private void Update()
    {
        if (targetStats == null)
        {
            Destroy(this);
            return;
        }

        elapsed += Time.deltaTime;
        tickTimer += Time.deltaTime;

        if (tickTimer >= tickInterval)
        {
            tickTimer -= tickInterval;
            float tickDamage = damagePerSecond * tickInterval;
            
            float hpBefore = targetStats.currentHealth;
            // Dùng magic damage để tránh bị chặn hoàn toàn bởi physical armor
            targetStats.TakeMagicDamage(tickDamage, 0f, false);
            
            if (hpBefore > 0 && targetStats.currentHealth <= 0)
            {
                RewardMasteryOnDoTKill();
            }
        }

        if (elapsed >= duration)
        {
            Destroy(this);
        }
    }

    private void RewardMasteryOnDoTKill()
    {
        if (Inventory.Instance != null && ForgeManager.Instance != null)
        {
            var equippedWeapons = Inventory.Instance.ownedItems
                .Where(item => item.equipped && !string.IsNullOrEmpty(item.weaponClassName))
                .ToList();

            foreach (var weapon in equippedWeapons)
            {
                EnemyMasteryReward rewardComp = targetStats.GetComponentInParent<EnemyMasteryReward>();
                float masteryReward = rewardComp != null ? rewardComp.masteryGranted : 1f;
                if (rewardComp == null && targetStats is EnemyStat eStat)
                {
                    masteryReward = eStat.enemyLevel * 1f;
                }
                ForgeManager.Instance.AddMasteryOnKill(weapon, masteryReward);
            }
        }
    }
}
