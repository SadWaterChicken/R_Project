#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using TMPro;
using System.Reflection;

public static class SavePointPromptTools
{
    [MenuItem("Tools/SavePoint/Force Show All Interact Prompts (Editor)")]
    public static void ForceShowAllPrompts()
    {
    // Use FindObjectsByType for newer Unity versions (faster and not obsolete)
    var sps = UnityEngine.Object.FindObjectsByType<SavePoint>(FindObjectsSortMode.None);
        int fixedCount = 0;
        foreach (var sp in sps)
        {
            if (sp == null) continue;
            var prompt = sp.GetComponentInChildren<Canvas>(true)?.gameObject;
            if (prompt == null && sp != null)
            {
                // try common child name
                var tf = sp.transform.Find("InteractPrompt");
                if (tf != null) prompt = tf.gameObject;
            }

            if (prompt == null) continue;

            // Ensure active
            prompt.SetActive(true);

            // Canvas adjustments
            var canvas = prompt.GetComponentInChildren<Canvas>(true);
            if (canvas != null)
            {
                canvas.renderMode = RenderMode.WorldSpace;
                canvas.overrideSorting = true;
                canvas.sortingOrder = 500; // very high for debugging
                if (canvas.worldCamera == null && Camera.main != null)
                    canvas.worldCamera = Camera.main;
                canvas.transform.localScale = Vector3.one * 0.01f;
                var rt = canvas.GetComponent<RectTransform>();
                if (rt != null) rt.localPosition = new Vector3(0f, 1.2f, 0f);
            }

            // CanvasGroup / Alpha
            var cg = prompt.GetComponent<CanvasGroup>();
            if (cg == null) cg = prompt.AddComponent<CanvasGroup>();
            cg.alpha = 1f;
            cg.interactable = true;
            cg.blocksRaycasts = true;

            // TMP text adjustments
            var tmp = prompt.GetComponentInChildren<TextMeshProUGUI>(true);
            if (tmp != null)
            {
                tmp.color = Color.white;
                tmp.fontSize = 48;

                // Try to set the newer textWrappingMode enum if available, otherwise fall back to
                // setting the older enableWordWrapping via reflection to avoid compile-time API issues.
                var tmpType = tmp.GetType();
                var wrapProp = tmpType.GetProperty("textWrappingMode", BindingFlags.Public | BindingFlags.Instance);
                if (wrapProp != null)
                {
                    // find enum value named "NoWrap" on the property's type
                    var enumType = wrapProp.PropertyType;
                    object enumVal = null;
                    try
                    {
                        enumVal = System.Enum.Parse(enumType, "NoWrap");
                    }
                    catch
                    {
                        // ignore
                    }
                    if (enumVal != null) wrapProp.SetValue(tmp, enumVal);
                }
                else
                {
                    // older TMP: set enableWordWrapping via reflection (property might be obsolete in newer versions,
                    // but using reflection avoids compile-time reference)
                    var oldProp = tmpType.GetProperty("enableWordWrapping", BindingFlags.Public | BindingFlags.Instance);
                    if (oldProp != null && oldProp.CanWrite)
                    {
                        oldProp.SetValue(tmp, false);
                    }
                }
            }

            fixedCount++;
            EditorUtility.SetDirty(sp);
        }

        Debug.Log($"ForceShowAllPrompts: adjusted {fixedCount} SavePoint prompt(s) in the scene.");
    }
}
#endif
