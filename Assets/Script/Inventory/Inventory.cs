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
    
    // Lưu trữ Master toàn cục cho từng loại vũ khí
    public Dictionary<string, float> classMasteries = new Dictionary<string, float>();

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
            int maxAllowed = (item.equipmentType == EquipmentType.Weapon) ? 2 : 1;
            
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

            if (item.equipmentType == EquipmentType.Weapon)
            {
                WeaponData wData = item.BaseData as WeaponData;
                bool isTwoHanded = wData != null && wData.gripType == GripType.TwoHanded;

                if (isTwoHanded)
                {
                    // Nếu là vũ khí 2 tay, tự động tháo sạch mọi vũ khí đang cầm trên tay
                    foreach (var x in ownedItems)
                    {
                        if (x.equipped && x.equipmentType == EquipmentType.Weapon)
                        {
                            x.equipped = false;
                            OnItemEquipChanged?.Invoke(x, false);
                            x.equipSlot = EquipSlot.None;
                        }
                    }
                    item.equipSlot = EquipSlot.MainHand; // Cầm 2 tay, gắn vào slot Main
                }
                else
                {
                    // Vũ khí 1 tay: Kiểm tra xem đang cầm vũ khí 2 tay không
                    foreach (var x in ownedItems)
                    {
                        if (x.equipped && x.equipmentType == EquipmentType.Weapon)
                        {
                            WeaponData equippedWData = x.BaseData as WeaponData;
                            if (equippedWData != null && equippedWData.gripType == GripType.TwoHanded)
                            {
                                // Tháo vũ khí 2 tay cũ ra
                                x.equipped = false;
                                OnItemEquipChanged?.Invoke(x, false);
                                x.equipSlot = EquipSlot.None;
                                mainHandTaken = false;
                                offHandTaken = false;
                                currentlyEquippedCount = 0;
                            }
                        }
                    }

                    if (currentlyEquippedCount >= maxAllowed)
                    {
                        Debug.Log($"[Inventory] Blocked equipping {item.itemName}: Đã mặc 2 món One-Handed. Kín 2 tay.");
                        return; // Chặn không cho mặc thêm
                    }

                    if (!mainHandTaken) item.equipSlot = EquipSlot.MainHand;
                    else if (!offHandTaken) item.equipSlot = EquipSlot.OffHand;
                }
            }
            else
            {
                // Armor/Trang sức không dùng slot tay, nhưng cứ gán tạm MainHand hoặc None
                item.equipSlot = EquipSlot.None; 
            }
        }
        item.equipped = isTryingToEquip;

        // notify listeners (e.g., PlayerData, EquipmentManager) so they can use the CURRENT equipSlot
        OnItemEquipChanged?.Invoke(item, item.equipped);

        if (!isTryingToEquip)
        {
            item.equipSlot = EquipSlot.None;
        }

        // refresh UI
        inventoryUIReference?.Refresh();
        OnInventoryChanged?.Invoke();
        SaveInventory(); // Save changes
    }

    // NEW: Cho phép ép buộc trang bị một vũ khí vào ĐÚNG tay trái hoặc phải
    public void EquipToSpecificSlot(ItemData item, EquipSlot targetSlot)
    {
        if (item == null || !ownedItems.Contains(item) || !item.equippable || item.equipmentType != EquipmentType.Weapon) return;

        // Nếu vũ khí là loại 2 tay, không được chọn tay, xài hàm mặc định
        WeaponData wData = item.BaseData as WeaponData;
        if (wData != null && wData.gripType == GripType.TwoHanded)
        {
            ToggleEquip(item);
            return;
        }

        // 1. Nếu đang có vũ khí ở tay targetSlot, tháo nó ra trước
        foreach (var x in ownedItems)
        {
            if (x.equipped && x.equipSlot == targetSlot)
            {
                x.equipped = false;
                OnItemEquipChanged?.Invoke(x, false);
                x.equipSlot = EquipSlot.None;
            }
        }

        // 2. Mặc món mới vào đúng targetSlot
        item.equipSlot = targetSlot;
        item.equipped = true;
        OnItemEquipChanged?.Invoke(item, true);

        inventoryUIReference?.Refresh();
        OnInventoryChanged?.Invoke();
        SaveInventory();
    }

    public void Clear()
    {
        ownedItems.Clear();
        OnInventoryChanged?.Invoke();
        SaveInventory(); // Save changes
    }

    // --- SAVE / LOAD SYSTEM ---
    public void AddClassMastery(string className, float amount)
    {
        if (string.IsNullOrEmpty(className)) return;
        
        // Use GetClassMastery to fetch the most up-to-date value (which prioritizes PlayerStat Inspector)
        float currentVal = GetClassMastery(className);
        float newVal = Mathf.Min(currentVal + amount, 1000f);
        
        classMasteries[className] = newVal;
        
        if (PlayerStat.Instance != null)
        {
            PlayerStat.Instance.UpdateMasteryDisplay(className, newVal);
        }
        SaveInventory(); // Save immediately when mastery is earned
    }

    public float GetClassMastery(string className)
    {
        if (string.IsNullOrEmpty(className)) return 0f;
        
        // Priority 1: Read from PlayerStat so that Inspector modifications take instant effect
        if (PlayerStat.Instance != null)
        {
            switch (className.ToLower())
            {
                case "greatsword": return PlayerStat.Instance.greatswordMastery;
                case "katana": return PlayerStat.Instance.katanaMastery;
                case "warhammer": return PlayerStat.Instance.warhammerMastery;
                case "greatsaxe": return PlayerStat.Instance.greatsaxeMastery;
                case "spear": return PlayerStat.Instance.spearMastery;
                case "bow": return PlayerStat.Instance.bowMastery;
                case "staff": return PlayerStat.Instance.staffMastery;
                case "orb": return PlayerStat.Instance.orbMastery;
                case "dualblades": return PlayerStat.Instance.dualBladesMastery;
            }
        }

        // Priority 2: Fallback to the dictionary
        if (classMasteries.TryGetValue(className, out float val)) return val;
        return 0f;
    }

    private string GetSavePath()
    {
        return System.IO.Path.Combine(Application.persistentDataPath, "inventory_save.json");
    }

    public void SaveInventory()
    {
        // Sync display fields from PlayerStat back to classMasteries so Inspector modifications are saved
        if (PlayerStat.Instance != null)
        {
            classMasteries["Greatsword"] = PlayerStat.Instance.greatswordMastery;
            classMasteries["Katana"] = PlayerStat.Instance.katanaMastery;
            classMasteries["Warhammer"] = PlayerStat.Instance.warhammerMastery;
            classMasteries["Greatsaxe"] = PlayerStat.Instance.greatsaxeMastery;
            classMasteries["Spear"] = PlayerStat.Instance.spearMastery;
            classMasteries["Bow"] = PlayerStat.Instance.bowMastery;
            classMasteries["Staff"] = PlayerStat.Instance.staffMastery;
            classMasteries["Orb"] = PlayerStat.Instance.orbMastery;
            classMasteries["DualBlades"] = PlayerStat.Instance.dualBladesMastery;
        }

        InventorySaveData data = new InventorySaveData { items = this.ownedItems };
        data.classMasteries = new List<ClassMasteryData>();
        foreach(var kvp in classMasteries)
        {
            data.classMasteries.Add(new ClassMasteryData { weaponClassName = kvp.Key, masteryExp = kvp.Value });
        }
        
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
            if (data != null)
            {
                if (data.items != null)
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
                }
                
                classMasteries.Clear();
                if (data.classMasteries != null)
                {
                    foreach(var cm in data.classMasteries)
                    {
                        classMasteries[cm.weaponClassName] = cm.masteryExp;
                        if (PlayerStat.Instance != null)
                        {
                            PlayerStat.Instance.UpdateMasteryDisplay(cm.weaponClassName, cm.masteryExp);
                        }
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

    private void OnDestroy()
    {
        SaveInventory();
    }

    private void OnApplicationQuit()
    {
        SaveInventory();
    }
}

[Serializable]
public class InventorySaveData
{
    public List<ItemData> items;
    public List<ClassMasteryData> classMasteries;
}

[Serializable]
public class ClassMasteryData
{
    public string weaponClassName;
    public float masteryExp;
}
