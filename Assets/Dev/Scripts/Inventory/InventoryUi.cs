using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryUI : MonoBehaviour
{
    [Header("References")]
    public GameObject itemSlotPrefab;     // prefab with ItemSlotUI
    public Transform contentParent;       // ScrollView Content transform (GridLayoutGroup)
    public GameObject detailPanel;
    public Image detailIcon;
    public TMP_Text detailName;
    public TMP_Text detailDesc;
    public TMP_Text detailPrice;
    public TMP_Text detailQty;
    public Button closeDetailButton;

    private List<GameObject> spawned = new List<GameObject>();
    private bool visible = false;

    private void Awake()
    {
        // start hidden
        gameObject.SetActive(false);
        if (detailPanel != null) detailPanel.SetActive(false);
    }

    private void OnEnable()
    {
        if (Inventory.Instance != null)
            Inventory.Instance.OnInventoryChanged += Refresh;
    }

    private void OnDisable()
    {
        if (Inventory.Instance != null)
            Inventory.Instance.OnInventoryChanged -= Refresh;
    }

    // Toggle inventory visibility
    public void Toggle()
    {
        visible = !visible;
        gameObject.SetActive(visible);
        if (visible) Refresh();
    }

    // Refresh grid from Inventory.Instance
    public void Refresh()
    {
        foreach (var go in spawned) Destroy(go);
        spawned.Clear();

        if (Inventory.Instance == null) return;

        var items = Inventory.Instance.ownedItems;
        foreach (var item in items)
        {
            GameObject slotGO = Instantiate(itemSlotPrefab, contentParent);
            spawned.Add(slotGO);
            var slot = slotGO.GetComponent<ShopItemSlotUI>();
            if (slot != null)
                slot.Init(item, ShowDetail);
        }
    }

    public void ShowDetail(ItemData item)
    {
        if (item == null) return;
        if (detailPanel != null) detailPanel.SetActive(true);

        if (detailIcon != null) detailIcon.sprite = string.IsNullOrEmpty(item.iconPath) ? null : Resources.Load<Sprite>(item.iconPath);
        if (detailName != null) detailName.text = item.itemName;
        if (detailDesc != null) detailDesc.text = item.description;
        if (detailPrice != null) detailPrice.text = $"Price: {item.price}";
        if (detailQty != null) detailQty.text = $"Qty: {item.stack}";

        if (closeDetailButton != null)
        {
            closeDetailButton.onClick.RemoveAllListeners();
            closeDetailButton.onClick.AddListener(() => detailPanel.SetActive(false));
        }
    }
}
