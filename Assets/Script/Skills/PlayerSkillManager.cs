using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Quản lý danh sách kỹ năng mà người chơi đã mở khóa (Skill Roster) 
/// và các kỹ năng đang được trang bị vào các Slot (Skill Slots).
/// </summary>
public class PlayerSkillManager : MonoBehaviour
{
    public static PlayerSkillManager Instance { get; private set; }

    [Header("Available Skills")]
    public List<ActiveSkillData> unlockedSkills = new List<ActiveSkillData>();

    [Header("Equipped Skills")]
    public string equippedSkillSlot1 = "";
    public string equippedSkillSlot2 = "";

    private bool allSkillsLoaded = false;

    private void Start()
    {
        LoadAllSkillsFromResources();
    }

    private void LoadAllSkillsFromResources()
    {
        if (allSkillsLoaded) return;
        allSkillsLoaded = true;
        // Tự động load tất cả ActiveSkillData nằm trong thư mục Resources/Skills
        ActiveSkillData[] allSkills = Resources.LoadAll<ActiveSkillData>("Skills");
        foreach (var skill in allSkills)
        {
            if (!unlockedSkills.Contains(skill))
            {
                unlockedSkills.Add(skill);
                Debug.Log($"[PlayerSkillManager] Tải kỹ năng: {skill.name} | skillID='{skill.skillID}' | skillName='{skill.skillName}'");
            }
        }
        Debug.Log($"[PlayerSkillManager] Tổng cộng {unlockedSkills.Count} kỹ năng đã load.");
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    /// <summary>
    /// Lấy kỹ năng từ ID — tìm kiếm không phân biệt hoa thường.
    /// Hỗ trợ cả skillID ("wind_slash") lẫn tên file asset ("WindSlash").
    /// </summary>
    public ActiveSkillData GetSkillByID(string skillID)
    {
        if (string.IsNullOrEmpty(skillID)) return null;

        // 1. So sánh chính xác
        foreach (var skill in unlockedSkills)
        {
            if (skill != null && string.Equals(skill.skillID, skillID, System.StringComparison.OrdinalIgnoreCase))
                return skill;
        }

        // 2. So sánh theo tên asset (skill.name == tên file .asset)
        foreach (var skill in unlockedSkills)
        {
            if (skill != null && string.Equals(skill.name, skillID, System.StringComparison.OrdinalIgnoreCase))
                return skill;
        }

        // 3. Fallback: load từ Resources/Skills/<skillID>
        ActiveSkillData loaded = Resources.Load<ActiveSkillData>($"Skills/{skillID}");
        if (loaded != null)
        {
            if (!unlockedSkills.Contains(loaded)) unlockedSkills.Add(loaded);
            return loaded;
        }

        // 4. Fallback cuối: quét toàn bộ Resources/Skills
        if (allSkillsLoaded) return null;
        LoadAllSkillsFromResources();
        foreach (var skill in unlockedSkills)
        {
            if (skill != null && (
                string.Equals(skill.skillID, skillID, System.StringComparison.OrdinalIgnoreCase) ||
                string.Equals(skill.name,    skillID, System.StringComparison.OrdinalIgnoreCase)))
                return skill;
        }
        return null;
    }

    /// <summary>
    /// Mở khóa 1 kỹ năng để đưa vào danh sách có thể dùng.
    /// </summary>
    public void UnlockSkill(ActiveSkillData skill)
    {
        if (skill != null && !unlockedSkills.Contains(skill))
        {
            unlockedSkills.Add(skill);
            Debug.Log($"[PlayerSkillManager] Đã mở khóa kỹ năng: {skill.skillName}");
        }
    }
}
