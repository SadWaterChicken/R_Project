using System;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public static Inventory Instance { get; private set; }

    // Optional inspector reference (nếu bạn muốn gán UI trực tiếp)
    [Header("Optional reference (assign in Inspector if you want)")]
    public InventoryUI inventoryUIReference;

    // read-only list for UI
    public List<ItemData> ownedItems = new List<ItemData>();

    // event notify UI to refresh
    public event Action OnInventoryChanged;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void AddItem(ItemData item)
    {
        if (item == null) return;

        var existing = ownedItems.Find(x => x.itemID == item.itemID);
        if (existing != null)
        {
            existing.stack += item.stack;
        }
        else
        {
            var clone = new ItemData(item.itemID, item.itemName, item.description, item.price, item.iconPath, item.stack);
            ownedItems.Add(clone);
        }

        Debug.Log($"[Inventory] Added {item.itemName} x{item.stack}");
        // If inspector reference set, explicitly refresh it (safe)
        if (inventoryUIReference != null) inventoryUIReference.Refresh();
        // always fire event for listeners
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
