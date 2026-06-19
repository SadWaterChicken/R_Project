using UnityEngine;
using TMPro;
using UnityEngine.UI; // Add this if not present

public class ShopManager : MonoBehaviour
{
    public ShopUI shopUI;
    public PlayerStat playerStat;

    private ShopDataJson currentShop;
    private ItemData currentDetailItem; // Track item đang được hiển thị

    // Add this field to fix CS0103
    [SerializeField] private Button useButton;

    private void Awake()
    {
        if (shopUI == null) Debug.LogWarning("[ShopManager] shopUI not assigned.");
        if (playerStat == null) playerStat = PlayerStat.Instance;
        if (playerStat == null) Debug.LogWarning("[ShopManager] playerStat not assigned and PlayerStat.Instance is null.");
    }

    public void OpenShopFromJsonTextAsset(TextAsset textAsset)
    {
        if (textAsset == null) { Debug.LogWarning("Shop JSON is null"); return; }
        currentShop = JsonUtility.FromJson<ShopDataJson>(textAsset.text);
        OpenShop(currentShop);
    }

    public void OpenShop(ShopDataJson shop)
    {
        currentShop = shop;
        if (shopUI == null) { Debug.LogWarning("ShopUI not assigned."); return; }

        // Ensure playerStat is set before initializing UI
        if (playerStat == null)
            playerStat = PlayerStat.Instance;

        if (playerStat == null)
        {
            Debug.LogError("[ShopManager] PlayerStat not found!");
            return;
        }

        shopUI.Init(this, playerStat);
        shopUI.PopulateShop(shop);
        shopUI.gameObject.SetActive(true);
    }

    public void CloseShop()
    {
        if (shopUI != null) shopUI.gameObject.SetActive(false);
        // Time.timeScale = 1f;  // ← XÓA DÒNG NÀY (HOẶC COMMENT)
    }

    public void BuyItem(ItemData item)
    {
        if (item == null) return;
        if (playerStat == null) { Debug.LogError("PlayerStat is null in ShopManager"); return; }

        if (!playerStat.SpendGold(item.price))
        {
            Debug.Log("Not enough gold!");
            return;
        }

        // Preserve modifiers when adding to inventory
        Inventory.Instance?.AddItem(item.Clone(1));

        shopUI.UpdateGoldText(playerStat.GetGold());
    }

    public void SellItem(ItemData item)
    {
        if (item == null) return;
        if (playerStat == null) { Debug.LogError("PlayerStat is null in ShopManager"); return; }
        if (Inventory.Instance == null) { Debug.LogError("Inventory is null"); return; }

        // Check if player has this item
        var inventoryItem = Inventory.Instance.ownedItems.Find(x => x.itemID == item.itemID);
        if (inventoryItem == null)
        {
            Debug.Log("Item not in inventory!");
            return;
        }

        // Remove 1 item from inventory
        Inventory.Instance.RemoveItem(inventoryItem, 1);

        // Add gold (sell for half price or configurable)
        int sellPrice = Mathf.RoundToInt(item.price * 0.5f); // 50% of buy price
        playerStat.AddGold(sellPrice);

        // Update UI
        shopUI.UpdateGoldText(playerStat.GetGold());
        Debug.Log($"Sold {item.itemName} for {sellPrice} gold");
    }

    private void UpdateUseButtonText(ItemData item)
    {
        if (useButton == null) return;

        var label = useButton.GetComponentInChildren<TMP_Text>();
        if (label != null)
        {
            label.text = item.equipped ? "Unequip" : "Use";
        }
    }

    private void OnUseButtonClicked(ItemData item)
    {
        if (item == null || Inventory.Instance == null) return;

        // Toggle equip
        Inventory.Instance.ToggleEquip(item);

        // ← QUAN TRỌNG: Update button text ngay lập tức
        UpdateUseButtonText(item);
    }

    // Refresh detail panel nếu đang mở
    // (This code block should be inside a method, not at class scope. If you need help with this, please clarify.)
    // if (detailPanel != null && detailPanel.activeSelf && currentDetailItem != null)
    // {
    //     var updatedItem = Inventory.Instance.ownedItems.Find(x => x.itemID == currentDetailItem.itemID);
    //     if (updatedItem != null)
    //     {
    //         ShowDetail(updatedItem);
    //     }
    // }
    // currentDetailItem = item; // Lưu item
    // useButton.onClick.AddListener(() => OnUseButtonClicked(item)); // Đổi listener
    // UpdateUseButtonText(item); // Update text
}
