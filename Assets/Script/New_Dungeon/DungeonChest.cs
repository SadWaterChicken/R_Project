using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

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
            // Logic temporarily removed. The chest will just disappear on interact.
            Debug.Log("[DungeonChest] Chest opened! (Reward logic is currently disabled)");
        }
    }
}
