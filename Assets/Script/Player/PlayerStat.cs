using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class PlayerStat : CharacterStats
{
    public static PlayerStat Instance { get; private set; }

    [Header("Sanity Stats")]
    public int maxSanity = 100;
    public float currentSanity;
    public float sanityRegenRate = 5f; // per second

    [Header("Luck")]
    public float luck = 0f; // Affects drop rate and crit chance

    [Header("Movement Options")]
    public float dashCooldown = 0.5f;
    public bool isInvincible = false;
    private bool isDead = false;

    [Header("Economy")]
    public int gold = 0;
    public int currentEnergyCubes = 0;

    [Header("Weapon Class Masteries (View Only)")]
    public float greatswordMastery;
    public float katanaMastery;
    public float warhammerMastery;
    public float greatsaxeMastery;
    public float spearMastery;
    public float bowMastery;
    public float staffMastery;
    public float orbMastery;

    public void UpdateMasteryDisplay(string className, float masteryExp)
    {
        switch (className.ToLower())
        {
            case "greatsword": greatswordMastery = masteryExp; break;
            case "katana": katanaMastery = masteryExp; break;
            case "warhammer": warhammerMastery = masteryExp; break;
            case "greatsaxe": greatsaxeMastery = masteryExp; break;
            case "spear": spearMastery = masteryExp; break;
            case "bow": bowMastery = masteryExp; break;
            case "staff": staffMastery = masteryExp; break;
            case "orb": orbMastery = masteryExp; break;
            default:
                Debug.LogWarning($"[PlayerStat] Unknown weapon class: {className}");
                break;
        }
    }

    protected override void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        base.Awake();
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

    protected override void Start()
    {
        base.Start();
        currentSanity = maxSanity;
    }

    protected override void Update()
    {
        base.Update();

        // Hard check for testing purpose (so if someone sets health to 0 in Inspector, they die)
        if (currentHealth <= 0 && !isDead)
        {
            Die();
        }

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

            if (item.BaseData != null && item.BaseData.equipmentType == EquipmentType.Weapon)
            {
                string statLower = mod.stat.ToLower();
                if (statLower == "physical damage" || statLower == "physicaldamage" || 
                    statLower == "magic damage" || statLower == "magicdamage")
                {
                    // Lọc sát thương vũ khí ra, chỉ những món đồ khác (Áo, Mũ) hoặc Dòng Bonus khác mới được cộng vào Core
                    continue; 
                }
            }

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
                    physicalArmor += value;
                    break;
                case "magic armour":
                case "magicarmour":
                case "magic armor":
                case "magicarmor":
                    magicArmor += value;
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

        Debug.Log($"[PlayerStat] Stats updated - PhysicalDmg: {GetPhysicalDamage()}, MagicDmg: {GetMagicDamage()}, Armour: {physicalArmor}/{magicArmor}");
    }

    // ─── Stat Getters (Overrides) ──────────────────────────────────────────────
    public override float GetCritChance()
    {
        float luckBonus = luck * 0.1f; // 1 luck = 0.1% crit
        return Mathf.Clamp01(base.GetCritChance() + luckBonus);
    }

    public float GetGoldMultiplier()
    {
        float buffBonus = buffManager != null ? buffManager.GetBuffValue(DungeonBuff.BuffType.GoldMultiplier) : 0f;
        return 1f + buffBonus;
    }

    public float GetDropRateMultiplier()
    {
        float luckBonus = luck * 0.05f; // 1 luck = 0.05 drop rate bonus
        float buffBonus = buffManager != null ? buffManager.GetBuffValue(DungeonBuff.BuffType.DropRate) : 0f;
        return 1f + luckBonus + buffBonus;
    }

    // ─── Damage Application ────────────────────────────────────────────────────
    public override void TakePhysicalDamage(float damage, float armorPenetration = 0f)
    {
        if (isInvincible) return;
        
        PlayerCombat combat = GetComponent<PlayerCombat>();
        if (combat != null && combat.IsGuarding())
        {
            damage *= combat.guardDamageReduction;
            Debug.Log($"[PlayerStat] Guarding! Reduced incoming physical damage to {damage}");
        }

        base.TakePhysicalDamage(damage, armorPenetration);
    }

    public override void TakeMagicDamage(float damage, float resistancePenetration = 0f)
    {
        if (isInvincible) return;

        PlayerCombat combat = GetComponent<PlayerCombat>();
        if (combat != null && combat.IsGuarding())
        {
            damage *= combat.guardDamageReduction;
            Debug.Log($"[PlayerStat] Guarding! Reduced incoming magic damage to {damage}");
        }

        base.TakeMagicDamage(damage, resistancePenetration);
    }

    protected override void Die()
    {
        if (isDead) return;
        isDead = true;

        Debug.Log("[PlayerStat] Player has died.");
        
        // Mất đồ trong túi tạm khi chết
        if (DungeonSack.Instance != null)
        {
            DungeonSack.Instance.Clear();
        }

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

    public bool CanSpendGold(int amount)
    {
        return gold >= amount;
    }

    // ─── Energy Cubes ───────────────────────────────────────────────────────────
    public void AddEnergyCubes(int amount)
    {
        if (amount > 0)
        {
            currentEnergyCubes += amount;
            Debug.Log($"[PlayerStat] Added {amount} Energy Cubes. Total: {currentEnergyCubes}");
        }
    }

    public bool SpendEnergyCubes(int amount)
    {
        if (amount <= 0) return true;
        if (currentEnergyCubes < amount) return false;
        currentEnergyCubes -= amount;
        Debug.Log($"[PlayerStat] Spent {amount} Energy Cubes. Remaining: {currentEnergyCubes}");
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
