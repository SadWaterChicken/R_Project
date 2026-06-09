using UnityEngine;
using UnityEditor;
using TMPro;
using UnityEngine.UI;

public class FixForgeUI : EditorWindow
{
    [MenuItem("Tools/Fix Forge UI References")]
    public static void FixReferences()
    {
        
        // Find ForgeUI in the active scene instead of the prefab
        ForgeUI[] uis = Object.FindObjectsByType<ForgeUI>(FindObjectsSortMode.None);
        ForgeUI ui = null;
        foreach (var u in uis) {
            if (u.gameObject.name == "ShopPanel") {
                Debug.LogWarning("Found ForgeUI on ShopPanel! Destroying it to fix your mistake.");
                DestroyImmediate(u);
            } else if (u.gameObject.name.Contains("Forge")) {
                ui = u;
            }
        }
        if (ui == null)
        {
            Debug.LogError("Could not find any ForgeUI in the Scene!");
            return;
        }
        GameObject prefab = ui.gameObject;

        // Auto-assign references by searching children
        Undo.RecordObject(ui, "Fix Forge UI"); ui.forgePanel = FindChild(prefab.transform, "Panel_Window")?.gameObject;
        ui.npcNameText = FindChild(prefab.transform, "Text_Title")?.GetComponent<TMP_Text>();
        ui.closeButton = FindChild(prefab.transform, "Button_Close")?.GetComponent<Button>();

        Transform wList = FindChild(prefab.transform, "Panel_WeaponList"); ui.weaponListParent = FindChild(wList != null ? wList : prefab.transform, "Content");
        
        // Add VerticalLayoutGroup and ContentSizeFitter if missing
        if (ui.weaponListParent != null) {
            var vlg = ui.weaponListParent.gameObject.GetComponent<VerticalLayoutGroup>();
            if (vlg == null) {
                vlg = ui.weaponListParent.gameObject.AddComponent<VerticalLayoutGroup>();
                vlg.childControlHeight = false;
                vlg.childControlWidth = false;
                vlg.childForceExpandHeight = false;
                vlg.childForceExpandWidth = false;
                vlg.spacing = 10;
                vlg.padding = new RectOffset(10, 10, 10, 10);
            }
            var csf = ui.weaponListParent.gameObject.GetComponent<ContentSizeFitter>();
            if (csf == null) {
                csf = ui.weaponListParent.gameObject.AddComponent<ContentSizeFitter>();
                csf.verticalFit = ContentSizeFitter.FitMode.MinSize;
            }
        }
        
        // Load prefabs
        ui.weaponSlotPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefab/UI/WeaponSlot.prefab");
        ui.materialSlotPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefab/UI/ItemSlot.prefab"); // Assuming they use ItemSlot for materials

        ui.detailPanel = FindChild(prefab.transform, "Panel_Detail")?.gameObject;
        ui.weaponIcon = FindChild(prefab.transform, "Image_WeaponIcon")?.GetComponent<Image>();
        ui.weaponNameText = FindChild(prefab.transform, "Text_WeaponName")?.GetComponent<TMP_Text>();
        ui.weaponDescText = FindChild(prefab.transform, "Text_Description")?.GetComponent<TMP_Text>();
        
        // weaponClassText is missing in their UI, that's fine if it stays null
        ui.masteryProgressBar = FindChild(prefab.transform, "Slider_Mastery")?.GetComponent<Slider>();
        ui.masteryPercentText = FindChild(prefab.transform, "Text_MasteryPercent")?.GetComponent<TMP_Text>();
        ui.forgeeLevelText = FindChild(prefab.transform, "Text_ForgeLevel")?.GetComponent<TMP_Text>();

        ui.resultPreviewGroup = FindChild(prefab.transform, "Arrow")?.gameObject;
        ui.resultWeaponIcon = FindChild(prefab.transform, "Image_ResultIcon")?.GetComponent<Image>();
        ui.resultWeaponNameText = FindChild(prefab.transform, "Text_ResultName")?.GetComponent<TMP_Text>();

        ui.statsText = FindChild(prefab.transform, "Text_Stats")?.GetComponent<TMP_Text>();

        // We assume there's a second Content inside Panel_Requirements
        Transform reqPanel = FindChild(prefab.transform, "Panel_Requirements");
        if (reqPanel != null)
        {
            ui.materialsListParent = FindChild(reqPanel, "Content");
        }

        ui.goldRequiredText = FindChild(prefab.transform, "Text_GoldRequired")?.GetComponent<TMP_Text>();
        ui.weaponsNeededText = FindChild(prefab.transform, "Text_WeaponsNeeded")?.GetComponent<TMP_Text>();
        ui.forgeButton = FindChild(prefab.transform, "Button_Forge")?.GetComponent<Button>();

        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(prefab.scene); Debug.Log("weaponListParent is: " + ui.weaponListParent.name + ", parent: " + ui.weaponListParent.parent.name); EditorUtility.SetDirty(ui);
        AssetDatabase.SaveAssets();

        // Also fix WeaponSlot prefab
        GameObject weaponSlotPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefab/UI/WeaponSlot.prefab");
        if (weaponSlotPrefab != null) {
            WeaponSlotUI slotUI = weaponSlotPrefab.GetComponent<WeaponSlotUI>();
            if (slotUI != null) {
                slotUI.weaponNameText = weaponSlotPrefab.GetComponentInChildren<TMP_Text>();
                slotUI.weaponIcon = FindChild(weaponSlotPrefab.transform, "Image_Icon")?.GetComponent<Image>();
                slotUI.selectButton = weaponSlotPrefab.GetComponent<Button>();
                if (slotUI.selectButton == null) slotUI.selectButton = weaponSlotPrefab.GetComponentInChildren<Button>();
                EditorUtility.SetDirty(weaponSlotPrefab);
                Debug.Log("Fixed WeaponSlot.prefab references!");
            }
        }

        
        // Also fix ForgingSystem missing
        ForgingSystem fs = FindObjectOfType<ForgingSystem>();
        if (fs == null) {
            GameObject go = new GameObject("ForgingSystem");
            go.AddComponent<ForgingSystem>();
            Debug.Log("Created missing ForgingSystem object in scene.");
        }
        
        Debug.Log("<color=green><b>Successfully fixed and auto-assigned all references in ForgeUI!</b></color>");
    }

    private static Transform FindChild(Transform parent, string name)
    {
        if (parent.name == name) return parent;
        foreach (Transform child in parent)
        {
            Transform result = FindChild(child, name);
            if (result != null) return result;
        }
        return null;
    }
}
