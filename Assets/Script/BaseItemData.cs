using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct WeaponSkill
{
    public string skillName;
    [TextArea(2, 3)] public string description;
    public Sprite skillIcon;
    public float cooldown;
    public float damageMultiplier; // Ví dụ: 1.5 (x1.5 sát thương)
    public string animationTrigger; // Tên Trigger trên Animator (vd: "Skill")
    public GameObject skillPrefab; // Prefab của skill (VFX, đạn bay, chưởng lực...)
}

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

    [Header("Weapon Skill")]
    public bool hasSkill = false;
    public WeaponSkill weaponSkill;


    [Header("Forging Requirements")]
    [Tooltip("Check this if this item can be forged by the player")]
    public bool isForgeable = false;
    [Tooltip("Define what is needed to forge THIS item")]
    public ForgingRecipe forgingRecipe;

    private void OnValidate()
    {
        if (baseModifiers != null)
        {
            foreach (var mod in baseModifiers)
            {
                if (mod.statTypeSelection != StatType.Custom)
                {
                    mod.stat = mod.statTypeSelection.ToString();
                }
            }
        }
    }
}
