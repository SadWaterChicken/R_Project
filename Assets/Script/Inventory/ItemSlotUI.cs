using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class ItemSlotUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    // Biến toàn cục để ghi nhớ món đồ đang được kéo
    public static ItemData DraggedItem;
    public static GameObject DragIcon;
    [Header("UI References")]
    public Image icon;
    public TextMeshProUGUI qtyText;
    public TextMeshProUGUI masteryText;
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

        if (masteryText != null)
        {
            if (item != null && item.equippable)
            {
                // Chỉ hiển thị số Mastery nếu vũ khí này đã được cày cuốc (Mastery > 0)
                masteryText.text = item.weaponMastery > 0 ? $"M.{item.weaponMastery:F0}" : "";
            }
            else
            {
                masteryText.text = "";
            }
        }
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

    // ==========================================
    // CƠ CHẾ KÉO THẢ (DRAG & DROP)
    // ==========================================
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (currentItem == null || !currentItem.equippable) return;
        
        // Dọn dẹp bóng ma cũ bị kẹt (nếu có)
        if (DragIcon != null) Destroy(DragIcon);

        DraggedItem = currentItem;

        // Tạo ra một bóng ma (Ghost Icon) bay lơ lửng theo chuột
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas != null)
        {
            DragIcon = new GameObject("DragIcon");
            DragIcon.transform.SetParent(canvas.transform, false);
            DragIcon.transform.SetAsLastSibling(); // Hiện lên trên cùng

            Image dragImage = DragIcon.AddComponent<Image>();
            dragImage.sprite = icon.sprite;
            dragImage.color = new Color(1f, 1f, 1f, 0.7f); // Hơi trong suốt
            dragImage.raycastTarget = false; // Quan trọng: Nếu không tắt sẽ chặn sự kiện thả (Drop)

            // Dùng kích thước thực của ô hiện tại
            RectTransform rt = DragIcon.GetComponent<RectTransform>();
            RectTransform myRt = icon.GetComponent<RectTransform>();
            rt.sizeDelta = myRt.rect.size;
            rt.position = Input.mousePosition;
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (DragIcon != null)
        {
            DragIcon.GetComponent<RectTransform>().position = Input.mousePosition;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        ClearDragState();
    }

    private void OnDisable()
    {
        // Nhỡ may UI bị Refresh (xóa đi vẽ lại) lúc đang kéo, phải hủy bóng ma ngay
        if (DraggedItem == currentItem)
        {
            ClearDragState();
        }
    }

    private void ClearDragState()
    {
        DraggedItem = null;
        if (DragIcon != null)
        {
            Destroy(DragIcon);
            DragIcon = null;
        }
    }
}
