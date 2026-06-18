using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Item Data/Base Item")]
public class BaseItemData : ScriptableObject
{
    [Header("Basic Info")]
    public string itemID;
    public string itemName;
    [TextArea(3, 5)]
    public string description;
    public int basePrice;
    public string iconPath;

    [Header("Item Quality")]
    [Range(1, 5)] public int itemTier = 1; // 1 to 5 stars
    [Range(1, 5)] public int rarity = 1;   // 1=Common, 2=Uncommon, 3=Rare, 4=Epic, 5=Legendary
    
    [Header("Equipment Settings")]
    public bool equippable = true;
    public EquipmentType equipmentType = EquipmentType.None;
    public string weaponClassName = "";  // "Greatsword", "Scythe", etc.
    
    [Header("Stats")]
    public List<ItemData.StatMod> baseModifiers = new List<ItemData.StatMod>();

    [Header("Visuals & Combat")]
    public GameObject weaponPrefab; // 3D/2D model with Collider and MeleeWeapon script
    public int customStanceID = 0;  // 0: default, 1: shoulder carry, etc.

    [Header("Forging Requirements")]
    [Tooltip("Check this if this item can be forged by the player")]
    public bool isForgeable = false;
    [Tooltip("Define what is needed to forge THIS item")]
    public ForgingRecipe forgingRecipe;
}
