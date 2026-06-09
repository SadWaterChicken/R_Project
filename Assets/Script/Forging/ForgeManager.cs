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
    /// Load advanced weapons dynamically from StreamingAssets for a specific weapon class.
    /// </summary>
    public List<ItemData> LoadAdvancedWeaponsForClass(string className)
    {
        if (string.IsNullOrEmpty(className)) return new List<ItemData>();

        string path = System.IO.Path.Combine(Application.streamingAssetsPath, "AdvancedWeapons", $"{className}_Upgrades.json");
        if (System.IO.File.Exists(path))
        {
            try
            {
                string json = System.IO.File.ReadAllText(path);
                var wrapper = JsonUtility.FromJson<AdvancedWeaponWrapper>(json);
                if (wrapper != null)
                {
                    Debug.Log($"[ForgeManager] Loaded {wrapper.advancedWeapons.Count} advanced weapons for class {className}.");
                    return wrapper.advancedWeapons;
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[ForgeManager] Failed to load {className}_Upgrades.json: {e.Message}");
            }
        }
        else
        {
            Debug.LogWarning($"[ForgeManager] No advanced weapon data found at path: {path}");
        }

        return new List<ItemData>();
    }

    /// <summary>
    /// Attempt to forge weapons into a new, enhanced weapon
    /// </summary>
    public ItemData AttemptForge(List<ItemData> weaponsToForge, ForgingRecipe recipe, List<ForgingMaterial> materials)
    {
        if (!ValidateForgeAttempt(weaponsToForge, recipe, materials))
            return null;

        // Remove materials from inventory
        var forgingSystem = ForgingSystem.Instance;
        foreach (var req in recipe.requiredMaterials)
        {
            forgingSystem.RemoveMaterial(req.materialID, req.quantity);
        }

        // Spend gold
        if (PlayerStat.Instance.SpendGold(recipe.goldCost))
        {
            Debug.Log($"[ForgeManager] Spent {recipe.goldCost} gold on forging");
        }

        // Create new weapon from recipe
        ItemData newWeapon = CreateForgedWeapon(recipe, weaponsToForge);

        // Remove old weapons from inventory
        var inventory = Inventory.Instance;
        foreach (var weapon in weaponsToForge)
        {
            inventory.RemoveItem(weapon, 1);
        }

        // Add new weapon to inventory
        inventory.AddItem(newWeapon);

        // Trigger event
        OnWeaponForged?.Invoke(newWeapon);

        Debug.Log($"[ForgeManager] Successfully forged: {newWeapon.itemName}");
        return newWeapon;
    }

    /// <summary>
    /// Create a new forged weapon with enhanced stats
    /// </summary>
    private ItemData CreateForgedWeapon(ForgingRecipe recipe, List<ItemData> baseWeapons)
    {
        var resultWeapon = GetWeaponTemplate(recipe.resultItemID);
        if (resultWeapon == null)
        {
            Debug.LogError($"[ForgeManager] Result weapon template not found: {recipe.resultItemID}");
            return null;
        }

        // Calculate average mastery from base weapons
        float avgMastery = baseWeapons.Count > 0 
            ? baseWeapons.Average(w => w.weaponMastery) 
            : 0f;

        // Set new weapon properties
        resultWeapon.forgeLevel = 1;
        resultWeapon.weaponMastery = Mathf.Min(avgMastery + masteryGainPerForge, maxMastery);
        resultWeapon.baseItemID = baseWeapons[0].itemID;
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
    public void AddMasteryOnKill(ItemData weapon)
    {
        if (weapon == null || !weapon.equipped) return;

        weapon.weaponMastery = Mathf.Min(weapon.weaponMastery + masteryGainPerKill, maxMastery);

        Debug.Log($"[ForgeManager] {weapon.itemName} mastery: {weapon.weaponMastery:F1}%");
    }

    /// <summary>
    /// Validate forging conditions
    /// </summary>
    private bool ValidateForgeAttempt(List<ItemData> weaponsToForge, ForgingRecipe recipe, List<ForgingMaterial> materials)
    {
        if (weaponsToForge == null || weaponsToForge.Count < recipe.minWeaponCount)
        {
            Debug.LogWarning($"[ForgeManager] Not enough weapons. Need {recipe.minWeaponCount}, have {weaponsToForge?.Count}");
            return false;
        }

        // Check same weapon class
        var weaponClassName = weaponsToForge[0].weaponClassName;
        if (weaponsToForge.Any(w => w.weaponClassName != weaponClassName))
        {
            Debug.LogWarning("[ForgeManager] All weapons must be same class!");
            return false;
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

        return true;
    }

    /// <summary>
    /// Returns a preview of what the forged weapon will look like (stats, level) without consuming anything.
    /// </summary>
    public ItemData GetPreviewWeapon(ItemData baseWeapon, ForgingRecipe recipe)
    {
        if (baseWeapon == null || recipe == null) return null;
        var resultWeapon = GetWeaponTemplate(recipe.resultItemID);
        if (resultWeapon == null) return null;

        resultWeapon.forgeLevel = 1;
        resultWeapon.weaponMastery = Mathf.Min(baseWeapon.weaponMastery + masteryGainPerForge, maxMastery);
        resultWeapon.baseItemID = baseWeapon.itemID;
        resultWeapon.isForgeable = true;

        ApplyForgeStatBonus(resultWeapon, resultWeapon.forgeLevel);
        return resultWeapon;
    }

    /// <summary>
    /// Get a deep-copied weapon template from the database by itemID.
    /// Assign all craftable weapons to the Weapon Database list in the Inspector.
    /// </summary>
    public ItemData GetWeaponTemplate(string itemID)
    {
        var template = weaponDatabase.FirstOrDefault(w => w != null && w.itemID == itemID);
        if (template == null)
        {
            Debug.LogWarning($"[ForgeManager] Weapon template '{itemID}' not found. Please add an ItemData with that itemID to the Weapon Database in the Inspector.");
            return null;
        }
        return template.Clone(); // Deep copy so we don't modify the original template
    }

    public float GetMaxMastery() => maxMastery;
    public int GetMaxForgeLevel() => maxForgeLevel;
}

[Serializable]
public class AdvancedWeaponWrapper
{
    public List<ItemData> advancedWeapons = new List<ItemData>();
}
