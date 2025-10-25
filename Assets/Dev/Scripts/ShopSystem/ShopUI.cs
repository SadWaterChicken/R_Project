using System.Collections.Generic;
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
    public Button closeButton;

    private ShopManager manager;
    private PlayerData playerData;
    private List<ItemData> currentItems;
    private List<GameObject> spawned = new List<GameObject>();
    private ItemData selectedItem;

    private void Awake()
    {
        gameObject.SetActive(false);
        if (detailPanel != null) detailPanel.SetActive(false);
    }

    // Initialize (called by ShopManager.OpenShop)
    public void Init(ShopManager mgr, PlayerData pd)
    {
        manager = mgr;
        playerData = pd;

        if (closeButton != null)
        {
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(() => manager.CloseShop());
        }
    }

    // Populate with shop data
    public void PopulateShop(ShopDataJson shop)
    {
        if (shop == null) return;
        shopTitle.text = shop.shopName;
        currentItems = shop.items;
        Refresh();

        UpdateGoldText(playerData?.GetGold() ?? 0);
        detailPanel.SetActive(false);
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
        if (detailIcon != null) detailIcon.sprite = string.IsNullOrEmpty(item.iconPath) ? null : Resources.Load<Sprite>(item.iconPath);
        if (detailName != null) detailName.text = item.itemName;
        if (detailDesc != null) detailDesc.text = item.description;
        if (detailPrice != null) detailPrice.text = $"Price: {item.price}";

        if (buyButton != null)
        {
            buyButton.onClick.RemoveAllListeners();
            buyButton.onClick.AddListener(() => manager.BuyItem(item));
            buyButton.interactable = playerData != null && playerData.GetGold() >= item.price;
        }
    }

    public void UpdateGoldText(int gold)
    {
        if (goldText != null) goldText.text = $"Gold: {gold}";
    }
}
