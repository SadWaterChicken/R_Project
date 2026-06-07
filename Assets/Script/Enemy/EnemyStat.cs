using UnityEngine;
using System.Collections;

public class EnemyStat : MonoBehaviour
{
    [Header("General")]
    public int enemyLevel = 1;

    [Header("Health Stats")]
    public int maxHealth = 100;
    public float currentHealth;
    public float healthRegenRate = 0f;

    [Header("Mana Stats")]
    public int maxMana = 100;
    public float currentMana;
    public float manaRegenRate = 0f;

    [Header("Damage Stats")]
    public float basePhysicalDamage = 10f;
    public float physicalDamageBonus = 0f;
    public float baseMagicDamage = 5f;
    public float magicDamageBonus = 0f;

    [Header("Defence Stats")]
    public float physicalArmor = 0f;
    public float magicArmor = 0f;

    [Header("Movement & Combat")]
    public float baseSpeed = 6f;
    public float movementSpeed = 6f;
    public float attackSpeed = 1f;
    public float attackRange = 2f;
    public float attackCooldown = 1f;
    public float detectionRange = 10f;

    [Header("Luck & Crit")]
    public float critChance = 0f;

    [Header("Shield Stats")]
    public float shield = 0f;
    public float maxShield = 0f;
    public float shieldRegenRate = 0f;
    public float shieldRechargeCooldown = 5f;

    private float shieldRechargeTimer = 0f;
    private float lastDamageTime;
    public float outOfCombatDelay = 3f;

    private void Start()
    {
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
            basePhysicalDamage *= finalMulti;
            baseMagicDamage *= finalMulti;
            // (Armor and speed are kept original so enemies don't become too fast or unkillable, 
            // but you can scale them here too if you want)
        }

        currentHealth = maxHealth;
        currentMana = maxMana;
        shield = 0f;
        shieldRechargeTimer = 0f;
        lastDamageTime = Time.time;
    }

    private void Update()
    {
        // Regenerate health if out of combat
        if (currentHealth < maxHealth && healthRegenRate > 0)
        {
            if (Time.time - lastDamageTime >= outOfCombatDelay)
            {
                currentHealth += healthRegenRate * Time.deltaTime;
                currentHealth = Mathf.Min(currentHealth, maxHealth);
            }
        }

        // Regenerate mana
        if (currentMana < maxMana && manaRegenRate > 0)
        {
            currentMana += manaRegenRate * Time.deltaTime;
            currentMana = Mathf.Min(currentMana, maxMana);
        }

        UpdateShieldRegeneration();
    }

    private void UpdateShieldRegeneration()
    {
        if (shield >= maxShield) return;

        shieldRechargeTimer -= Time.deltaTime;
        if (shieldRechargeTimer <= 0 && shieldRegenRate > 0)
        {
            shield += shieldRegenRate * Time.deltaTime;
            shield = Mathf.Min(shield, maxShield);
        }
    }

    // ─── Damage Getters ────────────────────────────────────────────────────────
    public float GetPhysicalDamage()
    {
        return basePhysicalDamage + physicalDamageBonus;
    }

    public float GetMagicDamage()
    {
        return baseMagicDamage + magicDamageBonus;
    }

    // ─── Damage Application ────────────────────────────────────────────────────
    public void TakePhysicalDamage(float damage, float armorPenetration = 0f)
    {
        float reduction = (physicalArmor * (1f - armorPenetration)) / 100f;
        float finalDamage = damage * (1f - reduction);
        
        // Apply shield first
        if (shield > 0)
        {
            float shieldDamage = Mathf.Min(finalDamage, shield);
            shield -= shieldDamage;
            finalDamage -= shieldDamage;
            shieldRechargeTimer = shieldRechargeCooldown;
        }

        ApplyDamage(finalDamage);
        Debug.Log($"[EnemyStat] Took {finalDamage} physical damage. Health: {currentHealth}/{maxHealth}");
    }

    public void TakeMagicDamage(float damage, float resistancePenetration = 0f)
    {
        float reduction = (magicArmor * (1f - resistancePenetration)) / 100f;
        float finalDamage = damage * (1f - reduction);
        
        // Apply shield first
        if (shield > 0)
        {
            float shieldDamage = Mathf.Min(finalDamage, shield);
            shield -= shieldDamage;
            finalDamage -= shieldDamage;
            shieldRechargeTimer = shieldRechargeCooldown;
        }

        ApplyDamage(finalDamage);
        Debug.Log($"[EnemyStat] Took {finalDamage} magic damage. Health: {currentHealth}/{maxHealth}");
    }

    public void changeHealth(float damage)
    {
        // Legacy method, applies raw damage (true damage) bypassing armor
        ApplyDamage(damage);
    }

    private void ApplyDamage(float finalDamage)
    {
        lastDamageTime = Time.time;
        
        if (finalDamage > 0)
        {
            currentHealth -= finalDamage;
            if (currentHealth <= 0)
            {
                Destroy(gameObject);
            }
        }
    }
}
