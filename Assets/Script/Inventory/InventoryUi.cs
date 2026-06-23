using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using TMP_Text = TMPro.TextMeshProUGUI;

public class InventoryUI : MonoBehaviour
{
    [Header("References")]
    public GameObject itemSlotPrefab;
    public Transform contentParent;
    public GameObject detailPanel;
    public Image detailIcon;
    public TMP_Text detailName;
    public TMP_Text detailDesc;
    public TMP_Text detailPrice;
    public TMP_Text detailQty;
    public TMP_Text detailStats;
    
    [Header("Mastery UI")]
    public Slider detailMasteryBar;
    public TMP_Text detailMasteryText;

    [Header("Buttons")]
    public Button useButton;
    public Button closeDetailButton;
    public float toggleDebounce = 0.15f;

    private List<GameObject> spawned = new List<GameObject>();
    private bool visible = false;
    private ItemData _lastSelectedItem;
    private float _nextToggleAllowedTime;
    private static int s_lastToggleFrame = -1;

    private void Awake()
    {
        gameObject.SetActive(false);
        if (detailPanel != null) detailPanel.SetActive(false);
    }

    private void OnEnable()
    {
        if (Inventory.Instance != null)
            Inventory.Instance.OnInventoryChanged += Refresh;
            
        if (CursorManager.Instance != null)
            CursorManager.OnCloseAllUI += CloseInventory;
    }

    private void OnDisable()
    {
        if (Inventory.Instance != null)
            Inventory.Instance.OnInventoryChanged -= Refresh;
            
        if (CursorManager.Instance != null)
            CursorManager.OnCloseAllUI -= CloseInventory;
    }

    private void CloseInventory()
    {
        if (visible)
        {
            visible = false;
            gameObject.SetActive(false);
            if (CursorManager.Instance != null) CursorManager.Instance.SetUIOpen(false);
        }
    }

    // Toggle inventory visibility (now guarded)
    public void Toggle()
    {
        // Ignore re-entrant toggles in the same frame
        if (s_lastToggleFrame == Time.frameCount) return;

        // Debounce: ignore if pressed too quickly (use unscaled time so it works while paused)
        if (Time.unscaledTime < _nextToggleAllowedTime) return;

        s_lastToggleFrame = Time.frameCount;
        _nextToggleAllowedTime = Time.unscaledTime + toggleDebounce;

        visible = !visible;
        gameObject.SetActive(visible);
        if (visible) Refresh();
        
        if (CursorManager.Instance != null)
        {
            CursorManager.Instance.SetUIOpen(visible);
        }
    }

    // Refresh grid from Inventory.Instance
    public void Refresh()
    {
        foreach (var go in spawned) Destroy(go);
        spawned.Clear();

        if (Inventory.Instance == null || itemSlotPrefab == null || contentParent == null) return;

        var items = Inventory.Instance.ownedItems;
        foreach (var item in items)
        {
            GameObject slotGO = Instantiate(itemSlotPrefab, contentParent);
            spawned.Add(slotGO);

            // Prefer ShopItemSlotUI if present
            var shopSlot = slotGO.GetComponent<ShopItemSlotUI>();
            if (shopSlot != null)
            {
                shopSlot.Init(item, ShowDetail);
                continue;
            }

            // Fallback to ItemSlotUI if that’s the prefab you’re using for inventory
            var invSlot = slotGO.GetComponent<ItemSlotUI>();
            if (invSlot != null)
            {
                invSlot.SetItem(item, item.stack);
                var btn = invSlot.button != null ? invSlot.button : slotGO.GetComponent<Button>();
                if (btn != null)
                {
                    btn.onClick.RemoveAllListeners();
                    btn.onClick.AddListener(() => ShowDetail(item));
                }
                continue;
            }

            Debug.LogWarning("[InventoryUI] itemSlotPrefab has neither ShopItemSlotUI nor ItemSlotUI.", slotGO);
        }

        // Auto-select first item or remember last selected
        if (items.Count > 0)
        {
            ItemData itemToSelect = items[0];
            if (detailPanel != null && detailPanel.activeSelf && _lastSelectedItem != null && items.Contains(_lastSelectedItem))
            {
                itemToSelect = _lastSelectedItem;
            }
            ShowDetail(itemToSelect);
        }
        else
        {
            if (detailPanel != null) detailPanel.SetActive(false);
        }
    }

    public void ShowDetail(ItemData item)
    {
        if (item == null) return;
        _lastSelectedItem = item;

        if (detailPanel != null) detailPanel.SetActive(true);
        if (detailIcon != null) 
        {
            detailIcon.sprite = string.IsNullOrEmpty(item.iconPath) ? null : Resources.Load<Sprite>(item.iconPath);
            detailIcon.color = detailIcon.sprite == null ? new Color(1, 1, 1, 0) : Color.white;
        }
        if (detailName != null) detailName.text = item.itemName;
        if (detailDesc != null) detailDesc.text = item.description;
        if (detailPrice != null) detailPrice.text = $"Price: {item.price}";
        if (detailQty != null) detailQty.text = $"Qty: {item.stack}";

        // Hiển thị thanh Mastery (Tiến trình)
        if (detailMasteryBar != null)
        {
            // Lấy giới hạn Max Mastery từ ForgeManager (nếu có), mặc định là 100
            float maxMastery = ForgeManager.Instance != null ? ForgeManager.Instance.GetMaxMastery() : 100f;
            
            // Chỉ hiển thị thanh Mastery nếu món đồ này có thể mặc (vũ khí)
            bool showMastery = item.equippable;
            detailMasteryBar.gameObject.SetActive(showMastery);
            
            if (showMastery)
            {
                detailMasteryBar.maxValue = maxMastery;
                detailMasteryBar.value = item.weaponMastery;
            }
        }

        if (detailMasteryText != null)
        {
            float maxMastery = ForgeManager.Instance != null ? ForgeManager.Instance.GetMaxMastery() : 100f;
            bool showMastery = item.equippable;
            detailMasteryText.gameObject.SetActive(showMastery);
            
            if (showMastery)
            {
                detailMasteryText.text = $"Mastery: {item.weaponMastery:F1} / {maxMastery:F0}";
            }
        }

        // Flexible stats
        var statsText = BuildStatsText(item);
        if (detailStats != null) detailStats.text = statsText;
        else if (detailDesc != null && !string.IsNullOrEmpty(statsText))
            detailDesc.text = item.description + "\n" + statsText;

        // ← SỬA: Thay đổi cách xử lý Use/Unequip button
        if (useButton != null)
        {
            useButton.onClick.RemoveAllListeners();
            
            // ← QUAN TRỌNG: Gọi method mới thay vì trực tiếp ToggleEquip
            useButton.onClick.AddListener(() => OnUseButtonClick(item));
            
            // Update button text ban đầu
            UpdateButtonText(item);
        }

        if (closeDetailButton != null)
        {
            closeDetailButton.onClick.RemoveAllListeners();
            closeDetailButton.onClick.AddListener(() => detailPanel.SetActive(false));
        }
    }

    // ← THÊM METHOD MỚI: Xử lý khi click Use/Unequip button
    private void OnUseButtonClick(ItemData item)
    {
        if (item == null || Inventory.Instance == null) return;
        
        // Toggle equip state
        Inventory.Instance.ToggleEquip(item);
        
        // ← QUAN TRỌNG: Update button text NGAY LẬP TỨC
        UpdateButtonText(item);
    }

    // ← THÊM METHOD MỚI: Update button text
    private void UpdateButtonText(ItemData item)
    {
        if (useButton == null || item == null) return;
        
        var label = useButton.GetComponentInChildren<TMP_Text>();
        if (label != null)
        {
            label.text = item.equipped ? "Unequip" : "Use";
        }
    }

    private static string BuildStatsText(ItemData item)
    {
        if (item.modifiers == null || item.modifiers.Count == 0) return "";
        var sb = new StringBuilder();

        // 1. In ra Dòng Chính trước (Màu Vàng / Cam) - Tương thích ngược với các file JSON cũ
        foreach (var m in item.modifiers)
        {
            if (m.isMainStat)
            {
                var sign = m.value >= 0 ? "+" : "";
                var val = m.percent ? $"{sign}{m.value}%" : $"{sign}{m.value}";
                sb.AppendLine($"<color=#FFB300><b>{m.stat}: {val}</b></color>");
            }
        }

        // 2. In ra Dòng Phụ (Màu Trắng/Xám)
        foreach (var m in item.modifiers)
        {
            if (!m.isMainStat)
            {
                var sign = m.value >= 0 ? "+" : "";
                var val = m.percent ? $"{sign}{(m.percentValue * 100).ToString("0.##")}%" : $"{sign}{m.value}";
                sb.AppendLine($"  <color=#DDDDDD>• {m.stat}: {val}</color>");
            }
        }

        return sb.ToString().TrimEnd();
    }
}
