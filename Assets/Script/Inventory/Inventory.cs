using System;
using System.Collections.Generic;
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
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void AddItem(ItemData item)
    {
        if (item == null) return;

        var existing = ownedItems.Find(x => x.itemID == item.itemID && !x.equipped);
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
    }

    public bool RemoveItem(ItemData item, int amount = 1)
    {
        var existing = ownedItems.Find(x => x.itemID == item.itemID);
        if (existing == null) return false;

        existing.stack -= amount;
        if (existing.stack <= 0) ownedItems.Remove(existing);
        OnInventoryChanged?.Invoke();
        return true;
    }

    // NEW: toggle equip state for an item already in ownedItems
    public void ToggleEquip(ItemData item)
    {
        if (item == null) return;
        if (!ownedItems.Contains(item)) return;
        if (!item.equippable) return;

        item.equipped = !item.equipped;

        // notify listeners (e.g., PlayerData) and refresh UI
        OnItemEquipChanged?.Invoke(item, item.equipped);
        inventoryUIReference?.Refresh();
        OnInventoryChanged?.Invoke();
    }

    public void Clear()
    {
        ownedItems.Clear();
        OnInventoryChanged?.Invoke();
    }
}
