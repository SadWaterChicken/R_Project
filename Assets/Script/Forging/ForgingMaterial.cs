using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Represents a material/stone used for forging weapons
/// </summary>
[CreateAssetMenu(fileName = "New Forging Material", menuName = "R-Project/Forging Material")]
public class ForgingMaterial : ScriptableObject
{
    public string materialID;
    public string materialName;
    [TextArea] public string description;
    public string iconPath; // Giữ lại để migration
    public Sprite icon;

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
    [HideInInspector] public string recipeID;
    [HideInInspector] public string resultItemID;        // Thường vẫn dùng string hoặc BaseItemData, để tiện lưu file ta nên giữ ID hoặc chuyển sang BaseItemData
    [HideInInspector] public BaseItemData resultItem;    // Tùy chọn kéo thả Prefab kết quả
    [HideInInspector] public List<string> requiredItemIDs = new List<string>(); // Dùng để phục hồi data cũ
    [HideInInspector] [UnityEngine.Serialization.FormerlySerializedAs("requiredWeapons")] public List<BaseItemData> oldRequiredWeapons = new List<BaseItemData>(); // Giữ tạm để Migration
    
    [System.Serializable]
    public class WeaponRequirement
    {
        public BaseItemData weapon;
        public int quantity;
    }
    public List<WeaponRequirement> requiredWeapons = new List<WeaponRequirement>();

    [System.Serializable]
    public class MaterialRequirement
    {
        [HideInInspector] public string materialID; // Dùng để phục hồi data cũ
        public ForgingMaterial material; // Không dùng chuỗi nữa
        public int quantity;
    }

    public List<MaterialRequirement> requiredMaterials = new List<MaterialRequirement>();
    public int goldCost = 0;           // Gold needed to forge
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
