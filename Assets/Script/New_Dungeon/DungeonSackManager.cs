using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace New_Dungeon
{
    public class DungeonSackManager : MonoBehaviour
    {
        public static DungeonSackManager Instance { get; private set; }

        [Header("Configuration")]
        public DungeonSackData equippedSack; // ScriptableObject defining capacity

        [Header("Runtime Data")]
        // Items stored in the dungeon sack
        public List<ItemData> sackItems = new List<ItemData>();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public int GetMaxCapacity()
        {
            if (equippedSack != null)
                return equippedSack.capacity;
            return 5; // Default if none equipped
        }

        public bool IsSackFull()
        {
            return sackItems.Count >= GetMaxCapacity();
        }

        public bool AddItemToSack(ItemData item)
        {
            if (IsSackFull())
            {
                Debug.LogWarning("[DungeonSack] Sack is full! Cannot add " + item.itemName);
                return false;
            }

            sackItems.Add(item.Clone());
            Debug.Log($"[DungeonSack] Added {item.itemName}. Current size: {sackItems.Count}/{GetMaxCapacity()}");
            return true;
        }

        /// <summary>
        /// Called when the player dies in the dungeon.
        /// Deletes all items in the sack. DOES NOT touch main inventory.
        /// </summary>
        public void OnPlayerDeathInDungeon()
        {
            Debug.Log("[DungeonSack] Player died. Losing all items in Dungeon Sack!");
            // Event buff items are picked up directly into the normal inventory,
            // so everything in this sack is strictly dungeon-loot and should be lost.
            sackItems.Clear();
        }

        /// <summary>
        /// Called when the player successfully escapes the dungeon.
        /// Transfers all sack items to the main inventory.
        /// </summary>
        public void OnDungeonEscape()
        {
            Debug.Log("[DungeonSack] Player escaped! Transferring items to main inventory.");
            
            // TODO: Call your Main Inventory Manager here to add items.
            // foreach(var item in sackItems) { MainInventory.Add(item); }

            // Clear the sack after transfer
            sackItems.Clear();
        }
    }
}
