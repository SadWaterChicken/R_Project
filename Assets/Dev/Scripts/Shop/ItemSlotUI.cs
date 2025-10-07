using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class ItemSlotUI : MonoBehaviour, IPointerClickHandler
{
    public Image icon;
    public TMP_Text nameText;
    public TMP_Text priceText;
    public Button buyBtn;

    ShopEntry entry;
    ShopManager manager;
    ShopUI ui;

    public void Setup(ShopEntry e, ShopManager m, ShopUI u)
    {
        entry = e; manager = m; ui = u;
        icon.sprite = e.item.icon;
        nameText.text = e.item.itemName;
        int price = e.priceOverride > 0 ? e.priceOverride : e.item.basePrice;
        priceText.text = price.ToString();

        buyBtn.onClick.RemoveAllListeners();
        buyBtn.onClick.AddListener(() => manager.BuyItem(entry, 1));

        // enable/disable buy button depending on gold and inventory space
        buyBtn.interactable = (manager.playerData.Gold >= price && manager.playerInventory.HasSpaceFor(e.item, 1));
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        ui.ShowDetail(entry);
    }
}
