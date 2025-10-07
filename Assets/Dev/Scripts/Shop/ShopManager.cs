using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance;

    [Header("References")]
    public Inventory playerInventory;
    public PlayerData playerData;
    public QuestManager questManager;
    public ShopUI shopUI;                 // assign ShopPanel's ShopUI component in inspector
    public MonoBehaviour[] disableWhileOpen;

    private ShopData currentShop;
    private List<MonoBehaviour> disabledComponents = new List<MonoBehaviour>();
    private float previousTimeScale = 1f;
    private bool isOpen = false;

    public bool IsOpen => isOpen;

    void Awake()
    {
        if (Instance == null) { Instance = this; } else Destroy(gameObject);
    }

    public void OpenShop(ShopData shop)
    {
        if (isOpen) return; // ignore if already open
        currentShop = shop;
        shopUI.Open(shop, this, playerData, playerInventory, questManager);
        isOpen = true;

        previousTimeScale = Time.timeScale;
        //if (pauseGame) Time.timeScale = 0f;

        disabledComponents.Clear();
        if (disableWhileOpen != null)
        {
            foreach (var c in disableWhileOpen)
            {
                if (c != null && c.enabled) { c.enabled = false; disabledComponents.Add(c); }
            }
        }
    }

    public void CloseShop()
    {
        if (!isOpen) return;
        shopUI.Close();
        isOpen = false;

        Time.timeScale = previousTimeScale;

        foreach (var c in disabledComponents)
        {
            if (c != null) c.enabled = true;
        }
        disabledComponents.Clear();
    }

    // Replace all direct assignments to playerData.Gold with the appropriate method call
    // Use playerData.SpendGold(int amount) for spending gold, and playerData.AddGold(int amount) for adding gold

    public bool BuyItem(ShopEntry entry, int qty = 1)
    {
        int price = (entry.priceOverride > 0 ? entry.priceOverride : entry.item.basePrice) * qty;
        if (playerData.Gold < price) { shopUI.ShowError("Not enough money"); return false; }
        if (!playerInventory.HasSpaceFor(entry.item, qty)) { shopUI.ShowError("No Space Left"); return false; }
        if (!playerData.SpendGold(price)) { shopUI.ShowError("Not enough money"); return false; }
        playerInventory.AddItem(entry.item, qty);
        if (!entry.unlimited) entry.stock = Mathf.Max(0, entry.stock - qty);
        shopUI.Refresh();
        return true;
    }

    public bool SellItem(ItemInstance inst, int qty = 1)
    {
        if (inst == null) return false;
        if (qty > inst.qty) qty = inst.qty;
        int gain = Mathf.CeilToInt(inst.template.basePrice * 0.5f) * qty;
        if (!playerInventory.RemoveItem(inst.template, qty)) return false;
        playerData.AddGold(gain);
        shopUI.Refresh();
        return true;
    }

    public bool Craft(RecipeSO recipe)
    {
        if (!questManager.HasCompleted(recipe.requiredQuestId)) { shopUI.ShowError("Chưa hoàn thành quest"); return false; }
        foreach (var ing in recipe.ingredients)
        {
            int have = playerInventory.items.FindAll(i => i.template == ing.item).Sum(i => i.qty);
            if (have < ing.amount) { shopUI.ShowError(" Not Enough" + ing.item.itemName); return false; }
        }
        if (playerData.Gold < recipe.goldCost) { shopUI.ShowError("Not enough money"); return false; }
        if (!playerData.SpendGold(recipe.goldCost)) { shopUI.ShowError("Not enough money"); return false; }
        foreach (var ing in recipe.ingredients) playerInventory.RemoveItem(ing.item, ing.amount);
        playerInventory.AddItem(recipe.result, 1);
        shopUI.Refresh();
        return true;
    }

    public bool UpgradePotion(ItemInstance potion)
    {
        int cost = 100 * (potion.level + 1);
        if (playerData.Gold < cost) { shopUI.ShowError("Not enough money"); return false; }
        if (!playerData.SpendGold(cost)) { shopUI.ShowError("Not enough money"); return false; }
        potion.level++;
        shopUI.Refresh();
        return true;
    }

    public bool SocketGem(ItemInstance weaponInstance, ItemSO gemItem)
    {
        if (weaponInstance == null || gemItem == null) return false;
        if (weaponInstance.template.socketCount <= weaponInstance.socketItemIds.Count) { shopUI.ShowError("Inventory Full"); return false; }
        if (!string.IsNullOrEmpty(currentShop.requiredQuestIdToEnableSocketing) &&
            !questManager.HasCompleted(currentShop.requiredQuestIdToEnableSocketing))
        {
            shopUI.ShowError("Chưa mở khảm");
            return false;
        }
        if (!playerInventory.RemoveItem(gemItem, 1)) { shopUI.ShowError("Empty Socket"); return false; }
        weaponInstance.socketItemIds.Add(gemItem.id);
        shopUI.Refresh();
        return true;
    }
}
