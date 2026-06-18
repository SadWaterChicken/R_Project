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
        if (Instance != null && Instance != this)
        {
            if (transform.root != transform)
            {
                Destroy(transform.root.gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
            return;
        }

        Instance = this;
        if (transform.root != transform)
        {
            DontDestroyOnLoad(transform.root.gameObject);
        }
        else
        {
            DontDestroyOnLoad(gameObject);
        }
        LoadInventory(); // Load data on startup
    }

    public void AddItem(ItemData item)
    {
        if (item == null) return;

        // Ensure icon path is always correct (Now handled by BaseItemData)
        // item.iconPath = "Amor_Pic/" + item.itemID;

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
            // Cài đặt số lượng đồ tối đa được phép mặc cùng lúc (Vũ khí cho phép 2 tay = 2 món)
            int maxAllowed = (item.equipmentType == EquipmentType.PrimaryWeapon) ? 2 : 1;
            
            // Đếm xem người chơi đang mặc bao nhiêu món thuộc loại này rồi
            int currentlyEquippedCount = 0;
            bool mainHandTaken = false;
            bool offHandTaken = false;

            foreach (var x in ownedItems)
            {
                // Bỏ qua chính nó, đếm các món khác có cùng loại và đang được mặc
                if (x != item && x.equipped && x.equipmentType == item.equipmentType)
                {
                    currentlyEquippedCount++;
                    if (x.equipSlot == EquipSlot.MainHand) mainHandTaken = true;
                    if (x.equipSlot == EquipSlot.OffHand) offHandTaken = true;
                }
            }

            if (currentlyEquippedCount >= maxAllowed)
            {
                Debug.Log($"[Inventory] Blocked equipping {item.itemName}: Đã mặc tối đa {maxAllowed} món thuộc loại {item.equipmentType}. Hãy tháo bớt ra trước.");
                return; // Chặn không cho mặc thêm
            }

            // Gán Slot
            if (item.equipmentType == EquipmentType.PrimaryWeapon)
            {
                if (!mainHandTaken) item.equipSlot = EquipSlot.MainHand;
                else if (!offHandTaken) item.equipSlot = EquipSlot.OffHand;
            }
            else
            {
                // Armor/Trang sức không dùng slot tay, nhưng cứ gán tạm MainHand hoặc None
                item.equipSlot = EquipSlot.None; 
            }
        }
        else if (!isTryingToEquip)
        {
            item.equipSlot = EquipSlot.None;
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
        return System.IO.Path.Combine(Application.persistentDataPath, "inventory_save.json");
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
                
                // Ensure all loaded items fetch their BaseData
                foreach (var item in ownedItems)
                {
                    if (item.BaseData == null) {
                        // Kích hoạt property getter để load BaseData từ Resources
                        var _ = item.BaseData;
                    }
                }
                
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
