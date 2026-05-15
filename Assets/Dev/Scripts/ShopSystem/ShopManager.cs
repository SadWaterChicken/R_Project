using UnityEngine;
using TMPro;
using UnityEngine.UI; // Add this if not present

public class ShopManager : MonoBehaviour
{
    public ShopUI shopUI;
    public PlayerData playerData;

    private ShopDataJson currentShop;
    private ItemData currentDetailItem; // Track item đang được hiển thị

    // Add this field to fix CS0103
    [SerializeField] private Button useButton;

    private void Awake()
    {
        if (shopUI == null) Debug.LogWarning("[ShopManager] shopUI not assigned.");
        if (playerData == null) Debug.LogWarning("[ShopManager] playerData not assigned.");
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
        shopUI.Init(this, playerData);
        shopUI.PopulateShop(shop);
        shopUI.gameObject.SetActive(true);
        // Time.timeScale = 0f;  // ← XÓA DÒNG NÀY (HOẶC COMMENT)
    }

    public void CloseShop()
    {
        if (shopUI != null) shopUI.gameObject.SetActive(false);
        // Time.timeScale = 1f;  // ← XÓA DÒNG NÀY (HOẶC COMMENT)
    }

    public void BuyItem(ItemData item)
    {
        if (item == null) return;
        if (playerData == null) { Debug.LogError("PlayerData is null in ShopManager"); return; }

        if (!playerData.SpendGold(item.price))
        {
            Debug.Log("Not enough gold!");
            return;
        }

        // Preserve modifiers when adding to inventory
        Inventory.Instance?.AddItem(item.Clone(1));

        shopUI.UpdateGoldText(playerData.GetGold());
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