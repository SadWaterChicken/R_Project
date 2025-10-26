using System;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-100)] // ensure Inventory.Awake runs before PlayerData
public class Inventory : MonoBehaviour
{
    public static Inventory Instance { get; private set; }

    [Header("Optional reference (assign in Inspector if you want)")]
    public InventoryUI inventoryUIReference;

    public List<ItemData> ownedItems = new List<ItemData>();

    public event Action OnInventoryChanged;
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
        if (existing != null) existing.stack += item.stack;
        else
        {
            var clone = new ItemData(item.itemID, item.itemName, item.description, item.price, item.iconPath, item.stack)
            {
                equippable = item.equippable,
                equipped = false,
                modifiers = item.modifiers != null ? new List<ItemData.StatMod>(item.modifiers) : new List<ItemData.StatMod>()
            };
            ownedItems.Add(clone);
        }

        inventoryUIReference?.Refresh();
        OnInventoryChanged?.Invoke();
    }

    public void ToggleEquip(ItemData item)
    {
        if (item == null || !ownedItems.Contains(item) || !item.equippable) return;
        item.equipped = !item.equipped;
        OnItemEquipChanged?.Invoke(item, item.equipped);
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

    public void Clear()
    {
        ownedItems.Clear();
        OnInventoryChanged?.Invoke();
    }
}
