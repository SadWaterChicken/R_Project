using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// UI for material requirement slot
/// </summary>
public class MaterialSlotUI : MonoBehaviour
{
    public Image materialIcon;
    public TMP_Text materialNameText;
    public TMP_Text quantityText;

    public void SetMaterial(ForgingMaterial material, int required, int playerHas)
    {
        if (material == null) return;

        if (materialIcon != null)
        {
            materialIcon.sprite = string.IsNullOrEmpty(material.iconPath)
                ? null
                : Resources.Load<Sprite>(material.iconPath);
            materialIcon.color = materialIcon.sprite == null ? new Color(1,1,1,0) : Color.white;
        }

        if (materialNameText != null) materialNameText.text = material.materialName;

        if (quantityText != null)
        {
            // Color text based on if player has enough
            bool hasEnough = playerHas >= required;
            var color = hasEnough ? Color.green : Color.red;

            quantityText.text = $"{playerHas}/{required}";
            quantityText.color = color;
        }
    }
}
