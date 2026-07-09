using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Controller chính của màn hình Skill Tree (Canvas UI).
/// Gắn vào root GameObject của Canvas SkillTree.
///
/// Nhấn K để bật/tắt màn hình.
/// Tự động refresh toàn bộ node và thông tin khi mở.
/// </summary>
public class SkillTreeUIController : MonoBehaviour
{
    public static SkillTreeUIController Instance { get; private set; }

    [Header("Canvas Root")]
    [Tooltip("Panel gốc của toàn bộ Skill Tree UI — bật/tắt khi nhấn K")]
    public GameObject skillTreePanel;

    [Header("Mastery Info")]
    public TextMeshProUGUI masteryExpText;    // "EXP: 45.0 / 100"
    public Slider          masteryExpBar;
    public TextMeshProUGUI spAvailableText;  // "SP Khả Dụng: 3"

    [Header("All Skill Nodes")]
    [Tooltip("Kéo tất cả SkillNodeUI trong scene vào đây")]
    public List<SkillNodeUI> allNodes = new List<SkillNodeUI>();

    [Header("Equip Slots")]
    public SkillEquipSlotUI slot1UI;
    public SkillEquipSlotUI slot2UI;

    [Header("Detail Panel (Right Side)")]
    [Tooltip("Panel hiển thị thông tin chi tiết ở góc phải")]
    public GameObject         detailPanel;
    public Image              detailIcon;
    public TextMeshProUGUI    detailTitle;
    public TextMeshProUGUI    detailDesc;
    public TextMeshProUGUI    detailCost;
    public Button             detailUnlockBtn;

    [Header("Selection Panel (Right Side)")]
    [Tooltip("Panel chọn skill chung cho các Slot")]
    public GameObject         selectionPanel;

    private SkillNodeUI selectedNode;

    [Header("Debug / Reset")]
    public Button debugAddExpBtn;    // 1 nút debug duy nhất (thêm 10 EXP mỗi lần bấm)
    public Button resetSkillTreeBtn; // Nút reset cây kỹ năng về 0

    private bool isOpen = false;

    // ─── LIFECYCLE ────────────────────────────────────────────────────────────
    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        if (skillTreePanel != null)
            skillTreePanel.SetActive(false);

        if (detailPanel != null)
            detailPanel.SetActive(false);

        if (selectionPanel != null)
            selectionPanel.SetActive(false);

        if (detailUnlockBtn != null)
            detailUnlockBtn.onClick.AddListener(UnlockSelectedNode);

        // 1 nút debug duy nhất: mỗi lần bấm +10 EXP
        if (debugAddExpBtn != null)
            debugAddExpBtn.onClick.AddListener(() => AddDebugExp(10f));

        // Nút reset
        if (resetSkillTreeBtn != null)
            resetSkillTreeBtn.onClick.AddListener(ResetSkillTree);
    }

    private void OnEnable()
    {
        CursorManager.OnCloseAllUI += CloseSkillTree;
    }

    private void OnDisable()
    {
        CursorManager.OnCloseAllUI -= CloseSkillTree;
    }

    private void CloseSkillTree()
    {
        if (isOpen)
        {
            isOpen = false;
            if (skillTreePanel != null)
                skillTreePanel.SetActive(false);
            if (CursorManager.Instance != null)
                CursorManager.Instance.SetUIOpen(false);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
            Toggle();
    }

    // ─── PUBLIC API ───────────────────────────────────────────────────────────

    /// <summary>Bật/tắt màn hình.</summary>
    public void Toggle()
    {
        isOpen = !isOpen;
        if (skillTreePanel != null)
            skillTreePanel.SetActive(isOpen);

        if (CursorManager.Instance != null)
            CursorManager.Instance.SetUIOpen(isOpen);

        if (isOpen) RefreshAll();
    }

    /// <summary>Refresh toàn bộ UI — gọi sau khi unlock node hoặc thay đổi exp.</summary>
    public void RefreshAll()
    {
        RefreshMasteryBar();
        RefreshNodes();
        RefreshSlots();
    }

    /// <summary>Hiển thị chi tiết node vào panel bên phải khi click vào node.</summary>
    public void SelectNode(SkillNodeUI node)
    {
        if (node == null) return;

        // Bỏ highlight node cũ, thêm highlight node mới
        if (selectedNode != null)
            selectedNode.SetSelected(false);

        selectedNode = node;
        selectedNode.SetSelected(true);

        if (detailPanel == null) return;

        var (title, desc, _) = SwordSkillTreeData.GetNodeInfo(node.nodeID);
        int cost = SwordSkillTreeData.GetNodeCost(node.nodeID);
        bool unlocked = SwordSkillTreeManager.Instance != null && SwordSkillTreeManager.Instance.IsNodeUnlocked(node.nodeID);
        bool canUnlock = SwordSkillTreeManager.Instance != null && SwordSkillTreeManager.Instance.CanUnlockNode(node.nodeID);

        if (detailTitle != null) detailTitle.text = title;
        if (detailDesc  != null) detailDesc.text  = desc;
        if (detailCost  != null) detailCost.text  = unlocked ? "Đã mở khóa" : $"Chi phí: {cost} SP";

        if (detailIcon != null && node.iconImage != null)
            detailIcon.sprite = node.iconImage.sprite;

        if (detailUnlockBtn != null)
        {
            detailUnlockBtn.gameObject.SetActive(!unlocked);
            detailUnlockBtn.interactable = canUnlock;
        }

        if (selectionPanel != null)
            selectionPanel.SetActive(false);

        detailPanel.SetActive(true);
    }

    /// <summary>Hiển thị chi tiết skill đang trang bị khi click vào ô Slot.</summary>
    public void ShowEquippedSkillDetails(ActiveSkillData skill)
    {
        if (skill == null || detailPanel == null) return;

        // Bỏ highlight node nếu có
        if (selectedNode != null)
        {
            selectedNode.SetSelected(false);
            selectedNode = null;
        }

        if (detailTitle != null) detailTitle.text = skill.skillName;
        if (detailDesc  != null) detailDesc.text  = skill.description;
        if (detailCost  != null) detailCost.text  = $"Yêu cầu: {skill.weaponClassRequirement} | Năng lượng: {skill.manaCost}";
        
        if (detailIcon != null)
            detailIcon.sprite = skill.skillIcon;

        if (detailUnlockBtn != null)
            detailUnlockBtn.gameObject.SetActive(false);

        if (selectionPanel != null)
            selectionPanel.SetActive(false);

        detailPanel.SetActive(true);
    }

    /// <summary>Lấy icon của node tương ứng trên cây kỹ năng để dùng chung cho các chỗ khác.</summary>
    public Sprite GetNodeSprite(string skillID)
    {
        // Thường skillID và nodeID giống hệt nhau hoặc gần giống
        foreach (var node in allNodes)
        {
            if (node.nodeID.Equals(skillID, System.StringComparison.OrdinalIgnoreCase))
            {
                if (node.customIcon != null) return node.customIcon;
                if (node.iconImage != null) return node.iconImage.sprite;
            }
        }
        return null;
    }

    private void UnlockSelectedNode()
    {
        if (selectedNode == null || SwordSkillTreeManager.Instance == null) return;

        bool success = SwordSkillTreeManager.Instance.UnlockNode(selectedNode.nodeID);
        if (success)
        {
            RefreshAll();
            SelectNode(selectedNode); // Refresh detail panel cho node hiện tại
        }
    }

    // ─── PRIVATE REFRESH ─────────────────────────────────────────────────────

    private void RefreshMasteryBar()
    {
        if (SwordMasteryTracker.Instance == null) return;

        float exp    = SwordMasteryTracker.Instance.GetCurrentExp();
        float maxExp = SwordMasteryTracker.MAX_MASTERY_EXP;

        if (masteryExpText != null)
            masteryExpText.text = $"Mastery EXP: {exp:F1} / {maxExp:F0}";

        if (masteryExpBar != null)
        {
            masteryExpBar.minValue = 0f;
            masteryExpBar.maxValue = maxExp;
            masteryExpBar.value    = exp;
        }

        if (SwordSkillTreeManager.Instance != null)
        {
            int spAvail = SwordSkillTreeManager.Instance.GetAvailablePoints();

            if (spAvailableText != null)
                spAvailableText.text = $"SP Khả Dụng: {spAvail}";
        }
    }

    private void RefreshNodes()
    {
        foreach (var node in allNodes)
        {
            if (node != null) node.Refresh();
        }
    }

    private void RefreshSlots()
    {
        if (slot1UI != null) slot1UI.Refresh();
        if (slot2UI != null) slot2UI.Refresh();
    }

    private void AddDebugExp(float amount)
    {
        if (SwordMasteryTracker.Instance != null)
            SwordMasteryTracker.Instance.AddMasteryExp(amount);
        RefreshAll();
    }

    /// <summary>Reset toàn bộ cây kỹ năng, xoá sạch SP và EXP về 0.</summary>
    private void ResetSkillTree()
    {
        if (SwordSkillTreeManager.Instance == null) return;

        SwordSkillTreeManager.Instance.HardReset();

        // Bỏ highlight node đang chọn
        if (selectedNode != null)
        {
            selectedNode.SetSelected(false);
            selectedNode = null;
        }

        // Ẩn detail panel
        if (detailPanel != null)
            detailPanel.SetActive(false);

        RefreshAll();
        Debug.Log("[SkillTreeUIController] Đã reset toàn bộ cây kỹ năng.");
    }
}
