using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class RarityProbability
{
    [Range(1, 5)] public int rarityLevel; // 1: Common, 2: Uncommon, 3: Rare, 4: Epic, 5: Legendary
    [Tooltip("Probabilities must sum up to 100 or less. If less, the remainder means no stat roll or base stat.")]
    public float chanceT1;
    public float chanceT2;
    public float chanceT3;
    public float chanceT4;
    public float chanceT5;
    public float chanceT6;

    public int RollTier()
    {
        float roll = Random.Range(0f, 100f);
        float cumulative = 0f;

        cumulative += chanceT6;
        if (roll <= cumulative) return 6;

        cumulative += chanceT5;
        if (roll <= cumulative) return 5;

        cumulative += chanceT4;
        if (roll <= cumulative) return 4;

        cumulative += chanceT3;
        if (roll <= cumulative) return 3;

        cumulative += chanceT2;
        if (roll <= cumulative) return 2;

        cumulative += chanceT1;
        if (roll <= cumulative) return 1;

        // Default fallback
        return 1;
    }
}

[CreateAssetMenu(fileName = "New Rarity Config", menuName = "Item Data/Rarity Config")]
public class RarityConfig : ScriptableObject
{
    public List<RarityProbability> probabilities = new List<RarityProbability>();

    // Helper: weights to pick a random rarity (Common to Legendary) if not provided.
    [Header("Drop Rarity Weights")]
    public float weightCommon = 60f;
    public float weightUncommon = 25f;
    public float weightRare = 10f;
    public float weightEpic = 4f;
    public float weightLegendary = 1f;

    public RarityProbability GetProbability(int rarityLevel)
    {
        return probabilities.Find(p => p.rarityLevel == rarityLevel);
    }

    public int RollRandomRarity()
    {
        float totalWeight = weightCommon + weightUncommon + weightRare + weightEpic + weightLegendary;
        float roll = Random.Range(0f, totalWeight);

        if (roll <= weightCommon) return 1;
        roll -= weightCommon;

        if (roll <= weightUncommon) return 2;
        roll -= weightUncommon;

        if (roll <= weightRare) return 3;
        roll -= weightRare;

        if (roll <= weightEpic) return 4;

        return 5;
    }
}
