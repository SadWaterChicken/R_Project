using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HUDSkillCooldownUI : MonoBehaviour
{
    [Header("Slot 1 (Main Hand - Q)")]
    public Image slot1Icon;
    public Image slot1CooldownOverlay; // Lớp mờ đen đè lên icon khi đang hồi chiêu (Set Type: Filled, Radial 360)
    public TextMeshProUGUI slot1CooldownText;

    [Header("Slot 2 (Off Hand - E)")]
    public Image slot2Icon;
    public Image slot2CooldownOverlay;
    public TextMeshProUGUI slot2CooldownText;

    [Header("Visual Settings")]
    public Color normalColor = Color.white;
    public Color onCooldownColor = new Color(0.3f, 0.3f, 0.3f, 1f); // Màu tối xám khi hồi chiêu
    
    private void Update()
    {
        if (PlayerSkillCastController.Instance == null) return;

        UpdateSlot(
            PlayerSkillCastController.Instance.GetMainHandSkill(),
            PlayerSkillCastController.Instance.GetMainHandCooldownRemaining(),
            PlayerSkillCastController.Instance.GetMainHandCooldownTotal(),
            slot1Icon,
            slot1CooldownOverlay,
            slot1CooldownText
        );

        UpdateSlot(
            PlayerSkillCastController.Instance.GetOffHandSkill(),
            PlayerSkillCastController.Instance.GetOffHandCooldownRemaining(),
            PlayerSkillCastController.Instance.GetOffHandCooldownTotal(),
            slot2Icon,
            slot2CooldownOverlay,
            slot2CooldownText
        );
    }

    private void UpdateSlot(ActiveSkillData skill, float cooldownRemaining, float cooldownTotal, Image icon, Image overlay, TextMeshProUGUI text)
    {
        // 1. Không có skill đang trang bị
        if (skill == null)
        {
            if (icon != null) { icon.enabled = false; }
            if (overlay != null) { overlay.enabled = false; }
            if (text != null) { text.gameObject.SetActive(false); }
            return;
        }

        // 2. Hiển thị Icon skill
        if (icon != null)
        {
            icon.enabled = true;
            icon.sprite = skill.skillIcon;
        }

        // 3. Xử lý trạng thái Cooldown
        bool isOnCooldown = cooldownRemaining > 0f;

        if (isOnCooldown)
        {
            if (icon != null) icon.color = onCooldownColor;

            if (overlay != null)
            {
                overlay.enabled = true;
                // Nếu bạn set overlay Image là Filled -> Radial 360, nó sẽ quay vòng rất đẹp
                if (cooldownTotal > 0)
                    overlay.fillAmount = cooldownRemaining / cooldownTotal;
                else
                    overlay.fillAmount = 1f;
            }

            if (text != null)
            {
                text.gameObject.SetActive(true);
                text.text = Mathf.CeilToInt(cooldownRemaining).ToString(); // Làm tròn lên: 2.1s -> hiển thị 3
            }
        }
        else
        {
            // Hồi chiêu xong
            if (icon != null) icon.color = normalColor;
            if (overlay != null) overlay.enabled = false;
            if (text != null) text.gameObject.SetActive(false);
        }
    }
}
