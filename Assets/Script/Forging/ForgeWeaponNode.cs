using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Gắn vào UI Button đại diện cho 1 Vũ Khí trên cây rèn
/// </summary>
public class ForgeWeaponNode : MonoBehaviour
{
    [Header("Weapon Data")]
    [Tooltip("Kéo file ScriptableObject dữ liệu vũ khí (BaseItemData) vào đây")]
    public BaseItemData weaponData;

    [Header("UI References")]
    public Button nodeButton;
    public Image iconImage;

    private void OnValidate()
    {
        if (nodeButton == null) nodeButton = GetComponent<Button>();
        if (iconImage == null) iconImage = GetComponent<Image>();

        if (weaponData != null)
        {
            // Auto-load Icon
            if (iconImage != null && !string.IsNullOrEmpty(weaponData.iconPath))
            {
                var sp = Resources.Load<Sprite>(weaponData.iconPath);
                if (sp == null)
                {
                    var tex = Resources.Load<Texture2D>(weaponData.iconPath);
                    if (tex != null) sp = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
                }
                if (sp != null)
                {
                    iconImage.sprite = sp;
                    iconImage.color = Color.white;
                }
            }

            // Auto-update Text
            Transform txtChild = transform.Find("Text");
            if (txtChild != null)
            {
                var txt = txtChild.GetComponent<TMPro.TextMeshProUGUI>();
                if (txt != null) txt.text = string.IsNullOrEmpty(weaponData.itemName) ? weaponData.itemID : weaponData.itemName;
            }

            // Auto-rename GameObject
            gameObject.name = "Wpn_" + weaponData.itemID;
        }
    }
}
