using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Giao diện cơ bản (dùng IMGUI) để chọn và trang bị Skill vào các Slot.
/// Tự động hiện lên khi người chơi đang mở Inventory và click vào một Vũ Khí.
/// </summary>
public class AutoSkillEquipUI : MonoBehaviour
{
    private InventoryUI inventoryUI;
    private Rect windowRect = new Rect(Screen.width - 320, 100, 300, 400);

    // State cho dropdown chọn skill
    private bool isSelectingSkill = false;
    private Vector2 scrollPos;

    private void Start()
    {
        inventoryUI = FindAnyObjectByType<InventoryUI>(FindObjectsInactive.Include);
    }

    private void OnGUI()
    {
        if (inventoryUI == null) return;
        if (PlayerSkillManager.Instance == null) return;

        // Chỉ hiện khi Inventory đang mở và có 1 item đang được chọn
        if (inventoryUI.IsVisible && inventoryUI.SelectedItem != null)
        {
            ItemData selected = inventoryUI.SelectedItem;
            // Nếu item đó là vũ khí, hiện bảng trang bị skill
            if (selected.equipmentType == EquipmentType.Weapon)
            {
                GUI.skin.window.fontSize = 14;
                GUI.skin.button.fontSize = 14;
                GUI.skin.label.fontSize = 14;

                windowRect = GUI.Window(999, windowRect, DrawSkillEquipWindow, "Gắn Kỹ Năng (Skill Slots)");
            }
            else
            {
                isSelectingSkill = false;
            }
        }
        else
        {
            isSelectingSkill = false;
        }
    }

    private void DrawSkillEquipWindow(int windowID)
    {
        GUILayout.Space(10);
        GUILayout.Label("Trang bị skill để dùng chung với vũ khí này:", AutoSkillEquipUIStyles.wordWrappedLabel);
        GUILayout.Space(10);

        ItemData selected = inventoryUI.SelectedItem;
        if (selected == null) return;

        if (!isSelectingSkill)
        {
            // Hiển thị 1 Slot duy nhất của vũ khí
            ActiveSkillData equipped = PlayerSkillManager.Instance.GetSkillByID(selected.equippedSkillID);
            string skillName = equipped != null ? equipped.skillName : "<Trống>";
            
            GUILayout.BeginHorizontal("box");
            GUILayout.Label("Kỹ năng gắn:", GUILayout.Width(100));
            if (GUILayout.Button(skillName, GUILayout.Height(40)))
            {
                isSelectingSkill = true;
            }
            GUILayout.EndHorizontal();
            GUILayout.Space(5);
        }
        else
        {
            // Màn hình chọn Skill từ Roster
            GUILayout.Label("Chọn Kỹ năng cho vũ khí:");
            if (GUILayout.Button("<< Quay lại", GUILayout.Height(30)))
            {
                isSelectingSkill = false;
            }

            GUILayout.Space(10);
            scrollPos = GUILayout.BeginScrollView(scrollPos);

            List<ActiveSkillData> roster = PlayerSkillManager.Instance.unlockedSkills;
            
            if (GUILayout.Button("--- THÁO SKILL ---", GUILayout.Height(40)))
            {
                selected.equippedSkillID = "";
                isSelectingSkill = false;
            }

            if (roster.Count == 0)
            {
                GUILayout.Label("Không có kỹ năng nào khả dụng!");
            }
            else
            {
                foreach (var skill in roster)
                {
                    if (skill == null) continue;

                    // FireBladeSlash: chỉ hiện sau khi unlock trong Skill Tree (2 SP)
                    bool isFireSkill = skill.skillID.IndexOf("fire", System.StringComparison.OrdinalIgnoreCase) >= 0 ||
                                       skill.name.IndexOf("fire", System.StringComparison.OrdinalIgnoreCase) >= 0;
                    if (isFireSkill && (SwordSkillTreeManager.Instance == null || !SwordSkillTreeManager.Instance.IsFireBladeSlashUnlocked()))
                        continue;

                    // WindSlash: luôn hiện sau khi unlock (1 SP) — check qua IsWindSlashUnlocked
                    bool isWindSkill = skill.skillID.IndexOf("wind", System.StringComparison.OrdinalIgnoreCase) >= 0 ||
                                       skill.name.IndexOf("wind", System.StringComparison.OrdinalIgnoreCase) >= 0;
                    if (isWindSkill && (SwordSkillTreeManager.Instance == null || !SwordSkillTreeManager.Instance.IsWindSlashUnlocked()))
                        continue;

                    // Kiểm tra weaponClassName có khớp không
                    bool isValidClass = true;
                    if (!string.IsNullOrEmpty(skill.weaponClassRequirement))
                    {
                        isValidClass = string.Equals(
                            selected.weaponClassName, skill.weaponClassRequirement,
                            System.StringComparison.OrdinalIgnoreCase);
                    }

                    GUI.enabled = isValidClass;
                    string btnText = $"{skill.skillName}\n<size=10>CD: {skill.cooldown}s  Mana: {skill.manaCost}</size>";
                    if (!isValidClass)
                        btnText += $"\n<color=red>Yêu cầu: {skill.weaponClassRequirement}</color>";

                    if (GUILayout.Button(btnText, GUILayout.Height(52)))
                    {
                        selected.equippedSkillID = skill.skillID;
                        isSelectingSkill = false;
                    }
                    GUI.enabled = true;
                }
            }

            GUILayout.EndScrollView();
        }

        GUI.DragWindow();
    }
}

// ─── EDITOR STYLES HELPER CHO IMGUI ──────────────────────────────────────────
public static class AutoSkillEquipUIStyles
{
    public static GUIStyle wordWrappedLabel
    {
        get
        {
            GUIStyle style = new GUIStyle(GUI.skin.label);
            style.wordWrap = true;
            return style;
        }
    }
    public static GUIStyle richtextLabel
    {
        get
        {
            GUIStyle style = new GUIStyle(GUI.skin.label);
            style.richText = true;
            return style;
        }
    }
}
