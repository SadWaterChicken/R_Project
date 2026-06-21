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
    public event Action OnMaterialInventoryChanged;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            LoadRecipesFromResources();
        }
        else
            Destroy(gameObject);
    }

    /// <summary>
    /// Load recipes dynamically from all BaseItemData in Resources
    /// </summary>
    public void LoadRecipesFromResources()
    {
        BaseItemData[] allItems = Resources.LoadAll<BaseItemData>("");
        foreach (var item in allItems)
        {
            if (item.isForgeable && item.forgingRecipe != null)
            {
                if (string.IsNullOrEmpty(item.forgingRecipe.recipeID))
                {
                    item.forgingRecipe.recipeID = "Recipe_" + item.itemID;
                }
                
                // Force the result item to be this item itself
                item.forgingRecipe.resultItem = item;
                
                if (!recipes.Any(r => r.recipeID == item.forgingRecipe.recipeID))
                {
                    recipes.Add(item.forgingRecipe);
                }
            }
        }
        Debug.Log($"[ForgingSystem] Auto-loaded {recipes.Count} recipes directly from BaseItemData.");
        // Ghi chú: Vì hệ thống chuyển sang kéo thả BaseItemData và ForgingMaterial trực tiếp trên Inspector,
        // Ta không nạp từ JSON nữa, vì JSON không lưu được tham chiếu tới ScriptableObject.
        // Bạn sẽ thiết lập danh sách 'materials' thông qua Unity Inspector của ForgingSystem.
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
            if (req.material != null && GetMaterialQuantity(req.material.materialID) < req.quantity)
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
