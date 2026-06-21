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
        SetInfo(material.icon, material.materialName, required, playerHas);
    }

    public void SetInfo(Sprite icon, string name, int required, int playerHas)
    {
        if (materialIcon != null)
        {
            materialIcon.sprite = icon;
            materialIcon.color = icon == null ? new Color(1,1,1,0) : Color.white;
        }

        if (materialNameText != null) materialNameText.text = name;

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
