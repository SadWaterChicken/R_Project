using System.Collections.Generic;
using UnityEngine;

public enum StatType
{
    Custom,
    PhysicalDamage,
    PhysicalDamageBonus,
    MagicDamage,
    MagicDamageBonus,
    PhysicalArmor,
    MagicArmor,
    MaxHealth,
    MaxMana,
    MaxShield,
    HealthRegenRate,
    ManaRegenRate,
    SanityRegenRate,
    MovementSpeed,
    AttackSpeed,
    CritChance,
    Luck
}


[System.Serializable]
public class StatTierDefinition
{
    public int tier; // e.g. 1 to 6
    public float minValue;
    public float maxValue;
}

[System.Serializable]
public class StatDefinition
{
    [Tooltip("Select the stat from the predefined list. The name will be saved as a string.")]
    public StatType statTypeSelection = StatType.PhysicalDamage;
    
    [Tooltip("The actual string value used in the game logic.")]
    public string statName;
    
    public bool isPercent;
    [Tooltip("List of tiers. Usually 6 tiers (T1 to T6)")]
    public List<StatTierDefinition> tiers = new List<StatTierDefinition>();
}

[CreateAssetMenu(fileName = "New Stat Roll Config", menuName = "Item Data/Stat Roll Config")]
public class StatRollConfig : ScriptableObject
{
    public List<StatDefinition> stats = new List<StatDefinition>();

    public StatDefinition GetStatDefinition(string statName, bool isPercent)
    {
        return stats.Find(s => s.statName == statName && s.isPercent == isPercent);
    }

    [ContextMenu("Auto-Populate Missing Stats")]
    public void AutoPopulateMissingStats()
    {
        var statTypes = (StatType[])System.Enum.GetValues(typeof(StatType));
        foreach (var statType in statTypes)
        {
            if (statType == StatType.Custom) continue;

            if (!stats.Exists(s => s.statTypeSelection == statType && !s.isPercent))
            {
                var newStat = new StatDefinition
                {
                    statTypeSelection = statType,
                    statName = statType.ToString(),
                    isPercent = false,
                    tiers = new List<StatTierDefinition>()
                };

                for (int i = 1; i <= 6; i++)
                {
                    newStat.tiers.Add(new StatTierDefinition { tier = i });
                }

                stats.Add(newStat);
            }
            
            // Optionally, we could auto-populate the percent version too, 
            // but let's stick to the base flat stats first.
        }

#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
#endif
    }

    private void OnValidate()
    {
        if (stats != null)
        {
            foreach (var statDef in stats)
            {
                if (statDef.statTypeSelection != StatType.Custom)
                {
                    statDef.statName = statDef.statTypeSelection.ToString();
                }

                if (statDef.tiers == null)
                {
                    statDef.tiers = new List<StatTierDefinition>();
                }

                // Auto-create 6 tiers if empty
                if (statDef.tiers.Count == 0)
                {
                    for (int i = 1; i <= 6; i++)
                    {
                        statDef.tiers.Add(new StatTierDefinition { tier = i });
                    }
                }
                else
                {
                    // Ensure the tier numbers are always correctly sequential (1, 2, 3...)
                    for (int i = 0; i < statDef.tiers.Count; i++)
                    {
                        statDef.tiers[i].tier = i + 1;
                    }
                }
            }
        }
    }
}
