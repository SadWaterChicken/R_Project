using UnityEngine;
using System.Collections;

public class EnemyStat : CharacterStats
{
    [Header("General")]
    public int enemyLevel = 1;

    [Header("Combat Ranges")]
    public float attackRange = 2f;
    public float attackCooldown = 1f;
    public float detectionRange = 10f;

    public float outOfCombatDelay = 3f;

    protected override void Start()
    {
        base.Start();

        // Check if there's a difficulty multiplier applied by the Dungeon Spawner
        RoomEnemyTracker tracker = GetComponent<RoomEnemyTracker>();
        if (tracker != null && tracker.statMultiplier != 1.0f)
        {
            float multi = tracker.statMultiplier;
            
            // Example of Level Scaling: +10% stats per level beyond 1
            float levelScale = 1f + (Mathf.Max(1, enemyLevel) - 1) * 0.1f;
            float finalMulti = multi * levelScale;

            // Apply multiplier to core stats
            maxHealth = Mathf.RoundToInt(maxHealth * finalMulti);
            currentHealth = maxHealth;
            basePhysicalDamage *= finalMulti;
            baseMagicDamage *= finalMulti;
            // (Armor and speed are kept original so enemies don't become too fast or unkillable, 
            // but you can scale them here too if you want)
        }
    }

    protected override void Update()
    {
        base.Update();
        
        // Regenerate health if out of combat
        if (currentHealth < maxHealth && healthRegenRate > 0)
        {
            if (Time.time - lastDamageTime >= outOfCombatDelay)
            {
                currentHealth += healthRegenRate * Time.deltaTime;
                currentHealth = Mathf.Min(currentHealth, maxHealth);
            }
        }
    }

    public void changeHealth(float damage)
    {
        // Legacy method, applies raw damage (true damage) bypassing armor
        ProcessDamage(damage);
    }

    protected override void Die()
    {
        Destroy(gameObject);
    }
}
