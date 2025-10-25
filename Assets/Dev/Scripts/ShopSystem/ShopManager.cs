using UnityEngine;

public class ShopManager : MonoBehaviour
{
    public ShopUI shopUI;                // assign in Inspector (ShopPanel)
    public PlayerData playerData;        // assign PlayerData from Player GameObject

    private ShopDataJson currentShop;

    private void Awake()
    {
        if (shopUI == null) Debug.LogWarning("[ShopManager] shopUI not assigned.");
        if (playerData == null) Debug.LogWarning("[ShopManager] playerData not assigned.");
    }

    // Opens a shop from a TextAsset JSON (drag JSON TextAsset to inspector or assign via ShopTrigger)
    public void OpenShopFromJsonTextAsset(TextAsset textAsset)
    {
        if (textAsset == null) { Debug.LogWarning("Shop JSON is null"); return; }
        currentShop = JsonUtility.FromJson<ShopDataJson>(textAsset.text);
        OpenShop(currentShop);
    }

    // Open using already parsed object
    public void OpenShop(ShopDataJson shop)
    {
        currentShop = shop;
        if (shopUI == null)
        {
            Debug.LogWarning("ShopUI not assigned.");
            return;
        }
        shopUI.Init(this, playerData);
        shopUI.PopulateShop(shop);
        shopUI.gameObject.SetActive(true);

        // optional pause
        Time.timeScale = 0f;
    }

    public void CloseShop()
    {
        if (shopUI != null) shopUI.gameObject.SetActive(false);
        Time.timeScale = 1f;
    }

    // Called when Buy pressed on ShopUI
    public void BuyItem(ItemData item)
    {
        if (item == null) return;
        if (playerData == null) { Debug.LogError("PlayerData is null in ShopManager"); return; }

        // Use PlayerData.SpendGold (returns bool)
        if (!playerData.SpendGold(item.price))
        {
            Debug.Log("Not enough gold!");
            // OPTIONAL: show UI message "Not enough gold"
            return;
        }

        // Add to inventory (clone)
        var newItem = new ItemData(item.itemID, item.itemName, item.description, item.price, item.iconPath, 1);
        Inventory.Instance?.AddItem(newItem);

        // Update gold text on UI
        shopUI.UpdateGoldText(playerData.GetGold());
    }
}