using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Handles loot drops when enemies are defeated
/// Can drop forging materials or regular items
/// </summary>
public class EnemyLootTable : MonoBehaviour
{
    [System.Serializable]
    public class LootEntry
    {
        public string itemID;                    // Item or Material ID
        public LootType lootType = LootType.Item;
        public int minQuantity = 1;
        public int maxQuantity = 1;
        public float dropChance = 1f;            // 0-1, probability to drop
    }

    public enum LootType
    {
        Item,
        Material,
        Gold
    }

    [SerializeField] private List<LootEntry> lootTable = new List<LootEntry>();
    [SerializeField] private int minGoldDrop = 10;
    [SerializeField] private int maxGoldDrop = 50;

    /// <summary>
    /// Roll for loot when enemy dies
    /// </summary>
    public void DropLoot(Vector3 dropPosition)
    {
        var inventory = Inventory.Instance;
        var forgingSystem = ForgingSystem.Instance;
        var playerStat = PlayerStat.Instance;

        foreach (var entry in lootTable)
        {
            if (Random.value > entry.dropChance) continue;

            int quantity = Random.Range(entry.minQuantity, entry.maxQuantity + 1);

            switch (entry.lootType)
            {
                case LootType.Item:
                    // TODO: Add item to inventory
                    Debug.Log($"[EnemyLootTable] Dropped item: {entry.itemID} x{quantity}");
                    break;

                case LootType.Material:
                    if (forgingSystem != null)
                    {
                        forgingSystem.AddMaterial(entry.itemID, quantity);
                        Debug.Log($"[EnemyLootTable] Dropped material: {entry.itemID} x{quantity}");
                    }
                    break;

                case LootType.Gold:
                    int goldAmount = Random.Range(minGoldDrop, maxGoldDrop + 1);
                    if (playerStat != null)
                    {
                        playerStat.AddGold(goldAmount);
                        Debug.Log($"[EnemyLootTable] Dropped gold: {goldAmount}");
                    }
                    break;
            }
        }
    }
}
