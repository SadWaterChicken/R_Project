using System;
using System.IO;
using UnityEngine;

/// <summary>
/// Lưu trạng thái Sword Skill Tree gồm 2 nhánh:
///   Nhánh 1 — WindSlash  (đơn mục tiêu → Tempest Blade AoE)
///   Nhánh 2 — FireBladeSlash (AoE từ đầu → Twin Inferno)
/// </summary>
[Serializable]
public class SwordSkillTreeData
{
    // ─── SKILL POINTS ─────────────────────────────────────────────────────────
    public int availableSkillPoints = 0;
    public int totalSpGranted       = 0;

    // ══════════════════════════════════════════════════════════════════════════
    // NHÁNH 1: WIND SLASH
    //   windSlash          (1 SP) — mở khóa, đơn mục tiêu
    //   windManaSave       (1 SP) — giảm mana
    //   windRangeUp        (2 SP) — tăng tầm 10m→15m
    //   windDamageUp       (2 SP) — tăng sát thương
    //   windCooldownDown   (2 SP) — giảm hồi chiêu 5s→3s
    //   tempestBlade       (3 SP) — AoE + tăng dame+tầm, đổi tên Tempest Blade
    // ══════════════════════════════════════════════════════════════════════════
    public bool windSlashUnlocked    = false;   // Node W1 (1 SP)
    public bool windManaSave         = false;   // Node W2 (1 SP)
    public bool windRangeUp          = false;   // Node W3 (2 SP)
    public bool windDamageUp         = false;   // Node W4 (2 SP)
    public bool windCooldownDown     = false;   // Node W5 (2 SP)
    public bool tempestBladeUnlocked = false;   // Node W6 (3 SP)

    // ══════════════════════════════════════════════════════════════════════════
    // NHÁNH 2: FIRE BLADE SLASH
    //   fireBladeSlash     (2 SP) — mở khóa, AoE (nhiều mục tiêu) từ đầu
    //   fireManaSave       (1 SP) — giảm mana
    //   fireCritUp         (1 SP) — tăng tỉ lệ chí mạng
    //   fireDamageUp       (2 SP) — tăng sát thương
    //   fireBurnDuration   (2 SP) — tăng thời gian cháy 3s→5s
    //   twinInferno        (3 SP) — tung 2 lần + tăng phạm vi, đổi tên Twin Inferno
    // ══════════════════════════════════════════════════════════════════════════
    public bool fireBladeSlashUnlocked = false; // Node F1 (2 SP)
    public bool fireManaSave           = false; // Node F2 (1 SP)
    public bool fireCritUp             = false; // Node F3 (1 SP)
    public bool fireDamageUp           = false; // Node F4 (2 SP)
    public bool fireBurnDuration       = false; // Node F5 (2 SP)
    public bool twinInfernoUnlocked    = false; // Node F6 (3 SP)

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
        switch (nodeID)
        {
            // WindSlash branch
            case "windSlash":         return 1;
            case "windManaSave":      return 1;
            case "windRangeUp":       return 2;
            case "windDamageUp":      return 2;
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
}
