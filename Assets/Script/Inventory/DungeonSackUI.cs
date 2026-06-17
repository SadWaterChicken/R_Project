using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using TMP_Text = TMPro.TextMeshProUGUI;

public class DungeonSackUI : MonoBehaviour
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
    
    [Tooltip("Nút Use/Equip sẽ tự động bị vô hiệu hóa hoặc ẩn đi trong ngục")]
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
        if (DungeonSack.Instance != null)
            DungeonSack.Instance.OnSackChanged += Refresh;
    }

    private void OnDisable()
    {
        if (DungeonSack.Instance != null)
            DungeonSack.Instance.OnSackChanged -= Refresh;
    }

    public void Toggle()
    {
        if (s_lastToggleFrame == Time.frameCount) return;
        if (Time.unscaledTime < _nextToggleAllowedTime) return;

        s_lastToggleFrame = Time.frameCount;
        _nextToggleAllowedTime = Time.unscaledTime + toggleDebounce;

        visible = !visible;
        gameObject.SetActive(visible);
        if (visible) Refresh();
    }

    public void Refresh()
    {
        foreach (var go in spawned) Destroy(go);
        spawned.Clear();

        if (DungeonSack.Instance == null || itemSlotPrefab == null || contentParent == null) return;

        var items = DungeonSack.Instance.sackedItems;
        foreach (var item in items)
        {
            GameObject slotGO = Instantiate(itemSlotPrefab, contentParent);
            spawned.Add(slotGO);

            var shopSlot = slotGO.GetComponent<ShopItemSlotUI>();
            if (shopSlot != null)
            {
                shopSlot.Init(item, ShowDetail);
                continue;
            }

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
        }

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
            detailIcon.color = Color.white; // Luôn hiển thị cái ô trống (màu trắng) kể cả khi không có ảnh
        }
        if (detailName != null) detailName.text = item.itemName;
        if (detailDesc != null) detailDesc.text = item.description;
        if (detailPrice != null) detailPrice.text = $"Price: {item.price}";
        if (detailQty != null) detailQty.text = $"Qty: {item.stack}";

        var statsText = BuildStatsText(item);
        if (detailStats != null) detailStats.text = statsText;
        else if (detailDesc != null && !string.IsNullOrEmpty(statsText))
            detailDesc.text = item.description + "\n" + statsText;

        // Vô hiệu hóa nút Use trong DungeonSack
        if (useButton != null)
        {
            useButton.interactable = false;
            var label = useButton.GetComponentInChildren<TMP_Text>();
            if (label != null)
            {
                label.text = "Sacked (No Use)";
            }
        }

        if (closeDetailButton != null)
        {
            closeDetailButton.onClick.RemoveAllListeners();
            closeDetailButton.onClick.AddListener(() => detailPanel.SetActive(false));
        }
    }

    private static string BuildStatsText(ItemData item)
    {
        if (item.modifiers == null || item.modifiers.Count == 0) return "";
        var sb = new StringBuilder();

        foreach (var m in item.modifiers)
        {
            if (m.isMainStat)
            {
                var sign = m.value >= 0 ? "+" : "";
                var val = m.percent ? $"{sign}{m.value}%" : $"{sign}{m.value}";
                sb.AppendLine($"<color=#FFB300><b>{m.stat}: {val}</b></color>");
            }
        }

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
