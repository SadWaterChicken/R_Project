using UnityEngine;
using System.Collections.Generic;

public static class ItemGenerator
{
    private static RarityConfig rarityConfig;
    private static StatRollConfig statRollConfig;

    private static void LoadConfigs()
    {
        if (rarityConfig == null)
        {
            rarityConfig = Resources.Load<RarityConfig>("RarityConfig");
            if (rarityConfig == null) Debug.LogWarning("[ItemGenerator] RarityConfig not found in Resources folder.");
        }
        
        if (statRollConfig == null)
        {
            statRollConfig = Resources.Load<StatRollConfig>("StatRollConfig");
            if (statRollConfig == null) Debug.LogWarning("[ItemGenerator] StatRollConfig not found in Resources folder.");
        }
    }

    /// <summary>
    /// Generates a new ItemData with randomized sub-stats based on Rarity and Tier.
    /// Main stats are kept fixed.
    /// </summary>
    /// <param name="baseItem">The scriptable object containing the base stats.</param>
    /// <param name="forceRarity">Optional. If set between 1-5, forces the item to be that rarity instead of rolling.</param>
    /// <returns>A new randomized ItemData instance.</returns>
    public static ItemData GenerateLoot(BaseItemData baseItem, int forceRarity = -1)
    {
        LoadConfigs();

        if (baseItem == null)
        {
            Debug.LogError("[ItemGenerator] BaseItemData is null!");
            return null;
        }

        // 1. Create a new ItemData instance
        ItemData newItem = new ItemData(baseItem.itemID, 1);
        newItem.BaseData = baseItem;

        // 2. Determine Rarity
        int finalRarity = 1;
        if (forceRarity >= 1 && forceRarity <= 5)
        {
            finalRarity = forceRarity;
        }
        else if (rarityConfig != null)
        {
            finalRarity = rarityConfig.RollRandomRarity();
        }
        newItem.rarity = finalRarity;

        // 3. Roll Stats
        newItem.modifiers = new List<ItemData.StatMod>();

        foreach (var baseMod in baseItem.baseModifiers)
        {
            ItemData.StatMod newMod = new ItemData.StatMod
            {
                stat = baseMod.stat,
                percent = baseMod.percent,
                isMainStat = baseMod.isMainStat,
                statTier = 0 // default for main stat
            };

            if (baseMod.isMainStat)
            {
                // Main stats are fixed, just copy the value
                newMod.value = baseMod.value;
                newMod.percentValue = baseMod.percentValue;
            }
            else
            {
                // Sub-stats are randomized based on Tier and Rarity
                if (rarityConfig != null && statRollConfig != null)
                {
                    RarityProbability rarityProb = rarityConfig.GetProbability(finalRarity);
                    if (rarityProb != null)
                    {
                        // Roll which Tier this stat gets
                        int rolledTier = rarityProb.RollTier();
                        newMod.statTier = rolledTier;

                        // Find the Min-Max range for this Tier
                        StatDefinition statDef = statRollConfig.GetStatDefinition(baseMod.stat, baseMod.percent);
                        if (statDef != null)
                        {
                            StatTierDefinition tierDef = statDef.tiers.Find(t => t.tier == rolledTier);
                            if (tierDef != null)
                            {
                                float randomVal = Random.Range(tierDef.minValue, tierDef.maxValue);
                                
                                if (baseMod.percent)
                                {
                                    // Set percentValue directly to randomVal (0-100 format)
                                    newMod.percentValue = randomVal;
                                    // Normally you apply this percent to some base stat later in your stats system
                                    newMod.value = 0; 
                                }
                                else
                                {
                                    newMod.value = Mathf.Round(randomVal); // Flat stats are usually integers
                                }
                            }
                            else
                            {
                                Debug.LogWarning($"[ItemGenerator] StatRollConfig missing Tier {rolledTier} for Stat {baseMod.stat}");
                                newMod.value = baseMod.value;
                            }
                        }
                        else
                        {
                            Debug.LogWarning($"[ItemGenerator] StatRollConfig missing definition for Stat {baseMod.stat}");
                            newMod.value = baseMod.value;
                        }
                    }
                    else
                    {
                        newMod.value = baseMod.value;
                    }
                }
                else
                {
                    newMod.value = baseMod.value;
                }
            }
            
            newItem.modifiers.Add(newMod);
        }

        return newItem;
    }
}
