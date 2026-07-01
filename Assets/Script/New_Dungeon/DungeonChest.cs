using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace New_Dungeon
{
    public class DungeonChest : MonoBehaviour, IInteractable
    {
        [System.Serializable]
        public class DungeonChestReward
        {
            public BaseItemData itemData;
            public int minQuantity = 1;
            public int maxQuantity = 1;
            public float dropChance = 1f; // 0.0 to 1.0
            [Tooltip("If checked, the item will roll random sub-stats based on Rarity (T1-T6) using ItemGenerator.")]
            public bool isEquipment = false;
        }

        [Header("Chest Loot Settings")]
        [Tooltip("The list of items this chest can potentially drop.")]
        public List<DungeonChestReward> possibleRewards = new List<DungeonChestReward>();

        [Header("Dungeon Transfer Options")]
        [Tooltip("Nếu tick, khi mở rương này sẽ rút toàn bộ đồ từ DungeonSack ném vào Inventory thật của người chơi.")]
        public bool isFinalRewardChest = false;

        [Header("Gold Reward")]
        public int minGold = 10;
        public int maxGold = 50;

        [Header("Events")]
        [Tooltip("Triggered when the chest is opened. Used by Event Rooms to continue the wave logic.")]
        public UnityEvent onChestOpened = new UnityEvent();

        public void Interact()
        {
            GiveReward();
            
            // Nếu đây là rương thưởng cuối, hút sạch đồ từ túi ảo về túi thật
            if (isFinalRewardChest && DungeonSack.Instance != null)
            {
                Debug.Log("[DungeonChest] Đang chuyển toàn bộ đồ từ Dungeon Sack về Inventory chính...");
                DungeonSack.Instance.TransferToInventory();
            }
            
            // Invoke the unity event so anyone listening (like EventRoomController) knows it opened
            onChestOpened?.Invoke();
            
            // Destroy this chest once opened
            Destroy(gameObject);
        }

        private void GiveReward()
        {
            var inventory = Inventory.Instance;
            var playerStat = PlayerStat.Instance;

            // 1. Give Gold
            int goldAmount = Random.Range(minGold, maxGold + 1);
            if (goldAmount > 0 && playerStat != null)
            {
                playerStat.AddGold(goldAmount);
                Debug.Log($"[DungeonChest] Found {goldAmount} gold!");
            }

            if (possibleRewards == null || possibleRewards.Count == 0)
            {
                Debug.LogWarning("[DungeonChest] Chest has no possible rewards!");
                return;
            }

            string currentScene = SceneManager.GetActiveScene().name.ToLower();
            bool isDungeon = currentScene.Contains("dungeon");

            // 2. Give Items
            foreach (var reward in possibleRewards)
            {
                if (Random.value > reward.dropChance) continue;
                if (reward.itemData == null) continue;

                int quantity = Random.Range(reward.minQuantity, reward.maxQuantity + 1);
                ItemData newItem = null;

                if (reward.isEquipment)
                {
                    newItem = ItemGenerator.GenerateLoot(reward.itemData);
                    newItem.stack = quantity;
                }
                else
                {
                    newItem = new ItemData(reward.itemData.itemID, quantity);
                    newItem.BaseData = reward.itemData;
                }

                if (isDungeon)
                {
                    if (DungeonSack.Instance != null)
                    {
                        DungeonSack.Instance.AddItem(newItem);
                        Debug.Log($"[DungeonChest] Added {newItem.itemName} to DungeonSack!");
                    }
                }
                else
                {
                    if (inventory != null)
                    {
                        inventory.AddItem(newItem);
                        Debug.Log($"[DungeonChest] Added {newItem.itemName} to main Inventory!");
                    }
                }
            }
        }
    }
}
