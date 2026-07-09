using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Gắn vào Slot 1 hoặc Slot 2 trong phần "Trang Bị Kỹ Năng".
/// Hiển thị skill đang được trang bị, click để chọn skill mới.
/// </summary>
public class SkillEquipSlotUI : MonoBehaviour
{
    [Header("Slot Config")]
    [Tooltip("1 = Slot 1 (Q), 2 = Slot 2 (E)")]
    public int slotIndex = 1;

    [Header("UI References")]
    public Image              skillIconImage;
    public TextMeshProUGUI    slotLabel;       // "Slot 1 (Q)" hoặc "Slot 2 (E)"
    public TextMeshProUGUI    skillNameText;
    public Button             selectButton;
    public Button             unequipButton;   // Nút tháo skill (đặt nhỏ gọn sát bên Slot)
    public GameObject         emptyIndicator;  // Hiện khi trống

    [Header("Selection Panel")]
    [Tooltip("Panel chứa danh sách skill có thể chọn")]
    public GameObject         selectionPanel;

    [Header("Skill List Container")]
    [Tooltip("ScrollRect content để spawn các skill button vào")]
    public Transform          skillListContent;

    [Tooltip("Prefab button 1 skill trong danh sách")]
    public GameObject         skillSelectButtonPrefab;

    private void Start()
    {
        if (slotLabel != null)
            slotLabel.text = slotIndex == 1 ? "Slot 1 (Q)" : "Slot 2 (E)";

        if (selectButton != null)
            selectButton.onClick.AddListener(OpenSelectionPanel);

        if (unequipButton != null)
            unequipButton.onClick.AddListener(() => EquipSkill(""));

        if (selectionPanel != null)
            selectionPanel.SetActive(false);

        Refresh();
    }

    /// <summary>Cập nhật hiển thị theo skill đang trang bị.</summary>
    public void Refresh()
    {
        if (PlayerSkillManager.Instance == null) return;

        string skillID = slotIndex == 1
            ? PlayerSkillManager.Instance.equippedSkillSlot1
            : PlayerSkillManager.Instance.equippedSkillSlot2;

        ActiveSkillData skill = PlayerSkillManager.Instance.GetSkillByID(skillID);

        bool hasSkill = skill != null;

        if (skillNameText != null)
            skillNameText.text = hasSkill ? skill.skillName : "";

        if (emptyIndicator != null)
            emptyIndicator.SetActive(!hasSkill);

        if (skillIconImage != null)
        {
            if (hasSkill)
            {
                // Ưu tiên lấy hình ảnh từ cây kỹ năng, sử dụng từ khoá để match cho chuẩn
                Sprite treeSprite = null;
                if (SkillTreeUIController.Instance != null)
                {
                    if (skillID.IndexOf("wind", System.StringComparison.OrdinalIgnoreCase) >= 0)
                        treeSprite = SkillTreeUIController.Instance.GetNodeSprite("windSlash");
                    else if (skillID.IndexOf("fire", System.StringComparison.OrdinalIgnoreCase) >= 0)
                        treeSprite = SkillTreeUIController.Instance.GetNodeSprite("fireBladeSlash");
                    else
                        treeSprite = SkillTreeUIController.Instance.GetNodeSprite(skillID);
                }

                skillIconImage.sprite = treeSprite != null ? treeSprite : skill.skillIcon;
                skillIconImage.gameObject.SetActive(true);
            }
            else
            {
                skillIconImage.gameObject.SetActive(false);
            }
        }

        // Hiện/ẩn nút tháo skill
        if (unequipButton != null)
            unequipButton.gameObject.SetActive(hasSkill);
    }

    private void OpenSelectionPanel()
    {
        if (selectionPanel == null) return;

        // Ẩn bảng Detail Panel đi để nhường chỗ cho bảng Chọn Skill
        if (SkillTreeUIController.Instance != null && SkillTreeUIController.Instance.detailPanel != null)
        {
            SkillTreeUIController.Instance.detailPanel.SetActive(false);
        }

        selectionPanel.SetActive(true);
        BuildSkillList();
    }

    /// <summary>Sinh ra danh sách button cho các skill đã unlock.</summary>
    private void BuildSkillList()
    {
        if (skillListContent == null || skillSelectButtonPrefab == null) return;

        // Xoá cũ
        foreach (Transform child in skillListContent)
            Destroy(child.gameObject);

        if (PlayerSkillManager.Instance == null) return;

        foreach (var skill in PlayerSkillManager.Instance.unlockedSkills)
        {
            if (skill == null) continue;

            // Chỉ hiện nếu đã unlock trong skill tree
            if (!IsSkillUnlockedInTree(skill)) continue;

            // Cập nhật lại hình ảnh của skill bằng với hình ảnh trên cây kỹ năng
            if (SkillTreeUIController.Instance != null)
            {
                Sprite treeSprite = null;
                if (skill.skillID.IndexOf("wind", System.StringComparison.OrdinalIgnoreCase) >= 0) 
                    treeSprite = SkillTreeUIController.Instance.GetNodeSprite("windSlash");
                else if (skill.skillID.IndexOf("fire", System.StringComparison.OrdinalIgnoreCase) >= 0) 
                    treeSprite = SkillTreeUIController.Instance.GetNodeSprite("fireBladeSlash");
                else 
                    treeSprite = SkillTreeUIController.Instance.GetNodeSprite(skill.skillID);

                if (treeSprite != null) skill.skillIcon = treeSprite;
            }

            var btnObj = Instantiate(skillSelectButtonPrefab, skillListContent);

            // Kiểm tra xem nút có gắn script giao diện mới không
            var listBtnUI = btnObj.GetComponent<SkillListButtonUI>();
            if (listBtnUI != null)
            {
                string capturedID = skill.skillID;
                listBtnUI.Setup(skill, () => EquipSkill(capturedID));
            }
            else
            {
                // Fallback cũ nếu người dùng chưa cập nhật Prefab
                var btnTxt = btnObj.GetComponentInChildren<TextMeshProUGUI>();
                if (btnTxt != null)
                    btnTxt.text = $"{skill.skillName} (Vũ khí: {skill.weaponClassRequirement})";

                var btn = btnObj.GetComponent<Button>();
                if (btn != null)
                {
                    string capturedID = skill.skillID;
                    btn.onClick.AddListener(() => EquipSkill(capturedID));
                }
            }
        }
    }

    private bool IsSkillUnlockedInTree(ActiveSkillData skill)
    {
        if (SwordSkillTreeManager.Instance == null) return true;

        bool isWind = skill.skillID.IndexOf("wind", System.StringComparison.OrdinalIgnoreCase) >= 0 ||
                      skill.name.IndexOf("wind", System.StringComparison.OrdinalIgnoreCase) >= 0;
        bool isFire = skill.skillID.IndexOf("fire", System.StringComparison.OrdinalIgnoreCase) >= 0 ||
                      skill.name.IndexOf("fire", System.StringComparison.OrdinalIgnoreCase) >= 0;

        if (isWind && !SwordSkillTreeManager.Instance.IsWindSlashUnlocked()) return false;
        if (isFire && !SwordSkillTreeManager.Instance.IsFireBladeSlashUnlocked()) return false;

        return true;
    }

    private void EquipSkill(string skillID)
    {
        if (PlayerSkillManager.Instance == null) return;

        if (slotIndex == 1) PlayerSkillManager.Instance.equippedSkillSlot1 = skillID;
        else                PlayerSkillManager.Instance.equippedSkillSlot2 = skillID;

        Refresh();

        if (selectionPanel != null)
            selectionPanel.SetActive(false);

        // Hiển thị info lên Detail Panel sau khi trang bị
        ActiveSkillData skill = PlayerSkillManager.Instance.GetSkillByID(skillID);
        if (skill != null && SkillTreeUIController.Instance != null)
            SkillTreeUIController.Instance.ShowEquippedSkillDetails(skill);
    }
}
