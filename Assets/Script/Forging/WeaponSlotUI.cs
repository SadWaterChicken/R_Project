using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// UI for individual weapon slot in forge list
/// </summary>
public class WeaponSlotUI : MonoBehaviour
{
    public Image weaponIcon;
    public TMP_Text weaponNameText;
    public TMP_Text masteryText;
    public Button selectButton;
    public GameObject equippedBadge; // Assign the "Equipped" UI element here

    private ItemData weapon;

    public void SetWeapon(ItemData w, System.Action onSelect)
    {
        weapon = w;

        if (weaponIcon == null)
        {
            Image[] imgs = GetComponentsInChildren<Image>(true);
            foreach(var img in imgs) { 
                if (img.gameObject != this.gameObject && img.gameObject.name.ToLower().Contains("icon")) { 
                    weaponIcon = img; break; 
                } 
            }
            if (weaponIcon == null) weaponIcon = GetComponent<Image>();
        }

        if (weaponIcon != null)
        {
            weaponIcon.sprite = string.IsNullOrEmpty(w.iconPath)
                ? null
                : Resources.Load<Sprite>(w.iconPath);
            weaponIcon.color = weaponIcon.sprite == null ? new Color(1,1,1,0) : Color.white;
        }
        
        if (weaponNameText != null) weaponNameText.text = w.itemName;
        
        float currentMastery = 0f;
        if (Inventory.Instance != null && !string.IsNullOrEmpty(w.weaponClassName))
        {
            currentMastery = Inventory.Instance.GetClassMastery(w.weaponClassName);
        }
        if (masteryText != null) masteryText.text = $"Mastery: {currentMastery:F1}%";

        if (equippedBadge != null)
        {
            equippedBadge.SetActive(w.equipped);
        }

        if (selectButton != null)
        {
            selectButton.onClick.RemoveAllListeners();
            selectButton.onClick.AddListener(() => onSelect?.Invoke());
        }
    }

    public void SetInteractable(bool isInteractable)
    {
        if (selectButton != null)
        {
            selectButton.interactable = isInteractable;
        }
        
        if (weaponIcon != null && weaponIcon.sprite != null)
        {
            weaponIcon.color = isInteractable ? Color.white : new Color(0.3f, 0.3f, 0.3f, 1f);
        }

        if (weaponNameText != null)
        {
            weaponNameText.color = isInteractable ? Color.white : new Color(0.5f, 0.5f, 0.5f, 1f);
        }
    }
}
