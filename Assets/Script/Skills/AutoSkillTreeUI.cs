using UnityEngine;

/// <summary>
/// Giao diện IMGUI cho Sword Skill Tree.
/// Nhánh 1: WindSlash → Tempest Blade
/// Nhánh 2: FireBladeSlash → Twin Inferno
/// Nhấn K để bật/tắt.
/// </summary>
public class AutoSkillTreeUI : MonoBehaviour
{
    private bool showUI = false;
    private Rect windowRect = new Rect(60, 50, 580, 680);
    private Vector2 scrollPos;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
            showUI = !showUI;
    }

    private void OnGUI()
    {
        if (!showUI) return;

        if (SwordSkillTreeManager.Instance == null || SwordMasteryTracker.Instance == null)
        {
            GUI.Label(new Rect(10, 10, 400, 30), "[Skill Tree] Đang khởi động...");
            return;
        }

        GUI.skin.window.fontSize = 16;
        GUI.skin.button.fontSize = 13;
        GUI.skin.label.fontSize  = 13;

        windowRect = GUI.Window(998, windowRect, DrawSkillTreeWindow, "⚔  Sword Skill Tree   [K để đóng]");
    }

    private void DrawSkillTreeWindow(int _)
    {
        SwordMasteryTracker   tracker = SwordMasteryTracker.Instance;
        SwordSkillTreeManager manager = SwordSkillTreeManager.Instance;
        SwordSkillTreeData    data    = manager.GetData();

        GUILayout.Space(8);

        // ─── THANH MASTERY ────────────────────────────────────────────────────
        GUILayout.BeginVertical("box");

        float exp     = tracker.GetCurrentExp();
        float maxExp  = SwordMasteryTracker.MAX_MASTERY_EXP;
        int   spAvail = manager.GetAvailablePoints();
        int   spTotal = SwordMasteryTracker.CalcTotalSPForExp(exp);
        int   spMax   = SwordMasteryTracker.TOTAL_MAX_SP;

        GUILayout.Label(
            $"<b>Sword Mastery EXP:</b>  {exp:F1} / {maxExp:F0}",
            AutoSkillEquipUIStyles.richtextLabel);

        // Progress bar
        Rect bg = GUILayoutUtility.GetRect(0, 16, GUILayout.ExpandWidth(true));
        bg.x += 4; bg.width -= 8;
        GUI.Box(bg, GUIContent.none);
        Rect fill = bg; fill.width = bg.width * (exp / maxExp);
        Color prev = GUI.color;
        GUI.color = new Color(0.25f, 0.75f, 1f);
        GUI.Box(fill, GUIContent.none);
        GUI.color = prev;

        GUILayout.Label(
            $"<b>SP khả dụng:</b>  <color=yellow>{spAvail}</color>   " +
            $"<color=grey>Tổng nhận: {spTotal}/{spMax} SP</color>",
            AutoSkillEquipUIStyles.richtextLabel);

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("▲ +5 EXP (Debug)",  GUILayout.Height(24))) tracker.AddMasteryExp(5f);
        if (GUILayout.Button("▲ +10 EXP (Debug)", GUILayout.Height(24))) tracker.AddMasteryExp(10f);
        if (GUILayout.Button("▲ +50 EXP (Debug)", GUILayout.Height(24))) tracker.AddMasteryExp(50f);
        GUILayout.EndHorizontal();

        GUILayout.EndVertical();

        GUILayout.Space(8);
        scrollPos = GUILayout.BeginScrollView(scrollPos);

        // ══════════════════════════════════════════════════════════════════════
        // NHÁNH 1: WIND SLASH
        // ══════════════════════════════════════════════════════════════════════
        GUILayout.Label("<size=15><b>💨  Nhánh 1 — Wind Slash</b></size>", AutoSkillEquipUIStyles.richtextLabel);

        DrawNode("windSlash",
            "Mở Khóa Wind Slash",
            "Đơn mục tiêu. Phóng luồng gió sắc bén thẳng về phía trước (tầm 10m).",
            1, data.windSlashUnlocked,
            prereq: true);

        DrawNode("windManaSave",
            "Tiết Kiệm Mana  (-30%)",
            "Giảm 30% mana tiêu thụ mỗi lần dùng Wind Slash.",
            1, data.windManaSave,
            prereq: data.windSlashUnlocked);

        DrawNode("windRangeUp",
            "Tăng Tầm  (10m → 15m)",
            "Luồng gió bắn xa hơn đáng kể.",
            2, data.windRangeUp,
            prereq: data.windManaSave);

        DrawNode("windDamageUp",
            "Tăng Sát Thương  (+40%)",
            "Gia cố sức mạnh của luồng gió.",
            2, data.windDamageUp,
            prereq: data.windRangeUp);

        DrawNode("windCooldownDown",
            "Giảm Hồi Chiêu  (5s → 3s)",
            "Rút ngắn thời gian chờ giữa các lần cast.",
            2, data.windCooldownDown,
            prereq: data.windDamageUp);

        DrawNode("tempestBlade",
            data.tempestBladeUnlocked ? "⭐ Tempest Blade" : "Tempest Blade",
            "Tăng sát thương, tăng tầm, từ đơn mục tiêu → AoE (xuyên nhiều mục tiêu trên đường thẳng). Đổi tên thành Tempest Blade.",
            3, data.tempestBladeUnlocked,
            prereq: data.windCooldownDown);

        GUILayout.Space(14);

        // ══════════════════════════════════════════════════════════════════════
        // NHÁNH 2: FIRE BLADE SLASH
        // ══════════════════════════════════════════════════════════════════════
        GUILayout.Label("<size=15><b>🔥  Nhánh 2 — Fire Blade Slash</b></size>", AutoSkillEquipUIStyles.richtextLabel);

        DrawNode("fireBladeSlash",
            "Mở Khóa Fire Blade Slash",
            "Tấn công nhiều mục tiêu trên đường thẳng từ đầu. Để lại vệt lửa gây sát thương theo thời gian.",
            2, data.fireBladeSlashUnlocked,
            prereq: true);

        DrawNode("fireManaSave",
            "Tiết Kiệm Mana  (-30%)",
            "Giảm 30% mana tiêu thụ mỗi lần dùng Fire Blade Slash.",
            1, data.fireManaSave,
            prereq: data.fireBladeSlashUnlocked);

        DrawNode("fireCritUp",
            "Tăng Chí Mạng  (+20%)",
            "Tăng tỉ lệ chí mạng thêm 20% cho mỗi hit của Fire Blade Slash.",
            1, data.fireCritUp,
            prereq: data.fireManaSave);

        DrawNode("fireDamageUp",
            "Tăng Sát Thương  (+40%)",
            "Gia cố sức mạnh của vệt lửa chém.",
            2, data.fireDamageUp,
            prereq: data.fireCritUp);

        DrawNode("fireBurnDuration",
            "Tăng Thời Gian Cháy  (3s → 5s)",
            "Kẻ địch bị dính cháy sẽ chịu DoT lâu hơn.",
            2, data.fireBurnDuration,
            prereq: data.fireDamageUp);

        DrawNode("twinInferno",
            data.twinInfernoUnlocked ? "⭐ Twin Inferno" : "Twin Inferno",
            "Tung ra skill 2 lần (lần 2 sau 0.35s), tăng phạm vi hitbox. Đổi tên thành Twin Inferno.",
            3, data.twinInfernoUnlocked,
            prereq: data.fireBurnDuration);

        GUILayout.EndScrollView();
        GUI.DragWindow();
    }

    /// <summary>Vẽ 1 node trong cây kỹ năng.</summary>
    private void DrawNode(string nodeID, string title, string desc, int cost, bool unlocked, bool prereq)
    {
        GUILayout.BeginVertical("box");

        string col   = unlocked ? "green" : (prereq ? "orange" : "red");
        string badge = unlocked ? "[ĐÃ MỞ]" : (prereq ? $"[{cost} SP]" : "[KHÓA]");

        GUILayout.Label($"<b>{title}</b>   <color={col}>{badge}</color>", AutoSkillEquipUIStyles.richtextLabel);
        GUILayout.Label(desc, AutoSkillEquipUIStyles.wordWrappedLabel);

        bool canClick = prereq && !unlocked && SwordSkillTreeManager.Instance.GetAvailablePoints() >= cost;
        GUI.enabled = canClick;
        if (GUILayout.Button($"Mở Khóa  (-{cost} SP)", GUILayout.Height(26)))
            SwordSkillTreeManager.Instance.UnlockNode(nodeID);
        GUI.enabled = true;

        GUILayout.EndVertical();
    }
}
