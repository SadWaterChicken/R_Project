using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public static Inventory Instance { get; private set; }

    [Header("Optional reference (assign in Inspector if you want)")]
    public InventoryUI inventoryUIReference;

    public List<ItemData> ownedItems = new List<ItemData>();

    public event Action OnInventoryChanged;
    // NEW: notify when an item is equipped/unequipped
    public event Action<ItemData, bool> OnItemEquipChanged;

    private void Awake()
    {
        if (Instance == null) 
        {
            Instance = this;
            LoadInventory(); // Load data on startup
        }
        else Destroy(gameObject);
    }

    public void AddItem(ItemData item)
    {
        if (item == null) return;

        // Weapons should not stack so they can have individual mastery/forge levels
        bool canStack = string.IsNullOrEmpty(item.weaponClassName);

        var existing = canStack ? ownedItems.Find(x => x.itemID == item.itemID && !x.equipped) : null;
        if (existing != null)
        {
            existing.stack += item.stack;
        }
        else
        {
            // Preserve modifiers/equip flags
            ownedItems.Add(item.Clone());
        }

        inventoryUIReference?.Refresh();
        OnInventoryChanged?.Invoke();
        SaveInventory(); // Save changes
    }

    public bool RemoveItem(ItemData item, int amount = 1)
    {
        // Try to find exact instance first (crucial for unstackable weapons)
        var existing = ownedItems.Find(x => x == item);
        
        // Fallback to finding by itemID (for materials or cloned references)
        if (existing == null) 
        {
            existing = ownedItems.Find(x => x.itemID == item.itemID);
        }

        if (existing == null) return false;

        // Ensure we unequip before removing to safely remove player stats
        if (existing.equipped && existing.stack <= amount)
        {
            existing.equipped = false;
            OnItemEquipChanged?.Invoke(existing, false);
        }

        existing.stack -= amount;
        if (existing.stack <= 0) ownedItems.Remove(existing);
        OnInventoryChanged?.Invoke();
        SaveInventory(); // Save changes
        return true;
    }

    // NEW: toggle equip state for an item already in ownedItems
    public void ToggleEquip(ItemData item)
    {
        if (item == null) return;
        if (!ownedItems.Contains(item)) return;
        if (!item.equippable) return;

        bool isTryingToEquip = !item.equipped;

        if (isTryingToEquip && item.equipmentType != EquipmentType.None)
        {
            // Check if another item of the same EquipmentType is already equipped
            bool isSlotTaken = ownedItems.Exists(x => x != item && x.equipped && x.equipmentType == item.equipmentType);
            if (isSlotTaken)
            {
                Debug.Log($"[Inventory] Blocked equipping {item.itemName}: An item of type {item.equipmentType} is already equipped. Please unequip it first.");
                return; // Block equipping!
            }
        }

        item.equipped = isTryingToEquip;

        // notify listeners (e.g., PlayerData) and refresh UI
        OnItemEquipChanged?.Invoke(item, item.equipped);
        inventoryUIReference?.Refresh();
        OnInventoryChanged?.Invoke();
        SaveInventory(); // Save changes
    }

    public void Clear()
    {
        ownedItems.Clear();
        OnInventoryChanged?.Invoke();
        SaveInventory(); // Save changes
    }

    // --- SAVE / LOAD SYSTEM ---
    private string GetSavePath()
    {
        return System.IO.Path.Combine(Application.streamingAssetsPath, "inventory_save.json");
    }

    public void SaveInventory()
    {
        InventorySaveData data = new InventorySaveData { items = this.ownedItems };
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(GetSavePath(), json);
        Debug.Log("[Inventory] Saved to: " + GetSavePath());
    }

    public void LoadInventory()
    {
        string path = GetSavePath();
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            InventorySaveData data = JsonUtility.FromJson<InventorySaveData>(json);
            if (data != null && data.items != null)
            {
                ownedItems = data.items;
                Debug.Log("[Inventory] Loaded from: " + path);
            }
        }
        else
        {
            Debug.Log("[Inventory] No save file found at: " + path);
        }
    }
}

[Serializable]
public class InventorySaveData
{
    public List<ItemData> items;
}
