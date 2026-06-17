using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class DungeonSack : MonoBehaviour
{
    public static DungeonSack Instance { get; private set; }

    [Header("Optional reference (assign in Inspector if you want)")]
    public DungeonSackUI dungeonSackUIReference;

    public List<ItemData> sackedItems = new List<ItemData>();

    public event Action OnSackChanged;

    private void Awake()
    {
        if (Instance == null) 
        {
            Instance = this;
            LoadSack(); // Load dữ liệu từ Json khi game chạy
            DontDestroyOnLoad(gameObject);
        }
        else 
        {
            Destroy(gameObject);
        }
    }

    public void AddItem(ItemData item)
    {
        if (item == null) return;

        // Ensure icon path is always correct
        item.iconPath = "Amor_Pic/" + item.itemID;

        // Weapons should not stack
        bool canStack = string.IsNullOrEmpty(item.weaponClassName);

        var existing = canStack ? sackedItems.Find(x => x.itemID == item.itemID) : null;
        if (existing != null)
        {
            existing.stack += item.stack;
        }
        else
        {
            // Clone để không ảnh hưởng dữ liệu gốc
            ItemData clonedItem = item.Clone();
            clonedItem.equipped = false; // Đảm bảo đồ nhặt trong ngục không bao giờ tự mặc
            sackedItems.Add(clonedItem);
        }

        dungeonSackUIReference?.Refresh();
        OnSackChanged?.Invoke();
        SaveSack(); // Lưu vào Json
    }

    public bool RemoveItem(ItemData item, int amount = 1)
    {
        var existing = sackedItems.Find(x => x == item);
        if (existing == null) existing = sackedItems.Find(x => x.itemID == item.itemID);
        if (existing == null) return false;

        existing.stack -= amount;
        if (existing.stack <= 0) sackedItems.Remove(existing);
        
        OnSackChanged?.Invoke();
        SaveSack(); // Lưu vào Json
        return true;
    }

    public void Clear()
    {
        sackedItems.Clear();
        OnSackChanged?.Invoke();
        SaveSack(); // Lưu vào Json để xóa file
        Debug.Log("[DungeonSack] Sacked items have been cleared (Player died).");
    }

    public void TransferToInventory()
    {
        if (Inventory.Instance == null)
        {
            Debug.LogError("[DungeonSack] Cannot transfer to Inventory because Inventory.Instance is null!");
            return;
        }

        int count = 0;
        foreach (var item in sackedItems)
        {
            // Chuyển đồ sang Inventory chính
            Inventory.Instance.AddItem(item);
            count++;
        }

        Debug.Log($"[DungeonSack] Successfully transferred {count} items to main Inventory (Player survived).");
        
        // Sau khi nhồi hết đồ thì xóa sạch cái túi tạm này đi (hàm Clear đã bao gồm SaveSack)
        Clear();
    }

    // --- SAVE / LOAD SYSTEM ---
    private string GetSavePath()
    {
        return System.IO.Path.Combine(Application.streamingAssetsPath, "dungeonsack_save.json");
    }

    public void SaveSack()
    {
        InventorySaveData data = new InventorySaveData { items = this.sackedItems };
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(GetSavePath(), json);
        Debug.Log("[DungeonSack] Saved to: " + GetSavePath());
    }

    public void LoadSack()
    {
        string path = GetSavePath();
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            InventorySaveData data = JsonUtility.FromJson<InventorySaveData>(json);
            if (data != null && data.items != null)
            {
                sackedItems = data.items;
                
                foreach (var item in sackedItems)
                {
                    item.iconPath = "Amor_Pic/" + item.itemID;
                    item.equipped = false; // Đảm bảo luôn tắt trạng thái Equip
                }
                
                Debug.Log("[DungeonSack] Loaded from: " + path);
            }
        }
        else
        {
            Debug.Log("[DungeonSack] No save file found at: " + path);
        }
    }
}
