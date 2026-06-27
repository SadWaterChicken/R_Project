using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopUI : MonoBehaviour
{
    [Header("Refs")]
    public TMP_Text shopTitle;
    public TMP_Text goldText;
    public Transform contentParent;          // grid content for shop
    public GameObject shopItemSlotPrefab;    // prefab ShopItemSlotUI
    public GameObject detailPanel;
    public Image detailIcon;
    public TMP_Text detailName;
    public TMP_Text detailDesc;
    public TMP_Text detailPrice;
    public Button buyButton;
    public Button sellButton;                // NEW: Sell button
    public Button closeButton;
    public Button closeDetailButton;         // Nút đóng riêng cho detailPanel

    // NEW: optional stats text (assign in Inspector). If null, stats will be appended to detailDesc.
    public TMP_Text detailStats;

    private ShopManager manager;
    private PlayerStat playerStat;
    private List<ItemData> currentItems;
    private List<GameObject> spawned = new List<GameObject>();
    private ItemData selectedItem;

    private void Awake()
    {
        gameObject.SetActive(false);
        if (detailPanel != null) detailPanel.SetActive(false);
    }

    // Initialize (called by ShopManager.OpenShop)
    public void Init(ShopManager mgr, PlayerStat ps)
    {
        manager = mgr;
        playerStat = ps;

        if (closeButton != null)
        {
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(() => manager.CloseShop());
        }

        if (closeDetailButton != null)
        {
            closeDetailButton.onClick.RemoveAllListeners();
            closeDetailButton.onClick.AddListener(() => {
                if (detailPanel != null) detailPanel.SetActive(false);
            });
        }
    }

    // Populate with shop data
    public void PopulateShop(ShopDataJson shop)
    {
        if (shop == null) return;
        shopTitle.text = shop.shopName;
        currentItems = shop.items;
        Refresh();

        // Always get the latest gold from PlayerStat (fixes manual gold changes in Inspector)
        int currentGold = playerStat != null ? playerStat.GetGold() : 0;
        UpdateGoldText(currentGold);

        if (detailPanel != null) detailPanel.SetActive(false);
        if (detailStats != null) detailStats.text = string.Empty;
    }

    public void Refresh()
    {
        foreach (var g in spawned) Destroy(g);
        spawned.Clear();

        if (currentItems == null) return;

        foreach (var item in currentItems)
        {
            GameObject slot = Instantiate(shopItemSlotPrefab, contentParent);
            spawned.Add(slot);
            var slotUI = slot.GetComponent<ShopItemSlotUI>();
            if (slotUI != null)
                slotUI.Init(item, ShowDetail);
        }
    }

    public void ShowDetail(ItemData item)
    {
        if (item == null) return;
        selectedItem = item;

        if (detailPanel != null) detailPanel.SetActive(true);
        if (detailIcon != null) 
        {
            detailIcon.sprite = string.IsNullOrEmpty(item.iconPath) ? null : Resources.Load<Sprite>(item.iconPath);
            detailIcon.color = detailIcon.sprite == null ? new Color(1, 1, 1, 0) : Color.white;
        }
        if (detailName != null) detailName.text = item.itemName;
        string descriptionText = ItemDescriptionFormatter.BuildDescription(item);
        if (detailDesc != null) detailDesc.text = descriptionText;
        if (detailPrice != null) detailPrice.text = $"Price: {item.price}";

        // NEW: show flexible equipment stats if present
        var statsText = ItemDescriptionFormatter.BuildStatsText(item);
        if (detailStats != null)
        {
            detailStats.text = statsText;
        }
        else if (!string.IsNullOrEmpty(statsText) && detailDesc != null)
        {
            detailDesc.text = descriptionText + "\n" + statsText;
        }

        if (buyButton != null)
        {
            buyButton.onClick.RemoveAllListeners();
            buyButton.onClick.AddListener(() => manager.BuyItem(item));
            buyButton.interactable = playerStat != null && playerStat.GetGold() >= item.price;
        }

        // NEW: Setup Sell button
        if (sellButton != null)
        {
            sellButton.onClick.RemoveAllListeners();
            sellButton.onClick.AddListener(() => manager.SellItem(item));

            // Sell button enabled only if player has this item in inventory
            bool hasItem = Inventory.Instance != null && 
                          Inventory.Instance.ownedItems.Exists(x => x.itemID == item.itemID);
            sellButton.interactable = hasItem;
        }
    }

    public void UpdateGoldText(int gold)
    {
        if (goldText != null) goldText.text = $"Gold: {gold}";
    }

    private static string BuildStatsText(ItemData item)
    {
        if (item.modifiers == null || item.modifiers.Count == 0) return string.Empty;
        var sb = new StringBuilder();

        // 1. In ra Dòng Chính trước (Màu Vàng / Cam)
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
                string sign = m.value >= 0 && m.percentValue >= 0 ? "+" : "";
                var val = m.percent ? $"{sign}{m.percentValue.ToString("0.##")}%" : $"{sign}{m.value}";
                sb.AppendLine($"  <color=#DDDDDD>• {m.stat}: {val}</color>");
            }
        }

        return sb.ToString().TrimEnd();
    }
}
