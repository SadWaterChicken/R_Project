using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Gắn vào Prefab SkillSelectButton_Prefab trong danh sách chọn Skill.
/// Để tách biệt rõ ràng Tên, Yêu cầu và Icon, tránh bị đè chữ.
/// </summary>
public class SkillListButtonUI : MonoBehaviour
{
    public Image iconImage;
    public TextMeshProUGUI skillNameText;
    public TextMeshProUGUI reqText;
    public Button button;

    public void Setup(ActiveSkillData skill, UnityEngine.Events.UnityAction onClickAction)
    {
        if (skillNameText != null)
            skillNameText.text = skill.skillName;

        if (reqText != null)
            reqText.text = "Vũ khí: " + skill.weaponClassRequirement;
        
        if (iconImage != null)
        {
            if (skill.skillIcon != null)
            {
                iconImage.sprite = skill.skillIcon;
                iconImage.gameObject.SetActive(true);
            }
            else
            {
                iconImage.gameObject.SetActive(false);
            }
        }

        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(onClickAction);
        }
    }
}
