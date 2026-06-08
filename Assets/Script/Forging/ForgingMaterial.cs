using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Represents a material/stone used for forging weapons
/// </summary>
[System.Serializable]
public class ForgingMaterial
{
    public string materialID;
    public string materialName;
    public string description;
    public string iconPath;

    [System.Serializable]
    public enum MaterialType
    {
        Stone = 0,
        Metal = 1,
        Crystal = 2,
        Ore = 3,
        Essence = 4
    }

    public MaterialType materialType;
    public int rarity = 1; // 1=Common, 2=Uncommon, 3=Rare, 4=Epic, 5=Legendary
    public float weight = 1f; // Used in forge recipe calculations
}

/// <summary>
/// Represents a forging recipe (what materials + items needed to create a new weapon)
/// </summary>
[System.Serializable]
public class ForgingRecipe
{
    public string recipeID;
    public string resultItemID;        // The weapon you get after forging
    public List<string> requiredItemIDs = new List<string>(); // Same weapon class items to combine

    [System.Serializable]
    public class MaterialRequirement
    {
        public string materialID;
        public int quantity;
    }

    public List<MaterialRequirement> requiredMaterials = new List<MaterialRequirement>();
    public int goldCost = 0;           // Gold needed to forge
    public int minWeaponCount = 2;     // Minimum weapons of same class needed
}

/// <summary>
/// Forging material inventory item (similar to ItemData but for materials)
/// </summary>
[System.Serializable]
public class ForgingMaterialStack
{
    public ForgingMaterial material;
    public int quantity;

    public ForgingMaterialStack(ForgingMaterial mat, int qty)
    {
        material = mat;
        quantity = qty;
    }

    public ForgingMaterialStack Clone()
    {
        return new ForgingMaterialStack(material, quantity);
    }
}
