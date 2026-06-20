using UnityEngine;
using System.Collections.Generic;

public class GachaItemGenerator : MonoBehaviour
{
    // Danh sách các chỉ số có thể ra ở Dòng Phụ
    private static readonly string[] PossibleSubStats = new string[]
    {
        "maxHealth", "maxMana", "physicalDamage", "magicDamage",
        "physicalArmour", "magicArmour", "movementSpeed", "attackSpeed", "critChance"
    };

    /// <summary>
    /// Tạo ra một trang bị Gacha mới với chỉ số ngẫu nhiên dựa trên template và tier
    /// </summary>
    public static ItemData GenerateGachaItem(ItemData baseTemplate, int tier)
    {
        ItemData newItem = baseTemplate.Clone();
        newItem.itemTier = Mathf.Clamp(tier, 1, 5); // Đảm bảo tier từ 1 đến 5
        newItem.modifiers.Clear();

        // 1. TẠO DÒNG CHÍNH (MAIN STATS)
        int numMainStats = (newItem.itemTier >= 3) ? 2 : 1; // Tier 3,4,5 có 2 dòng chính. Tier 1,2 có 1 dòng chính.
        
        for (int i = 0; i < numMainStats; i++)
        {
            string mainStatName = GetRandomMainStatForEquipmentType(newItem.equipmentType);
            float mainStatValue = GetMainStatValue(mainStatName, newItem.itemTier);
            bool isPercent = mainStatName.Contains("chance") || mainStatName.Contains("speed");

            newItem.modifiers.Add(new ItemData.StatMod
            {
                stat = mainStatName,
                value = mainStatValue,
                percent = isPercent,
                isMainStat = true
            });
        }

        // 2. TẠO 4 DÒNG PHỤ NGẪU NHIÊN (SUB STATS)
        List<string> usedSubStats = new List<string>();

        // Tránh trùng lặp với Dòng Chính
        foreach (var mod in newItem.modifiers)
        {
            usedSubStats.Add(mod.stat);
        }

        for (int i = 0; i < 4; i++)
        {
            string subStatName = GetUniqueSubStat(usedSubStats);
            usedSubStats.Add(subStatName);

            float percentValue = GetSubStatPercentValue(newItem.itemTier); // Ví dụ: 0.05
            float flatValue = CalculateFlatValueFromPercent(subStatName, percentValue);

            newItem.modifiers.Add(new ItemData.StatMod
            {
                stat = subStatName,
                value = flatValue,       // Lưu FLAT value để PlayerStat tự cộng như bình thường
                percent = true,          // Vẫn đánh dấu là dòng % để UI biết đường hiển thị
                percentValue = percentValue, // Lưu % gốc để InventoryUI in ra "5%"
                isMainStat = false
            });
        }

        return newItem;
    }

    private static float CalculateFlatValueFromPercent(string statName, float percent)
    {
        if (PlayerStat.Instance == null) return 0f;

        switch (statName)
        {
            case "maxHealth": return PlayerStat.Instance.maxHealth * percent;
            case "maxMana": return PlayerStat.Instance.maxMana * percent;
            case "physicalDamage": return PlayerStat.Instance.basePhysicalDamage * percent;
            case "magicDamage": return PlayerStat.Instance.baseMagicDamage * percent;
            case "physicalArmour": return PlayerStat.Instance.physicalArmor * percent;
            case "magicArmour": return PlayerStat.Instance.magicArmor * percent;
            case "movementSpeed": return PlayerStat.Instance.baseSpeed * percent;
            case "attackSpeed": return PlayerStat.Instance.attackSpeed * percent;
            case "critChance": return PlayerStat.Instance.critChance * percent;
            default: return 0f;
        }
    }

    private static string GetRandomMainStatForEquipmentType(EquipmentType type)
    {
        // Ví dụ: Vũ khí ưu tiên sát thương, Giáp ưu tiên phòng thủ/máu
        switch (type)
        {
            case EquipmentType.Weapon:
                return Random.value > 0.5f ? "physicalDamage" : "magicDamage";
            case EquipmentType.Helmet:
            case EquipmentType.ChestArmor:
            case EquipmentType.LegArmor:
                return Random.value > 0.5f ? "maxHealth" : "physicalArmour";
            case EquipmentType.Shoes:
                return "movementSpeed";
            default:
                return PossibleSubStats[Random.Range(0, PossibleSubStats.Length)];
        }
    }

    private static float GetMainStatValue(string statName, int tier)
    {
        // Dòng chính tăng mạnh theo Tier
        float baseValue = 0f;
        
        if (statName == "maxHealth" || statName == "maxMana") baseValue = 50f;
        else if (statName == "physicalDamage" || statName == "magicDamage") baseValue = 10f;
        else if (statName == "physicalArmour" || statName == "magicArmour") baseValue = 5f;
        else if (statName == "movementSpeed" || statName == "attackSpeed") baseValue = 0.1f; // 10%
        else if (statName == "critChance") baseValue = 0.05f; // 5%

        return baseValue * tier;
    }

    private static string GetUniqueSubStat(List<string> excludeList)
    {
        List<string> available = new List<string>(PossibleSubStats);
        available.RemoveAll(x => excludeList.Contains(x));

        if (available.Count == 0) return PossibleSubStats[0]; // Fallback
        return available[Random.Range(0, available.Count)];
    }

    private static float GetSubStatPercentValue(int tier)
    {
        // Trả về % dựa trên tier (Ví dụ: Tier 1 được 1-3%, Tier 5 được 5-10%)
        // Lưu trữ dưới dạng số thập phân: 0.05 = 5%
        float minPercent = tier * 0.01f;
        float maxPercent = tier * 0.02f;
        
        return Random.Range(minPercent, maxPercent);
    }
}
