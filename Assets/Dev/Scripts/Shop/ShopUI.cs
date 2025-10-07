using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ShopUI : MonoBehaviour
{
    [Header("UI refs")]
    public GameObject root;         // panel root (inactive by default)
    public TMP_Text shopNameText;
    public TMP_Text goldText;
    public Transform content;       // grid content
    public GameObject itemSlotPrefab;
    public GameObject itemDetailPanel;

    // detail refs
    public TMP_Text detailName;
    public Image detailIcon;
    public TMP_Text detailDesc;
    public TMP_Text detailPrice;
    public Button buyButton;
    public Button socketButton;
    public Button closeButton;

    // runtime
    ShopManager manager;
    PlayerData playerData;
    Inventory playerInventory;
    QuestManager questManager;
    ShopData currentShop;
    List<GameObject> spawnedSlots = new List<GameObject>();



    public void Open(ShopData shop, ShopManager mgr, PlayerData pd, Inventory inv, QuestManager qm)
    {
        root.SetActive(true);
        currentShop = shop;
        manager = mgr;
        playerData = pd;
        playerInventory = inv;
        questManager = qm;

        shopNameText.text = shop.shopName;
        closeButton.onClick.RemoveAllListeners();
        closeButton.onClick.AddListener(() => manager.CloseShop());

        Refresh();

        // set UI selection to first buy button for controller/keyboard navigation (if any)
        EventSystem.current?.SetSelectedGameObject(null);
        // optional: pick first spawned slot's button if exists after a frame (can use coroutine)
    }

    public void Close()
    {
        root.SetActive(false);
        // clear detail
        itemDetailPanel.SetActive(false);
    }

    public void Refresh()
    {
        goldText.text = $"Gold: {playerData.Gold}";
        foreach (var g in spawnedSlots) Destroy(g);
        spawnedSlots.Clear();

        foreach (var entry in currentShop.entries)
        {
            var go = Instantiate(itemSlotPrefab, content);
            var slot = go.GetComponent<ItemSlotUI>();
            slot.Setup(entry, manager, this);
            spawnedSlots.Add(go);
        }
    }

    public void ShowError(string msg)
    {
        Debug.LogWarning(msg);
        // optionally show on-screen popup
    }

    public void ShowDetail(ShopEntry entry)
    {
        detailName.text = entry.item.itemName;
        detailIcon.sprite = entry.item.icon;
        detailDesc.text = entry.item.description;
        int price = entry.priceOverride > 0 ? entry.priceOverride : entry.item.basePrice;
        detailPrice.text = $"Giá: {price}";

        // enable/disable buy button
        buyButton.interactable = (playerData.Gold >= price && playerInventory.HasSpaceFor(entry.item, 1));
        buyButton.onClick.RemoveAllListeners();
        buyButton.onClick.AddListener(() => manager.BuyItem(entry, 1));

        // Socket button logic: only if item type is Weapon AND quest completed (if required)
        if (socketButton != null)
        {
            bool hasQuestReq = !string.IsNullOrEmpty(currentShop.requiredQuestIdToEnableSocketing);
            bool questOk = !hasQuestReq || questManager.HasCompleted(currentShop.requiredQuestIdToEnableSocketing);
            socketButton.interactable = (entry.item.type == ItemType.Weapon && questOk);
            // optionally set tooltip text to explain why disabled
        }

        itemDetailPanel.SetActive(true);

        // focus buy button for keyboard/controller
        EventSystem.current?.SetSelectedGameObject(buyButton.gameObject);
    }
}
