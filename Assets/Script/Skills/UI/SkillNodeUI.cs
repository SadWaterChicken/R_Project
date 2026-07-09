using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

/// <summary>
/// Gắn vào mỗi Node prefab trong Skill Tree UI.
/// Hiển thị icon, tên, chi phí SP, trạng thái (mở/khoá/chờ).
/// Gọi SwordSkillTreeManager.Instance.UnlockNode khi click.
/// </summary>
public class SkillNodeUI : MonoBehaviour
{
    [Header("Node Data")]
    public string nodeID;                  // Phải khớp với ID trong SwordSkillTreeData

    [Header("UI References")]
    public Image      iconImage;           // Icon của node
    public Image      frameImage;          // Viền ngoài (đổi màu theo trạng thái)
    public Image      lockOverlay;         // Overlay tối (khi bị khoá)
    public TextMeshProUGUI nameText;       // Tên node
    public TextMeshProUGUI costText;       // Chi phí SP
    public Button     unlockButton;        // Nút bấm chọn node
    public GameObject checkmarkIcon;       // Dấu tích khi đã mở
    public GameObject highlightOutline;    // Viền sáng khi được chọn

    [Header("Colors")]
    public Color colorUnlocked  = new Color(0.2f, 0.8f, 0.3f);   // Xanh lá — đã mở
    public Color colorAvailable = new Color(1f,   0.8f, 0.1f);   // Vàng — có thể mở
    public Color colorLocked    = new Color(0.4f, 0.4f, 0.4f);   // Xám — bị khoá
    public Color colorFrame     = new Color(0.6f, 0.6f, 0.6f);   // Viền mặc định

    [Header("Skill Icon (optional override)")]
    public Sprite customIcon;              // Kéo icon từ 500FreeSkillIcons vào đây

    private void Start()
    {
        if (unlockButton != null)
            unlockButton.onClick.AddListener(OnClickSelect);

        SetSelected(false);
        Refresh();
    }

    /// <summary>Cập nhật hiển thị theo trạng thái hiện tại của node.</summary>
    public void Refresh()
    {
        if (SwordSkillTreeManager.Instance == null) return;

        bool unlocked  = SwordSkillTreeManager.Instance.IsNodeUnlocked(nodeID);
        bool prereqOk  = SwordSkillTreeManager.Instance.CheckPrereq(nodeID);
        bool canUnlock = SwordSkillTreeManager.Instance.CanUnlockNode(nodeID);
        int  cost      = SwordSkillTreeData.GetNodeCost(nodeID);

        var (title, desc, iconName) = SwordSkillTreeData.GetNodeInfo(nodeID);

        // Tên node
        if (nameText != null)
            nameText.text = title;

        // Chi phí
        if (costText != null)
            costText.text = unlocked ? "[Đã mở]" : $"{cost} SP";

        // Icon
        if (iconImage != null && customIcon != null)
            iconImage.sprite = customIcon;

        // Trạng thái màu sắc
        if (frameImage != null)
        {
            if      (unlocked)  frameImage.color = colorUnlocked;
            else if (canUnlock) frameImage.color = colorAvailable;
            else                frameImage.color = colorLocked;
        }

        // Lock overlay
        if (lockOverlay != null)
            lockOverlay.gameObject.SetActive(!unlocked && !prereqOk);

        // Checkmark
        if (checkmarkIcon != null)
            checkmarkIcon.SetActive(unlocked);

        // Nút bấm luôn tương tác được để chọn xem thông tin
        if (unlockButton != null)
            unlockButton.interactable = true;
    }

    public void SetSelected(bool isSelected)
    {
        if (highlightOutline != null)
            highlightOutline.SetActive(isSelected);
    }

    private void OnClickSelect()
    {
        if (SkillTreeUIController.Instance != null)
            SkillTreeUIController.Instance.SelectNode(this);
    }
}
