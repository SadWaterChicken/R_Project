using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class EquipSlotMap
{
    public EquipmentType type;
    public Image iconImage;
    public Button slotButton; // Nút bấm để tháo đồ (Unequip)
    
    [HideInInspector] 
    public ItemData currentItem;
}

public class EquipmentManagerUI : MonoBehaviour
{
    [Header("Kéo thả các ô trang bị tương ứng vào đây")]
    public List<EquipSlotMap> equipmentSlots = new List<EquipSlotMap>();

    private void Start()
    {
        // Lắng nghe sự kiện từ túi đồ
        if (Inventory.Instance != null)
        {
            Inventory.Instance.OnInventoryChanged += RefreshUI;
            Inventory.Instance.OnItemEquipChanged += (item, isEquipped) => RefreshUI();
        }

        // Gắn sự kiện bấm vào ô để tháo đồ
        foreach (var slot in equipmentSlots)
        {
            if (slot.slotButton != null)
            {
                EquipSlotMap currentSlot = slot; // Capture variable for closure
                slot.slotButton.onClick.AddListener(() => OnSlotClicked(currentSlot));
            }
        }

        RefreshUI();
    }

    private void OnDestroy()
    {
        if (Inventory.Instance != null)
        {
            Inventory.Instance.OnInventoryChanged -= RefreshUI;
        }
    }

    public void RefreshUI()
    {
        if (Inventory.Instance == null) return;

        // Reset toàn bộ các ô về trạng thái trống (trong suốt)
        foreach (var slot in equipmentSlots)
        {
            slot.currentItem = null;
            if (slot.iconImage != null)
            {
                slot.iconImage.sprite = null;
                slot.iconImage.color = new Color(1, 1, 1, 0); // Làm trong suốt
            }
        }

        // Quét kho đồ tìm các món đang mặc
        foreach (var item in Inventory.Instance.ownedItems)
        {
            if (item.equipped && item.equipmentType != EquipmentType.None)
            {
                // Tìm đúng cái ô trên UI có type tương ứng
                EquipSlotMap slot = equipmentSlots.Find(s => s.type == item.equipmentType);
                if (slot != null && slot.iconImage != null)
                {
                    slot.currentItem = item;
                    
                    Sprite itemSprite = Resources.Load<Sprite>(item.iconPath);
                    if (itemSprite != null)
                    {
                        slot.iconImage.sprite = itemSprite;
                        slot.iconImage.color = Color.white; // Hiện hình lên
                    }
                }
            }
        }
    }

    private void OnSlotClicked(EquipSlotMap slot)
    {
        // Nếu ô này đang có đồ, thì tháo nó ra
        if (slot.currentItem != null)
        {
            Inventory.Instance.ToggleEquip(slot.currentItem);
        }
    }
}
