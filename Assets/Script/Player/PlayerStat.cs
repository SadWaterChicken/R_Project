using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class PlayerStat : MonoBehaviour
{
    public static PlayerStat Instance { get; private set; }

    [Header("Health Stats")]
    public int maxHealth = 100;
    public float currentHealth;
    public float healthRegenRate = 0f;

    [Header("Mana Stats")]
    public int maxMana = 100;
    public float currentMana;
    public float manaRegenRate = 2f; // per second

    [Header("Sanity Stats")]
    public int maxSanity = 100;
    public float currentSanity;
    public float sanityRegenRate = 5f; // per second

    [Header("Damage Stats")]
    public float basePhysicalDamage = 10f;
    public float physicalDamageBonus = 0f;
    public float baseMagicDamage = 5f;
    public float magicDamageBonus = 0f;

    [Header("Defence Stats")]
    public float physicalArmour = 0f;
    public float magicArmour = 0f;

    [Header("Movement & Combat")]
    public float baseSpeed = 6f;
    public float movementSpeed = 6f;
    public float attackSpeed = 1f;
    public float dashCooldown = 0.5f;
    public bool isInvincible = false;

    [Header("Luck & Crit")]
    public float luck = 0f; // Affects drop rate and crit chance
    public float critChance = 0f;

    [Header("Shield Stats")]
    public float shield = 0f;
    public float maxShield = 50f;
    public float shieldRegenRate = 0f;
    public float shieldRechargeCooldown = 5f; // Time before shield starts regenerating

    [Header("Economy")]
    public int gold = 0;

    private float shieldRechargeTimer = 0f;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void OnEnable()
    {
        if (Inventory.Instance != null)
        {
            Inventory.Instance.OnItemEquipChanged += OnItemEquipChanged;
            Debug.Log("[PlayerStat] Subscribed to Inventory.OnItemEquipChanged");
        }
        else
        {
            Debug.LogWarning("[PlayerStat] Inventory.Instance is null in OnEnable!");
        }
    }

    private void OnDisable()
    {
        if (Inventory.Instance != null)
        {
            Inventory.Instance.OnItemEquipChanged -= OnItemEquipChanged;
        }
    }

    private void Start()
    {
        currentHealth = maxHealth;
        currentMana = maxMana;
        currentSanity = maxSanity;
        shield = 0f;
        shieldRechargeTimer = 0f;
    }

    private void Update()
    {
        // Regenerate health
        if (currentHealth < maxHealth && healthRegenRate > 0)
        {
            currentHealth += healthRegenRate * Time.deltaTime;
            currentHealth = Mathf.Min(currentHealth, maxHealth);
        }

        // Regenerate mana
        if (currentMana < maxMana && manaRegenRate > 0)
        {
            currentMana += manaRegenRate * Time.deltaTime;
            currentMana = Mathf.Min(currentMana, maxMana);
        }

        // Regenerate sanity
        if (currentSanity < maxSanity && sanityRegenRate > 0)
        {
            currentSanity += sanityRegenRate * Time.deltaTime;
            currentSanity = Mathf.Min(currentSanity, maxSanity);
        }

        // Shield regeneration logic
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

    private void OnDestroy()
    {
        if (Inventory.Instance != null)
        {
            Inventory.Instance.OnItemEquipChanged -= OnItemEquipChanged;
        }
    }

    // Called when item is equipped/unequipped
    private void OnItemEquipChanged(ItemData item, bool equipped)
    {
        Debug.Log($"[PlayerStat] OnItemEquipChanged called: {item?.itemName} equipped={equipped}");
        UpdateStatsForItem(item, equipped);
    }

    private void UpdateStatsForItem(ItemData item, bool isEquipped)
    {
        if (item?.modifiers == null || item.modifiers.Count == 0)
            return;

        float multiplier = isEquipped ? 1f : -1f;

        foreach (var mod in item.modifiers)
        {
            float value = mod.value * multiplier;
            Debug.Log($"[PlayerStat] {(isEquipped ? "Equipping" : "Unequipping")} {item.itemName}: {mod.stat} {(isEquipped ? "+" : "-")}{Mathf.Abs(mod.value)}");

            switch (mod.stat.ToLower())
            {
                case "physical damage":
                case "physicaldamage":
                case "physical damage bonus":
                case "physicaldamagebonus":
                    physicalDamageBonus += value;
                    break;
                case "magic damage":
                case "magicdamage":
                case "magic damage bonus":
                case "magicdamagebonus":
                    magicDamageBonus += value;
                    break;
                case "physical armour":
                case "physicalarmour":
                case "physical armor":
                case "physicalarmor":
                    physicalArmour += value;
                    break;
                case "magic armour":
                case "magicarmour":
                case "magic armor":
                case "magicarmor":
                    magicArmour += value;
                    break;
                case "max health":
                case "maxhealth":
                    maxHealth += (int)value;
                    break;
                case "max mana":
                case "maxmana":
                    maxMana += (int)value;
                    break;
                case "max shield":
                case "maxshield":
                    maxShield += value;
                    break;
                case "movement speed":
                case "movementspeed":
                    movementSpeed += value;
                    break;
                case "attack speed":
                case "attackspeed":
                    attackSpeed += value;
                    break;
                case "crit chance":
                case "critchance":
                    critChance += value;
                    break;
                case "luck":
                    luck += value;
                    break;
                default:
                    Debug.LogWarning($"[PlayerStat] Unknown stat type: {mod.stat}");
                    break;
            }
        }

        Debug.Log($"[PlayerStat] Stats updated - PhysicalDmg: {GetPhysicalDamage()}, MagicDmg: {GetMagicDamage()}, Armour: {physicalArmour}/{magicArmour}");
    }

    // ─── Damage Getters ────────────────────────────────────────────────────────
    public float GetPhysicalDamage()
    {
        float buffBonus = PlayerBuffManager.Instance?.GetBuffValue(DungeonBuff.BuffType.DamageBoost) ?? 0f;
        return basePhysicalDamage + physicalDamageBonus + (basePhysicalDamage * buffBonus);
    }

    public float GetMagicDamage()
    {
        float buffBonus = PlayerBuffManager.Instance?.GetBuffValue(DungeonBuff.BuffType.DamageBoost) ?? 0f;
        return baseMagicDamage + magicDamageBonus + (baseMagicDamage * buffBonus);
    }

    public float GetMovementSpeed()
    {
        float buffBonus = PlayerBuffManager.Instance?.GetBuffValue(DungeonBuff.BuffType.SpeedBoost) ?? 0f;
        return movementSpeed + (baseSpeed * buffBonus);
    }

    public float GetCritChance()
    {
        float luckBonus = luck * 0.1f; // 1 luck = 0.1% crit
        float buffBonus = PlayerBuffManager.Instance?.GetBuffValue(DungeonBuff.BuffType.CritChance) ?? 0f;
        return Mathf.Clamp01(critChance + luckBonus + buffBonus);
    }

    public float GetGoldMultiplier()
    {
        float buffBonus = PlayerBuffManager.Instance?.GetBuffValue(DungeonBuff.BuffType.GoldMultiplier) ?? 0f;
        return 1f + buffBonus;
    }

    public float GetDropRateMultiplier()
    {
        float luckBonus = luck * 0.05f; // 1 luck = 0.05 drop rate bonus
        float buffBonus = PlayerBuffManager.Instance?.GetBuffValue(DungeonBuff.BuffType.DropRate) ?? 0f;
        return 1f + luckBonus + buffBonus;
    }

    public float GetDefense()
    {
        return (physicalArmour + magicArmour) / 2f;
    }

    public float GetMaxHealth()
    {
        return maxHealth;
    }

    // ─── Damage Application ────────────────────────────────────────────────────
    public void TakeDamage(float damage, float armorPenetration = 0f)
    {
        if (isInvincible) return;

        float reduction = (physicalArmour * (1f - armorPenetration)) / 100f;
        float finalDamage = damage * (1f - reduction);

        // Apply shield first
        if (shield > 0)
        {
            float shieldDamage = Mathf.Min(finalDamage, shield);
            shield -= shieldDamage;
            finalDamage -= shieldDamage;
            shieldRechargeTimer = shieldRechargeCooldown;
        }

        if (finalDamage > 0)
        {
            currentHealth -= finalDamage;
            if (currentHealth <= 0)
            {
                Die();
            }
        }

        Debug.Log($"[PlayerStat] Took {finalDamage} damage. Health: {currentHealth}/{maxHealth}");
    }

    public void TakeMagicDamage(float damage, float resistancePenetration = 0f)
    {
        if (isInvincible) return;

        float reduction = (magicArmour * (1f - resistancePenetration)) / 100f;
        float finalDamage = damage * (1f - reduction);

        currentHealth -= finalDamage;
        if (currentHealth <= 0)
        {
            Die();
        }

        Debug.Log($"[PlayerStat] Took {finalDamage} magic damage. Health: {currentHealth}/{maxHealth}");
    }

    public void Heal(float amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Min(currentHealth, maxHealth);
        Debug.Log($"[PlayerStat] Healed {amount}. Health: {currentHealth}/{maxHealth}");
    }

    public void RestoreShield(float amount)
    {
        shield += amount;
        shield = Mathf.Min(shield, maxShield);
        shieldRechargeTimer = shieldRechargeCooldown;
    }

    private void Die()
    {
        Debug.Log("[PlayerStat] Player has died.");
        // TODO: Handle player death (animation, UI, respawn, etc.)
    }

    // ─── Economy ────────────────────────────────────────────────────────────────
    public int GetGold()
    {
        return gold;
    }

    public void AddGold(int amount)
    {
        if (amount > 0)
        {
            float multiplier = GetGoldMultiplier();
            int finalAmount = (int)(amount * multiplier);
            gold += finalAmount;
            Debug.Log($"[PlayerStat] Added {finalAmount} gold (base: {amount}, multiplier: {multiplier}x). Total: {gold}");
        }
    }

    public bool SpendGold(int amount)
    {
        if (amount <= 0) return true;
        if (gold < amount) return false;
        gold -= amount;
        Debug.Log($"[PlayerStat] Spent {amount} gold. Remaining: {gold}");
        return true;
    }

    // ─── Mana Management ────────────────────────────────────────────────────────
    public bool ConsumeMana(float amount)
    {
        if (currentMana < amount) return false;
        currentMana -= amount;
        return true;
    }

    public void RestoreMana(float amount)
    {
        currentMana += amount;
        currentMana = Mathf.Min(currentMana, maxMana);
    }

    // ─── Sanity Management ──────────────────────────────────────────────────────
    public void ReduceSanity(float amount)
    {
        currentSanity -= amount;
        currentSanity = Mathf.Max(currentSanity, 0);
    }

    public void RestoreSanity(float amount)
    {
        currentSanity += amount;
        currentSanity = Mathf.Min(currentSanity, maxSanity);
    }
}
