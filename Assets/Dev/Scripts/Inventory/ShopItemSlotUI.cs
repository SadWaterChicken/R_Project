using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopItemSlotUI : MonoBehaviour
{
    public Image icon;
    public TMP_Text priceText;
    public Button button;

    private ItemData boundItem;
    private Action<ItemData> onBuy;

    public void Init(ItemData item, Action<ItemData> onBuyCallback)
    {
        boundItem = item;
        onBuy = onBuyCallback;

        if (icon != null)
        {
            icon.sprite = string.IsNullOrEmpty(item.iconPath) ? null : Resources.Load<Sprite>(item.iconPath);
        }

        if (priceText != null)
            priceText.text = item.price.ToString();

        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => onBuy?.Invoke(boundItem));
        }
    }

    private void OnDestroy()
    {
        if (button != null) button.onClick.RemoveAllListeners();
    }
}
