using UnityEngine;
using UnityEditor;
using TMPro;
using UnityEngine.UI;

public class AddAdvancedWeaponUIPanel : MonoBehaviour
{
    [MenuItem("Tools/Add Advanced Weapons Panel")]
    public static void AddPanel()
    {
        string prefabPath = "Assets/Prefab/UI/Panel_Forge.prefab";
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (prefab == null)
        {
            Debug.LogError("Panel_Forge prefab not found!");
            return;
        }

        // Open prefab for editing
        GameObject instanceRoot = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
        
        ForgeUI ui = instanceRoot.GetComponent<ForgeUI>();
        if (ui == null)
        {
            Debug.LogError("ForgeUI not found on prefab!");
            DestroyImmediate(instanceRoot);
            return;
        }

        Transform window = FindChild(instanceRoot.transform, "Panel_Window");

        // Find existing Panel_WeaponList to duplicate
        Transform existingWeaponList = FindChild(window, "Panel_WeaponList");
        if (existingWeaponList == null)
        {
            Debug.LogError("Panel_WeaponList not found!");
            DestroyImmediate(instanceRoot);
            return;
        }

        // Check if already added
        Transform existingAdvanced = FindChild(window, "Panel_AdvancedWeapons");
        GameObject advancedPanel = null;

        if (existingAdvanced != null)
        {
            advancedPanel = existingAdvanced.gameObject;
        }
        else
        {
            // Duplicate
            advancedPanel = Instantiate(existingWeaponList.gameObject, window);
            advancedPanel.name = "Panel_AdvancedWeapons";

            // Move to center or slightly to the right to overlay
            RectTransform rect = advancedPanel.GetComponent<RectTransform>();
            rect.anchoredPosition = new Vector2(0, 0); // Center

            // Change title
            TMP_Text title = FindChild(advancedPanel.transform, "Text_Title")?.GetComponent<TMP_Text>();
            if (title != null) title.text = "Advanced Weapon Paths";

            // Add a Close button
            GameObject closeBtnObj = new GameObject("Button_ClosePanel");
            closeBtnObj.transform.SetParent(advancedPanel.transform, false);
            RectTransform btnRect = closeBtnObj.AddComponent<RectTransform>();
            btnRect.anchorMin = new Vector2(1, 1);
            btnRect.anchorMax = new Vector2(1, 1);
            btnRect.anchoredPosition = new Vector2(-20, -20);
            btnRect.sizeDelta = new Vector2(40, 40);
            
            Image btnImg = closeBtnObj.AddComponent<Image>();
            btnImg.color = Color.red; // simple red square for close
            
            Button btn = closeBtnObj.AddComponent<Button>();
            
            GameObject btnTextObj = new GameObject("Text");
            btnTextObj.transform.SetParent(closeBtnObj.transform, false);
            TMP_Text btnText = btnTextObj.AddComponent<TextMeshProUGUI>();
            btnText.text = "X";
            btnText.alignment = TextAlignmentOptions.Center;
            btnText.color = Color.white;
            btnText.fontSize = 24;
            btnText.GetComponent<RectTransform>().sizeDelta = new Vector2(40,40);

            // Hide by default
            advancedPanel.SetActive(false);
        }

        // Link references
        ui.advancedWeaponsPanel = advancedPanel;
        ui.advancedWeaponsContentParent = FindChild(advancedPanel.transform, "Content");
        ui.closeAdvancedWeaponsButton = FindChild(advancedPanel.transform, "Button_ClosePanel")?.GetComponent<Button>();

        // Apply back to prefab
        PrefabUtility.SaveAsPrefabAsset(instanceRoot, prefabPath);
        DestroyImmediate(instanceRoot);

        Debug.Log("<color=green><b>Successfully created Advanced Weapons Panel in Panel_Forge!</b></color>");
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
