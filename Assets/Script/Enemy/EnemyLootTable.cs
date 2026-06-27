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
        [Tooltip("Kéo thả file vũ khí/nguyên liệu từ thư mục ItemDatabase vào đây")]
        public BaseItemData itemData;            // Reference instead of string
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
                    if (entry.itemData == null)
                    {
                        Debug.LogWarning("[EnemyLootTable] LootEntry has no ItemData assigned!");
                        continue;
                    }
                    
                    string cleanID = entry.itemData.itemID.Trim();
                    ItemData newItem = new ItemData(cleanID, quantity);
                    newItem.BaseData = entry.itemData; // Gán trực tiếp dữ liệu luôn, khỏi phải Load lại!
                    
                    if (DungeonSack.Instance != null)
                    {
                        DungeonSack.Instance.AddItem(newItem);
                        Debug.Log($"[EnemyLootTable] Added {cleanID} x{quantity} to DungeonSack!");
                    }
                    else if (inventory != null)
                    {
                        inventory.AddItem(newItem);
                        Debug.Log($"[EnemyLootTable] Added {cleanID} x{quantity} to Inventory!");
                    }
                    break;

                case LootType.Material:
                    if (entry.itemData == null) continue;
                    
                    if (forgingSystem != null)
                    {
                        forgingSystem.AddMaterial(entry.itemData.itemID, quantity);
                        Debug.Log($"[EnemyLootTable] Dropped material: {entry.itemData.itemID} x{quantity}");
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

