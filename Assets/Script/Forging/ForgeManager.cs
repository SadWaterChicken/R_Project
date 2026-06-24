using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Handles the actual forging logic and weapon enhancement
/// Supports all 9 weapon types with their specific mechanics
/// </summary>
public class ForgeManager : MonoBehaviour
{
    public static ForgeManager Instance { get; private set; }

    [Header("Forging Config")]
    [SerializeField] private float statBonusPerLevel = 5f;      // +5 per forge level (base)
    [SerializeField] private float masteryGainPerForge = 10f;   // +10 mastery when weapon is forged
    [SerializeField] private float masteryGainPerKill = 0.5f;   // +0.5 mastery per enemy kill with weapon
    [SerializeField] private int maxForgeLevel = 10;
    [SerializeField] private float maxMastery = 100f;

    [Header("Class Database")]
    [Tooltip("Kéo thả các file WeaponClassAsset bạn tạo trên Unity vào đây")]
    public List<WeaponClassAsset> classDatabase = new List<WeaponClassAsset>();

    [Header("Weapon Database")]
    [Tooltip("Assign ItemData templates for every possible forge result here. itemID must match resultItemID in the recipe.")]
    [SerializeField] private List<ItemData> weaponDatabase = new List<ItemData>();

    public event Action<ItemData> OnWeaponForged;  // Triggered when weapon is successfully forged

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
            Destroy(gameObject);
    }

    /// <summary>
    /// Scan all BaseItemData in Resources to find forgeable weapons for a specific class.
    /// </summary>
    public List<ItemData> LoadAdvancedWeaponsForClass(string className)
    {
        if (string.IsNullOrEmpty(className)) return new List<ItemData>();

        List<ItemData> advancedWeapons = new List<ItemData>();
        BaseItemData[] allItems = Resources.LoadAll<BaseItemData>("");
        
        foreach (var baseItem in allItems)
        {
            // If it's the same class, is forgeable, and has a recipe defined
            if (baseItem.weaponClassName == className && baseItem.isForgeable && baseItem.forgingRecipe != null && !string.IsNullOrEmpty(baseItem.forgingRecipe.recipeID))
            {
                advancedWeapons.Add(new ItemData(baseItem.itemID));
            }
        }

        Debug.Log($"[ForgeManager] Found {advancedWeapons.Count} advanced weapons for class {className} directly from Resources.");
        return advancedWeapons;
    }

    /// <summary>
    /// Attempt to forge weapons into a new, enhanced weapon
    /// </summary>
    public ItemData AttemptForge(ForgingRecipe recipe, List<ForgingMaterial> materials)
    {
        if (!ValidateForgeAttempt(recipe, materials))
            return null;

        // Remove materials from inventory
        var forgingSystem = ForgingSystem.Instance;
        foreach (var req in recipe.requiredMaterials)
        {
            if (req.material != null)
                forgingSystem.RemoveMaterial(req.material.materialID, req.quantity);
        }

        // Spend gold
        if (PlayerStat.Instance.SpendGold(recipe.goldCost))
        {
            Debug.Log($"[ForgeManager] Spent {recipe.goldCost} gold on forging");
        }

        // Create new weapon from recipe
        ItemData newWeapon = CreateForgedWeapon(recipe);

        // Add new weapon to inventory
        Inventory.Instance.AddItem(newWeapon);

        // Trigger event
        OnWeaponForged?.Invoke(newWeapon);

        Debug.Log($"[ForgeManager] Successfully forged: {newWeapon.itemName}");
        return newWeapon;
    }

    /// <summary>
    /// Create a new forged weapon with enhanced stats
    /// </summary>
    private ItemData CreateForgedWeapon(ForgingRecipe recipe)
    {
        var resultWeapon = recipe.resultItem != null ? new ItemData(recipe.resultItem.itemID) : GetWeaponTemplate(recipe.resultItemID);
        if (resultWeapon == null)
        {
            Debug.LogError($"[ForgeManager] Result weapon template not found for recipe: {recipe.recipeID}");
            return null;
        }

        // Set new weapon properties
        resultWeapon.forgeLevel = 1;
        // No base weapon ID anymore
        resultWeapon.isForgeable = true;

        // Calculate enhanced stats based on forge level
        ApplyForgeStatBonus(resultWeapon, resultWeapon.forgeLevel);

        return resultWeapon;
    }

    /// <summary>
    /// Apply stat bonuses based on forge level and weapon type
    /// Each weapon type scales differently
    /// </summary>
    private void ApplyForgeStatBonus(ItemData weapon, int forgeLevel)
    {
        if (weapon.modifiers == null) return;

        float multiplier = GetWeaponMultiplier(weapon.weaponClassName);

        foreach (var mod in weapon.modifiers)
        {
            float bonus = statBonusPerLevel * forgeLevel * multiplier;
            if (!mod.percent)
            {
                mod.value += bonus;
            }
        }

        Debug.Log($"[ForgeManager] Applied bonus to {weapon.itemName} with multiplier {multiplier}x");
    }

    /// <summary>
    /// Get scaling multiplier for each weapon type
    /// </summary>
    private float GetWeaponMultiplier(string weaponClassName)
    {
        if (string.IsNullOrEmpty(weaponClassName)) return 1f;

        var classAsset = classDatabase.FirstOrDefault(c => c != null && c.className == weaponClassName);
        if (classAsset != null)
        {
            return classAsset.forgeMultiplier;
        }

        Debug.LogWarning($"[ForgeManager] Class '{weaponClassName}' not found in classDatabase. Defaulting multiplier to 1x.");
        return 1f;
    }

    /// <summary>
    /// Increase weapon mastery when killing enemies with it
    /// </summary>
    public void AddMasteryOnKill(ItemData weapon, float overrideAmount = -1f)
    {
        if (weapon == null || !weapon.equipped) return;
        if (string.IsNullOrEmpty(weapon.weaponClassName)) return;

        float amountToAdd = overrideAmount >= 0f ? overrideAmount : masteryGainPerKill;
        
        if (Inventory.Instance != null)
        {
            Inventory.Instance.AddClassMastery(weapon.weaponClassName, amountToAdd);
            Debug.Log($"[ForgeManager] Class '{weapon.weaponClassName}' mastery increased by {amountToAdd:F1}. Current: {Inventory.Instance.GetClassMastery(weapon.weaponClassName):F1}%");
        }
    }

    /// <summary>
    /// Validate forging conditions
    /// </summary>
    private bool ValidateForgeAttempt(ForgingRecipe recipe, List<ForgingMaterial> materials)
    {
        if (recipe == null) return false;

        if (recipe.resultItem != null && Inventory.Instance != null)
        {
            float currentMastery = Inventory.Instance.GetClassMastery(recipe.resultItem.weaponClassName);
            if (currentMastery < recipe.requiredMastery)
            {
                Debug.LogWarning($"[ForgeManager] Not enough mastery. Need {recipe.requiredMastery}, have {currentMastery}");
                return false;
            }
        }

        // Check gold
        if (!PlayerStat.Instance.CanSpendGold(recipe.goldCost))
        {
            Debug.LogWarning($"[ForgeManager] Not enough gold. Need {recipe.goldCost}, have {PlayerStat.Instance.GetGold()}");
            return false;
        }

        // Check materials
        var forgingSystem = ForgingSystem.Instance;
        if (!forgingSystem.HasMaterials(recipe.requiredMaterials))
        {
            Debug.LogWarning("[ForgeManager] Not enough materials!");
            return false;
        }

        // --- NEW: Rarity vs Tier restriction ---
        // Ensure that the materials used are high enough quality for this weapon's tier
        int targetTier = recipe.resultItem != null ? recipe.resultItem.itemTier : 1;
        foreach (var req in recipe.requiredMaterials)
        {
            ForgingMaterial matInfo = req.material;
            if (matInfo != null)
            {
                // Quy tắc: Rarity của đá rèn phải lớn hơn hoặc bằng Tier đích - 1
                if (matInfo.rarity < targetTier - 1)
                {
                    Debug.LogWarning($"[ForgeManager] Material '{matInfo.materialName}' (Rarity {matInfo.rarity}) is too low quality to forge a Tier {targetTier} weapon!");
                    return false;
                }
            }
        }

        return true;
    }

    /// <summary>
    /// Returns a preview of what the forged weapon will look like (stats, level) without consuming anything.
    /// </summary>
    public ItemData GetPreviewWeapon(ForgingRecipe recipe)
    {
        if (recipe == null) return null;
        return CreateForgedWeapon(recipe);
    }

    /// <summary>
    /// Get a deep-copied weapon template from the database or construct from BaseItemData
    /// </summary>
    public ItemData GetWeaponTemplate(string itemID)
    {
        var template = weaponDatabase.FirstOrDefault(w => w != null && w.itemID == itemID);
        if (template != null) return template.Clone();

        // Search in Resources (BaseItemData)
        var baseData = Resources.Load<BaseItemData>($"ItemDatabase/{itemID}");
        if (baseData != null)
        {
            return new ItemData(itemID);
        }

        Debug.LogWarning($"[ForgeManager] Weapon template '{itemID}' not found in database or Resources/ItemDatabase.");
        return null;
    }

    public float GetMaxMastery() => maxMastery;
    public int GetMaxForgeLevel() => maxForgeLevel;
}

[Serializable]
public class AdvancedWeaponWrapper
{
    public List<ItemData> advancedWeapons = new List<ItemData>();
}
