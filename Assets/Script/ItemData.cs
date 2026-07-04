using System;
using System.Collections.Generic;
using UnityEngine;

public enum EquipmentType
{
    None,
    Weapon,
    ChestArmor,
    LegArmor,
    Jewelry,
    Shoes,
    Helmet
}

public enum EquipSlot
{
    None,
    MainHand,
    OffHand
}

[Serializable]
public class ItemData
{
    // --- Dynamic Data (Saved to JSON) ---
    public string itemID;
    public int stack = 1;
    public bool equipped = false;
    public EquipSlot equipSlot = EquipSlot.None;
    public int itemTier = 1; // Từ 1 đến 5 sao (Gacha có thể đổi)
    public int forgeLevel = 0;                           // Forging enhancement level (0-10)
    public float weaponMastery = 0f;                     // Weapon mastery progress (0-100)
    public string baseItemID = "";                       // Original item ID before forging
    public int rarity = 1;                               // 1: Common, 2: Uncommon, 3: Rare, 4: Epic, 5: Legendary
    
    [NonSerialized] private bool? _isForgeableOverride;
    public bool isForgeable 
    {
        get => _isForgeableOverride.HasValue ? _isForgeableOverride.Value : (BaseData != null ? BaseData.isForgeable : false);
        set => _isForgeableOverride = value;
    }
    
    // --- Weapon Skill Slot (Mới: Mỗi vũ khí 1 slot skill) ---
    public string equippedSkillID = ""; 

    [NonSerialized] private ActiveSkillData _equippedSkill;
    public ActiveSkillData EquippedSkill
    {
        get
        {
            if (_equippedSkill == null || _equippedSkill.skillID != equippedSkillID)
            {
                if (PlayerSkillManager.Instance != null && !string.IsNullOrEmpty(equippedSkillID))
                {
                    _equippedSkill = PlayerSkillManager.Instance.GetSkillByID(equippedSkillID);
                }
                else
                {
                    _equippedSkill = null;
                }
            }
            return _equippedSkill;
        }
        set
        {
            _equippedSkill = value;
            equippedSkillID = value != null ? value.skillID : "";
        }
    }
    [Serializable]
    public class StatMod
    {
        [Tooltip("Select the stat from the predefined list. The name will be saved as a string.")]
        public StatType statTypeSelection = StatType.PhysicalDamage;
        public string stat;
        public float value;         // Giá trị Flat thực tế sẽ được cộng vào PlayerStat
        public bool percent;        // Đánh dấu đây là dòng %
        public float percentValue;  // Lưu % gốc (VD: 0.05) để UI in ra chữ "5%"
        public bool isMainStat = true; // Phân biệt Dòng chính (True) và Dòng phụ (False)
        public int statTier = 0;    // 0: Main Stat, 1-6: Sub-stat Tier (T1-T6)
    }
    public List<StatMod> modifiers = new List<StatMod>();

    // --- Runtime Data (Not saved to JSON) ---
    [NonSerialized]
    private BaseItemData _baseData;

    public BaseItemData BaseData 
    {
        get 
        {
            if (_baseData == null && !string.IsNullOrEmpty(itemID))
            {
                _baseData = Resources.Load<BaseItemData>($"ItemDatabase/{itemID}");
            }
            return _baseData;
        }
        set { _baseData = value; }
    }

    // --- Accessors for Static Data (Pulled from ScriptableObject) ---
    [NonSerialized] private string _itemNameOverride;
    public string itemName
    {
        get => !string.IsNullOrEmpty(_itemNameOverride) ? _itemNameOverride : (BaseData != null ? BaseData.itemName : "Unknown");
        set => _itemNameOverride = value;
    }

    [NonSerialized] private string _descriptionOverride;
    public string description
    {
        get => !string.IsNullOrEmpty(_descriptionOverride) ? _descriptionOverride : (BaseData != null ? BaseData.description : "");
        set => _descriptionOverride = value;
    }

    public int price => BaseData != null ? BaseData.basePrice : 0;
    public string iconPath => BaseData != null ? BaseData.iconPath : "";
    public bool equippable => BaseData != null ? BaseData.equippable : false;
    public EquipmentType equipmentType => BaseData != null ? BaseData.equipmentType : EquipmentType.None;
    public string weaponClassName => BaseData != null ? BaseData.weaponClassName : "";

    public bool hasSkill => BaseData != null ? BaseData.hasSkill : false;
    public WeaponSkill weaponSkill => BaseData != null ? BaseData.weaponSkill : default(WeaponSkill);

    public ItemData() { }

    public ItemData(string id, int s = 1)
    {
        itemID = id;
        stack = s;
        // Optionally auto-load base data here to get base modifiers if needed
        if (BaseData != null && modifiers.Count == 0)
        {
            foreach (var m in BaseData.baseModifiers)
            {
                modifiers.Add(new StatMod { stat = m.stat, value = m.value, percent = m.percent, percentValue = m.percentValue, isMainStat = m.isMainStat, statTier = m.statTier });
            }
        }
    }

    // Deep copy so stats are preserved when adding/buying
    public ItemData Clone(int? stackOverride = null)
    {
        var copy = new ItemData(itemID, stackOverride ?? stack)
        {
            equipped = equipped,
            equipSlot = equipSlot,
            itemTier = itemTier,
            modifiers = new List<StatMod>(),
            forgeLevel = forgeLevel,
            weaponMastery = weaponMastery,
            baseItemID = baseItemID,
            _isForgeableOverride = _isForgeableOverride,
            _baseData = _baseData,
            rarity = rarity
        };
        if (modifiers != null)
        {
            foreach (var m in modifiers)
                copy.modifiers.Add(new StatMod { stat = m.stat, value = m.value, percent = m.percent, percentValue = m.percentValue, isMainStat = m.isMainStat, statTier = m.statTier });
        }
        return copy;
    }
}
