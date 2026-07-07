using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

[Serializable]
public class PlayerSaveData
{
    public int maxHealth;
    public float currentHealth;
    public float healthRegenRate;

    public int maxMana;
    public float currentMana;
    public float manaRegenRate;

    public float basePhysicalDamage;
    public float physicalDamageBonus;
    public float baseMagicDamage;
    public float magicDamageBonus;

    public float physicalArmor;
    public float magicArmor;

    public float baseSpeed;
    public float movementSpeed;
    public float attackSpeed;

    public float critChance;

    public float shield;
    public float maxShield;
    public float shieldRegenRate;
    public float shieldRechargeCooldown;

    public int maxSanity;
    public float currentSanity;
    public float sanityRegenRate;

    public float luck;
    public float dashCooldown;
    public bool isInvincible;

    public int gold;
    public int currentEnergyCubes;
}

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
    // private bool isDead = false; (Moved to base CharacterStats)

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
    public float dualBladesMastery;

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
            case "dualblades": dualBladesMastery = masteryExp; break;
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
        
        // Load stats here so base.Start() doesn't overwrite values like currentHealth and shield
        LoadPlayerStats();

        // --- Sync masteries from Inventory ONLY IF they exist in save data ---
        if (Inventory.Instance != null)
        {
            if (Inventory.Instance.classMasteries.ContainsKey("Greatsword")) greatswordMastery = Inventory.Instance.classMasteries["Greatsword"];
            if (Inventory.Instance.classMasteries.ContainsKey("Katana")) katanaMastery = Inventory.Instance.classMasteries["Katana"];
            if (Inventory.Instance.classMasteries.ContainsKey("Warhammer")) warhammerMastery = Inventory.Instance.classMasteries["Warhammer"];
            if (Inventory.Instance.classMasteries.ContainsKey("Greatsaxe")) greatsaxeMastery = Inventory.Instance.classMasteries["Greatsaxe"];
            if (Inventory.Instance.classMasteries.ContainsKey("Spear")) spearMastery = Inventory.Instance.classMasteries["Spear"];
            if (Inventory.Instance.classMasteries.ContainsKey("Bow")) bowMastery = Inventory.Instance.classMasteries["Bow"];
            if (Inventory.Instance.classMasteries.ContainsKey("Staff")) staffMastery = Inventory.Instance.classMasteries["Staff"];
            if (Inventory.Instance.classMasteries.ContainsKey("Orb")) orbMastery = Inventory.Instance.classMasteries["Orb"];
            if (Inventory.Instance.classMasteries.ContainsKey("DualBlades")) dualBladesMastery = Inventory.Instance.classMasteries["DualBlades"];
        }
        
        // Optional: currentSanity can also be synced from load, but if not loaded properly, use max
        // actually LoadPlayerStats() handles currentSanity
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
        SavePlayerStats();
        if (Inventory.Instance != null)
        {
            // Sync any inspector tweaks to Inventory before PlayerStat is destroyed
            Inventory.Instance.SaveInventory();
            Inventory.Instance.OnItemEquipChanged -= OnItemEquipChanged;
        }
    }

    private void OnApplicationQuit()
    {
        SavePlayerStats();
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
            float flatVal = mod.value * multiplier;
            float percentVal = (mod.percentValue / 100f) * multiplier; // Divide by 100 because UI/Input uses 100 for 100%
            
            float logicVal = mod.percent ? (mod.percentValue != 0 ? percentVal : flatVal / 100f) : flatVal;

            Debug.Log($"[PlayerStat] {(isEquipped ? "Equipping" : "Unequipping")} {item.itemName}: {mod.stat} {(logicVal >= 0 ? "+" : "")}{Mathf.Abs(logicVal)} (IsPercent: {mod.percent})");

            if (item.BaseData != null && item.BaseData.equipmentType == EquipmentType.Weapon)
            {
                string statLower = mod.stat.ToLower();
                if (statLower == "physical damage" || statLower == "physicaldamage" || 
                    statLower == "magic damage" || statLower == "magicdamage" ||
                    statLower == "physical damage bonus" || statLower == "physicaldamagebonus" ||
                    statLower == "magic damage bonus" || statLower == "magicdamagebonus" ||
                    statLower == "crit chance" || statLower == "critchance" ||
                    statLower == "attack speed" || statLower == "attackspeed")
                {
                    // Vũ khí thì giữ lại các chỉ số này cho riêng nó (Local Stats), KHÔNG cộng vào Player (Global Stats)
                    continue; 
                }
            }

            switch (mod.stat.ToLower())
            {
                case "physical damage":
                case "physicaldamage":
                    basePhysicalDamage += logicVal;
                    break;
                case "physical damage bonus":
                case "physicaldamagebonus":
                    if (mod.percent) physicalDamageMultiplier += logicVal;
                    else physicalDamageBonus += logicVal;
                    break;
                case "magic damage":
                case "magicdamage":
                    baseMagicDamage += logicVal;
                    break;
                case "magic damage bonus":
                case "magicdamagebonus":
                    if (mod.percent) magicDamageMultiplier += logicVal;
                    else magicDamageBonus += logicVal;
                    break;
                case "physical armour":
                case "physicalarmour":
                case "physical armor":
                case "physicalarmor":
                    physicalArmor += logicVal;
                    break;
                case "magic armour":
                case "magicarmour":
                case "magic armor":
                case "magicarmor":
                    magicArmor += logicVal;
                    break;
                case "max health":
                case "maxhealth":
                    maxHealth += (int)logicVal;
                    break;
                case "max mana":
                case "maxmana":
                    maxMana += (int)logicVal;
                    break;
                case "max shield":
                case "maxshield":
                    maxShield += logicVal;
                    break;
                case "movement speed":
                case "movementspeed":
                    movementSpeed += logicVal;
                    break;
                case "attack speed":
                case "attackspeed":
                    attackSpeed += logicVal;
                    break;
                case "crit chance":
                case "critchance":
                    critChance += logicVal;
                    break;
                case "luck":
                    luck += logicVal;
                    break;
                default:
                    Debug.LogWarning($"[PlayerStat] Unknown stat type: {mod.stat}");
                    break;
            }
        }

        Debug.Log($"[PlayerStat] Stats updated - PhysicalDmg: {GetPhysicalDamage()}, MagicDmg: {GetMagicDamage()}, Armour: {physicalArmor}/{magicArmor}");
    }

    // ─── Stat Getters (Overrides) ──────────────────────────────────────────────
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
    public override void TakeMixedDamage(float physicalDamage, float magicDamage, float armorPenetration = 0f, float magicPenetration = 0f, bool isCrit = false)
    {
        if (isInvincible) return;
        
        PlayerCombat combat = GetComponent<PlayerCombat>();
        if (combat != null && combat.IsGuarding())
        {
            physicalDamage *= combat.guardDamageReduction;
            magicDamage *= combat.guardDamageReduction;
            Debug.Log($"[PlayerStat] Guarding! Reduced incoming damage.");
        }

        base.TakeMixedDamage(physicalDamage, magicDamage, armorPenetration, magicPenetration, isCrit);
    }

    protected override void Die()
    {
        if (isDead) return;
        base.Die();

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
            SavePlayerStats();
        }
    }

    public bool SpendGold(int amount)
    {
        if (amount <= 0) return true;
        if (gold < amount) return false;
        gold -= amount;
        Debug.Log($"[PlayerStat] Spent {amount} gold. Remaining: {gold}");
        SavePlayerStats();
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
            SavePlayerStats();
        }
    }

    public bool SpendEnergyCubes(int amount)
    {
        if (amount <= 0) return true;
        if (currentEnergyCubes < amount) return false;
        currentEnergyCubes -= amount;
        Debug.Log($"[PlayerStat] Spent {amount} Energy Cubes. Remaining: {currentEnergyCubes}");
        SavePlayerStats();
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

    // ─── Save / Load ────────────────────────────────────────────────────────────
    private string GetSavePath()
    {
        return System.IO.Path.Combine(Application.persistentDataPath, "player_stats_save.json");
    }

    public void SavePlayerStats()
    {
        PlayerSaveData data = new PlayerSaveData
        {
            maxHealth = this.maxHealth,
            currentHealth = this.currentHealth,
            healthRegenRate = this.healthRegenRate,
            maxMana = this.maxMana,
            currentMana = this.currentMana,
            manaRegenRate = this.manaRegenRate,
            basePhysicalDamage = this.basePhysicalDamage,
            physicalDamageBonus = this.physicalDamageBonus,
            baseMagicDamage = this.baseMagicDamage,
            magicDamageBonus = this.magicDamageBonus,
            physicalArmor = this.physicalArmor,
            magicArmor = this.magicArmor,
            baseSpeed = this.baseSpeed,
            movementSpeed = this.movementSpeed,
            attackSpeed = this.attackSpeed,
            critChance = this.critChance,
            shield = this.shield,
            maxShield = this.maxShield,
            shieldRegenRate = this.shieldRegenRate,
            shieldRechargeCooldown = this.shieldRechargeCooldown,
            maxSanity = this.maxSanity,
            currentSanity = this.currentSanity,
            sanityRegenRate = this.sanityRegenRate,
            luck = this.luck,
            dashCooldown = this.dashCooldown,
            isInvincible = this.isInvincible,
            gold = this.gold,
            currentEnergyCubes = this.currentEnergyCubes
        };

        string json = JsonUtility.ToJson(data, true);
        System.IO.File.WriteAllText(GetSavePath(), json);
        Debug.Log("[PlayerStat] Saved stats to: " + GetSavePath());
    }

    public void LoadPlayerStats()
    {
        string path = GetSavePath();
        if (System.IO.File.Exists(path))
        {
            string json = System.IO.File.ReadAllText(path);
            PlayerSaveData data = JsonUtility.FromJson<PlayerSaveData>(json);
            if (data != null)
            {
                this.maxHealth = data.maxHealth;
                this.currentHealth = data.currentHealth;
                this.healthRegenRate = data.healthRegenRate;
                this.maxMana = data.maxMana;
                this.currentMana = data.currentMana;
                this.manaRegenRate = data.manaRegenRate;
                this.basePhysicalDamage = data.basePhysicalDamage;
                this.physicalDamageBonus = data.physicalDamageBonus;
                this.baseMagicDamage = data.baseMagicDamage;
                this.magicDamageBonus = data.magicDamageBonus;
                this.physicalArmor = data.physicalArmor;
                this.magicArmor = data.magicArmor;
                this.baseSpeed = data.baseSpeed;
                this.movementSpeed = data.movementSpeed;
                this.attackSpeed = data.attackSpeed;
                this.critChance = data.critChance;
                this.shield = data.shield;
                this.maxShield = data.maxShield;
                this.shieldRegenRate = data.shieldRegenRate;
                this.shieldRechargeCooldown = data.shieldRechargeCooldown;
                this.maxSanity = data.maxSanity;
                this.currentSanity = data.currentSanity;
                this.sanityRegenRate = data.sanityRegenRate;
                this.luck = data.luck;
                this.dashCooldown = data.dashCooldown;
                this.isInvincible = data.isInvincible;
                this.gold = data.gold;
                this.currentEnergyCubes = data.currentEnergyCubes;

                Debug.Log("[PlayerStat] Loaded stats from: " + path);
            }
        }
        else
        {
            Debug.Log("[PlayerStat] No save file found, using defaults. Saving new file.");
            SavePlayerStats();
        }
    }
}
