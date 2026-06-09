using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ItemSlotUI : MonoBehaviour
{
    [Header("UI References")]
    public Image icon;
    public TextMeshProUGUI qtyText;
    public Button button;

    private ItemData currentItem;
    private int quantity;

    // Gán dữ liệu hiển thị
    public void SetItem(ItemData item, int qty)
    {
        currentItem = item;
        quantity = qty;

        if (icon == null)
        {
            Image[] imgs = GetComponentsInChildren<Image>(true);
            foreach(var img in imgs) { 
                if (img.gameObject != this.gameObject && img.gameObject.name.ToLower().Contains("icon")) { 
                    icon = img; break; 
                } 
            }
            if (icon == null) icon = GetComponent<Image>();
        }

        if (icon != null)
        {
            if (item != null && !string.IsNullOrEmpty(item.iconPath))
            {
                var sprite = Resources.Load<Sprite>(item.iconPath);
                icon.sprite = sprite;
                icon.color = sprite != null ? Color.white : new Color(1, 1, 1, 0);
            }
            else
            {
                icon.sprite = null;
                icon.color = new Color(1, 1, 1, 0);
            }
        }

        if (qtyText != null)
            qtyText.text = item != null && qty > 1 ? qty.ToString() : "";
    }

    // Xử lý khi click item trong inventory
    public void OnClick()
    {
        if (currentItem != null)
        {
            Debug.Log("Clicked item: " + currentItem.itemName);
            // Tùy logic game: mở tooltip, dùng item, v.v.
        }
    }

    private void Awake()
    {
        if (button != null)
            button.onClick.AddListener(OnClick);
    }
}