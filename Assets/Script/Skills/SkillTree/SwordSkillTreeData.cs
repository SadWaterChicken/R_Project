using System;
using System.IO;
using UnityEngine;

/// <summary>
/// Lưu trạng thái Sword Skill Tree — cây mới 2 nhánh:
///
///   ROOT: baseDamageUp (1 SP)
///     └── windSlash (1 SP)
///           ├── windCritDamageUp (1 SP)
///           │     └── fireBladeSlash (2 SP)
///           │           └── fireManaSave (1 SP)
///           │                 └── fireCritUp (1 SP)
///           │                       └── fireDamageUp (2 SP)
///           │                             └── fireBurnDuration (2 SP)
///           │                                   └── twinInferno (3 SP)
///           └── windManaSave (1 SP)
///                 └── windDamageUp (2 SP)
///                       └── windRangeUp (2 SP)
///                             └── windCooldownDown (2 SP)
///                                   └── tempestBlade (3 SP)
/// </summary>
[Serializable]
public class SwordSkillTreeData
{
    // ─── SKILL POINTS ─────────────────────────────────────────────────────────
    public int availableSkillPoints = 0;
    public int totalSpGranted       = 0;

    // ─── ROOT ─────────────────────────────────────────────────────────────────
    public bool baseDamageUp        = false;   // Root node (1 SP)

    // ══ Wind Slash branch ════════════════════════════════════════════════════
    public bool windSlashUnlocked   = false;   // (1 SP) — yêu cầu baseDamageUp
    public bool windCritDamageUp    = false;   // (1 SP) — yêu cầu windSlash
    public bool windManaSave        = false;   // (1 SP) — yêu cầu windSlash
    public bool windDamageUp        = false;   // (2 SP) — yêu cầu windManaSave
    public bool windRangeUp         = false;   // (2 SP) — yêu cầu windDamageUp
    public bool windCooldownDown    = false;   // (2 SP) — yêu cầu windRangeUp
    public bool tempestBladeUnlocked = false;  // (3 SP) — yêu cầu windCooldownDown

    // ══ Fire Blade Slash branch ══════════════════════════════════════════════
    public bool fireBladeSlashUnlocked = false; // (2 SP) — yêu cầu windCritDamageUp
    public bool fireManaSave           = false; // (1 SP) — yêu cầu fireBladeSlash
    public bool fireCritUp             = false; // (1 SP) — yêu cầu fireManaSave
    public bool fireDamageUp           = false; // (2 SP) — yêu cầu fireCritUp
    public bool fireBurnDuration       = false; // (2 SP) — yêu cầu fireDamageUp
    public bool twinInfernoUnlocked    = false; // (3 SP) — yêu cầu fireBurnDuration

    // ─── SAVE / LOAD ─────────────────────────────────────────────────────────
    public static string SavePath =>
        System.IO.Path.Combine(Application.persistentDataPath, "sword_skill_tree_save.json");

    public void Save()
    {
        File.WriteAllText(SavePath, JsonUtility.ToJson(this, true));
        Debug.Log("[SwordSkillTreeData] Saved → " + SavePath);
    }

    public static SwordSkillTreeData Load()
    {
        if (File.Exists(SavePath))
        {
            var data = JsonUtility.FromJson<SwordSkillTreeData>(File.ReadAllText(SavePath));
            if (data != null)
            {
                Debug.Log("[SwordSkillTreeData] Loaded from " + SavePath);
                return data;
            }
        }
        Debug.Log("[SwordSkillTreeData] No save — fresh start.");
        return new SwordSkillTreeData();
    }

    // ─── COST TABLE ──────────────────────────────────────────────────────────
    public static int GetNodeCost(string nodeID)
    {
        if (string.IsNullOrEmpty(nodeID)) return 999;
        switch (nodeID.Trim())
        {
            // Root
            case "baseDamageUp":      return 1;

            // WindSlash branch
            case "windSlash":         return 1;
            case "windCritDamageUp":  return 1;
            case "windManaSave":      return 1;
            case "windDamageUp":      return 2;
            case "windRangeUp":       return 2;
            case "windCooldownDown":  return 2;
            case "tempestBlade":      return 3;

            // FireBladeSlash branch
            case "fireBladeSlash":    return 2;
            case "fireManaSave":      return 1;
            case "fireCritUp":        return 1;
            case "fireDamageUp":      return 2;
            case "fireBurnDuration":  return 2;
            case "twinInferno":       return 3;

            default:
                Debug.LogWarning($"[SwordSkillTreeData] Unknown nodeID: {nodeID}");
                return 999;
        }
    }

    // ─── NODE DISPLAY INFO ────────────────────────────────────────────────────
    public static (string title, string desc, string iconName) GetNodeInfo(string nodeID)
    {
        if (string.IsNullOrEmpty(nodeID)) return ("", "", "skill_001");
        switch (nodeID.Trim())
        {
            case "baseDamageUp":      return ("Tăng Sát Thương", "+20% sát thương cơ bản cho tất cả kỹ năng Đại Kiếm.", "skill_001");
            case "windSlash":         return ("Wind Slash", "Mở khóa Wind Slash — phóng luồng gió sắc bén thẳng về phía trước (tầm 10m, đơn mục tiêu).", "skill_024");
            case "windCritDamageUp":  return ("Tăng Sát Thương Chí Mạng", "+30% sát thương chí mạng cho Wind Slash.", "skill_047");
            case "windManaSave":      return ("Tiết Kiệm Mana", "Giảm 30% mana tiêu thụ mỗi lần dùng Wind Slash.", "skill_015");
            case "windDamageUp":      return ("Tăng Sát Thương", "+40% sát thương Wind Slash.", "skill_010");
            case "windRangeUp":       return ("Tăng Tầm Đánh", "Wind Slash bắn xa hơn (10m → 15m).", "skill_003");
            case "windCooldownDown":  return ("Giảm Hồi Chiêu", "Wind Slash hồi chiêu nhanh hơn (5s → 3s).", "skill_012");
            case "tempestBlade":      return ("Tempest Blade (Max)", "Xuyên nhiều mục tiêu trên đường thẳng. +60% sát thương, +6m tầm, đổi tên Tempest Blade.", "skill_020");
            case "fireBladeSlash":    return ("Fire Blade Slash", "Mở khóa Fire Blade Slash — chém lửa, đánh nhiều mục tiêu và gây Burn 3s.", "skill_030");
            case "fireManaSave":      return ("Tiết Kiệm Mana", "Giảm 30% mana tiêu thụ mỗi lần dùng Fire Blade Slash.", "skill_015");
            case "fireCritUp":        return ("Tăng Tỉ Lệ Chí Mạng", "+20% tỉ lệ chí mạng cho Fire Blade Slash.", "skill_047");
            case "fireDamageUp":      return ("Tăng Sát Thương", "+40% sát thương Fire Blade Slash.", "skill_010");
            case "fireBurnDuration":  return ("Tăng Thời Gian Cháy", "Kẻ địch bị cháy lâu hơn (3s → 5s).", "skill_035");
            case "twinInferno":       return ("Twin Inferno (Max)", "Tung skill 2 lần, mở rộng phạm vi, tăng sát thương. Đổi tên Twin Inferno.", "skill_048");
            default:                  return (nodeID, "", "skill_001");
        }
    }
}
