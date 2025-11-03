using UnityEngine;

/// <summary>
/// Simple controller to show/hide the interact prompt by controlling CanvasGroup alpha.
/// Keeps the GameObject active so it stays fixed at the SavePoint position.
/// </summary>
[RequireComponent(typeof(CanvasGroup))]
public class InteractPromptController : MonoBehaviour
{
    private CanvasGroup cg;

    private void Awake()
    {
        cg = GetComponent<CanvasGroup>();
        if (cg == null) cg = gameObject.AddComponent<CanvasGroup>();
        
        // Ensure the GameObject is active so it remains visible in the world
        if (!gameObject.activeSelf) gameObject.SetActive(true);
        HideImmediate();
    }

    public void Show()
    {
        if (cg == null) cg = GetComponent<CanvasGroup>();
        if (cg == null) return;
        
        cg.alpha = 1f;
        cg.interactable = true;
        cg.blocksRaycasts = true;
    }

    public void Hide()
    {
        if (cg == null) cg = GetComponent<CanvasGroup>();
        if (cg == null) return;
        
        cg.alpha = 0f;
        cg.interactable = false;
        cg.blocksRaycasts = false;
    }

    public void ShowImmediate()
    {
        Show();
    }

    public void HideImmediate()
    {
        Hide();
    }
}
