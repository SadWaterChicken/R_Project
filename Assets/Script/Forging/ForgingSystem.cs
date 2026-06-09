using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Manages all forging recipes and operations
/// </summary>
public class ForgingSystem : MonoBehaviour
{
    public static ForgingSystem Instance { get; private set; }

    [SerializeField] public List<ForgingRecipe> recipes = new List<ForgingRecipe>();
    [SerializeField] public List<ForgingMaterial> materials = new List<ForgingMaterial>();

    // Material inventory
    private List<ForgingMaterialStack> materialInventory = new List<ForgingMaterialStack>();

    // Events
    public event Action<ForgingMaterialStack> OnMaterialAdded;
    public event Action<ForgingMaterialStack> OnMaterialRemoved;
    public event Action OnMaterialInventoryChanged;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            LoadRecipesFromStreamingAssets();
        }
        else
            Destroy(gameObject);
    }

    /// <summary>
    /// Load recipes and materials dynamically from StreamingAssets
    /// </summary>
    public void LoadRecipesFromStreamingAssets()
    {
        string path = System.IO.Path.Combine(Application.streamingAssetsPath, "ForgingRecipes.json");
        if (System.IO.File.Exists(path))
        {
            try
            {
                string json = System.IO.File.ReadAllText(path);
                var wrapper = JsonUtility.FromJson<ForgingRecipeWrapper>(json);
                if (wrapper != null)
                {
                    recipes = wrapper.recipes;
                    materials = wrapper.materials;
                    Debug.Log($"[ForgingSystem] Automatically loaded {recipes.Count} recipes and {materials.Count} materials from StreamingAssets.");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[ForgingSystem] Failed to load ForgingRecipes.json: {e.Message}");
            }
        }
        else
        {
            Debug.LogWarning($"[ForgingSystem] No ForgingRecipes.json found at {path}.");
        }
    }

    /// <summary>
    /// Get recipe by ID
    /// </summary>
    public ForgingRecipe GetRecipe(string recipeID)
    {
        return recipes.FirstOrDefault(r => r.recipeID == recipeID);
    }

    /// <summary>
    /// Get material by ID
    /// </summary>
    public ForgingMaterial GetMaterial(string materialID)
    {
        return materials.FirstOrDefault(m => m.materialID == materialID);
    }

    /// <summary>
    /// Add material to inventory
    /// </summary>
    public void AddMaterial(string materialID, int quantity)
    {
        var material = GetMaterial(materialID);
        if (material == null) return;

        var existing = materialInventory.FirstOrDefault(m => m.material.materialID == materialID);
        if (existing != null)
        {
            existing.quantity += quantity;
        }
        else
        {
            var stack = new ForgingMaterialStack(material, quantity);
            materialInventory.Add(stack);
            OnMaterialAdded?.Invoke(stack);
        }

        OnMaterialInventoryChanged?.Invoke();
        Debug.Log($"[ForgingSystem] Added {quantity}x {material.materialName}");
    }

    /// <summary>
    /// Remove material from Inventory by itemID (bridges shop items with forge requirements)
    /// </summary>
    public bool RemoveMaterial(string materialID, int quantity)
    {
        if (Inventory.Instance == null) return false;

        // Verify we have enough before removing
        if (GetMaterialQuantity(materialID) < quantity)
        {
            Debug.LogWarning($"[ForgingSystem] Not enough '{materialID}' to remove {quantity}.");
            return false;
        }

        int remaining = quantity;
        // Collect matching items (ToList to avoid modifying collection while iterating)
        var items = Inventory.Instance.ownedItems
            .Where(x => x.itemID == materialID)
            .ToList();

        foreach (var item in items)
        {
            if (remaining <= 0) break;
            int toRemove = Mathf.Min(item.stack, remaining);
            Inventory.Instance.RemoveItem(item, toRemove);
            remaining -= toRemove;
        }

        OnMaterialInventoryChanged?.Invoke();
        return true;
    }

    /// <summary>
    /// Check if Inventory contains enough items matching each materialID
    /// </summary>
    public bool HasMaterials(List<ForgingRecipe.MaterialRequirement> required)
    {
        if (Inventory.Instance == null) return false;
        foreach (var req in required)
        {
            if (GetMaterialQuantity(req.materialID) < req.quantity)
                return false;
        }
        return true;
    }

    /// <summary>
    /// Get current material inventory
    /// </summary>
    public List<ForgingMaterialStack> GetMaterialInventory()
    {
        return new List<ForgingMaterialStack>(materialInventory);
    }

    /// <summary>
    /// Count how many of a material the player has (sums stack counts from Inventory)
    /// </summary>
    public int GetMaterialQuantity(string materialID)
    {
        if (Inventory.Instance == null) return 0;
        return Inventory.Instance.ownedItems
            .Where(x => x.itemID == materialID)
            .Sum(x => x.stack);
    }

    /// <summary>
    /// Clear material inventory
    /// </summary>
    public void ClearMaterials()
    {
        materialInventory.Clear();
        OnMaterialInventoryChanged?.Invoke();
    }
}

[System.Serializable]
public class ForgingRecipeWrapper
{
    public List<ForgingRecipe> recipes = new List<ForgingRecipe>();
    public List<ForgingMaterial> materials = new List<ForgingMaterial>();
}
