using UnityEngine;

public class ShopManager : MonoBehaviour
{
    public ShopUI shopUI;
    public PlayerData playerData;

    private ShopDataJson currentShop;

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
        Time.timeScale = 0f;
    }

    public void CloseShop()
    {
        if (shopUI != null) shopUI.gameObject.SetActive(false);
        Time.timeScale = 1f;
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
}