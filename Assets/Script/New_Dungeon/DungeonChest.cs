using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace New_Dungeon
{
    public class DungeonChest : MonoBehaviour, IInteractable
    {
        [Header("Chest Loot Settings")]
        [Tooltip("The list of items this chest can potentially drop into the Dungeon Sack.")]
        public List<ItemData> possibleRewards;

        [Header("Events")]
        [Tooltip("Triggered when the chest is opened. Used by Event Rooms to continue the wave logic.")]
        public UnityEvent onChestOpened = new UnityEvent();

        public void Interact()
        {
            GiveReward();
            
            // Invoke the unity event so anyone listening (like EventRoomController) knows it opened
            onChestOpened?.Invoke();
            
            // Destroy this chest once opened
            Destroy(gameObject);
        }

        private void GiveReward()
        {
            if (possibleRewards == null || possibleRewards.Count == 0)
            {
                Debug.LogWarning("[DungeonChest] Chest has no possible rewards!");
                return;
            }

            // Chọn ngẫu nhiên 1 món đồ từ list
            int randomIndex = Random.Range(0, possibleRewards.Count);
            ItemData rewardItem = possibleRewards[randomIndex];

            if (rewardItem == null) return;

            // Kiểm tra xem có đang ở trong Dungeon không
            string currentScene = SceneManager.GetActiveScene().name.ToLower();
            bool isDungeon = currentScene.Contains("dungeon");

            if (isDungeon)
            {
                if (DungeonSack.Instance != null)
                {
                    DungeonSack.Instance.AddItem(rewardItem);
                    Debug.Log($"[DungeonChest] Added {rewardItem.itemName} to DungeonSack!");
                }
                else
                {
                    Debug.LogWarning("[DungeonChest] DungeonSack.Instance is null! Lost item.");
                }
            }
            else
            {
                if (Inventory.Instance != null)
                {
                    Inventory.Instance.AddItem(rewardItem);
                    Debug.Log($"[DungeonChest] Added {rewardItem.itemName} to main Inventory!");
                }
            }
        }
    }
}
