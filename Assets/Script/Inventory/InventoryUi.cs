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
    public Button useButton;
    public Button closeDetailButton;

    private List<GameObject> spawned = new List<GameObject>();
    private bool visible = false;

    // Guard: prevent multiple toggles in the same frame and debounce rapid presses
    private static int s_lastToggleFrame = -1;
    [SerializeField] private float toggleDebounce = 0.15f; // seconds, unscaled
    private float _nextToggleAllowedTime = 0f;

    private void Awake()
    {
        gameObject.SetActive(false);
        if (detailPanel != null) detailPanel.SetActive(false);
    }

    private void OnEnable()
    {
        if (Inventory.Instance != null)
            Inventory.Instance.OnInventoryChanged += Refresh;
    }

    private void OnDisable()
    {
        if (Inventory.Instance != null)
            Inventory.Instance.OnInventoryChanged -= Refresh;
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
    }

    public void ShowDetail(ItemData item)
    {
        if (item == null) return;

        if (detailPanel != null) detailPanel.SetActive(true);
        if (detailIcon != null) detailIcon.sprite = string.IsNullOrEmpty(item.iconPath) ? null : Resources.Load<Sprite>(item.iconPath);
        if (detailName != null) detailName.text = item.itemName;
        if (detailDesc != null) detailDesc.text = item.description;
        if (detailPrice != null) detailPrice.text = $"Price: {item.price}";
        if (detailQty != null) detailQty.text = $"Qty: {item.stack}";

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
        foreach (var m in item.modifiers)
        {
            var sign = m.value >= 0 ? "+" : "";
            var val = m.percent ? $"{sign}{m.value}%" : $"{sign}{m.value}";
            sb.AppendLine($"{m.stat}: {val}");
        }
        return sb.ToString().TrimEnd();
    }
}
