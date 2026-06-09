using UnityEngine;

/// <summary>
/// Forge Trigger - Place this on any NPC or GameObject (Anvil, Forge, etc.)
/// Add a Collider (Is Trigger = true) so the player can walk up and press F.
/// </summary>
public class ForgeTrigger : MonoBehaviour, IInteractable
{
    [Header("NPC / Object Settings")]
    [Tooltip("Tên hiển thị khi mở giao diện Lò Rèn (VD: Blacksmith, Anvil...)")]
    public string npcName = "Blacksmith";

    [Tooltip("Prompt hiển thị khi người chơi đến gần (VD: Press F to Forge)")]
    public string interactPrompt = "Press F to Forge";

    [Header("References")]
    [Tooltip("Kéo ForgeUI Panel vào đây")]
    public ForgeUI forgeUI;

    [Tooltip("Kéo file JSON công thức rèn vào đây (TextAsset)")]
    public TextAsset forgingDataJson;

    private void Start()
    {
        // Load forging recipes from JSON at the start
        if (ForgingSystem.Instance != null && forgingDataJson != null)
        {
            ForgingSystem.Instance.LoadRecipesFromJson(forgingDataJson);
        }
        else
        {
            if (ForgingSystem.Instance == null)
                Debug.LogWarning("[ForgeTrigger] ForgingSystem not found in scene. Add it to your GameManager.");
            if (forgingDataJson == null)
                Debug.LogWarning("[ForgeTrigger] No forging recipe JSON assigned.");
        }
    }

    // Called by the Interactor script when player presses F nearby
    public void Interact()
    {
        if (forgeUI == null)
        {
            Debug.LogError("[ForgeTrigger] ForgeUI is not assigned!");
            return;
        }

        forgeUI.Init(npcName);
    }

    public string GetInteractPrompt() => interactPrompt;

    private void OnDrawGizmosSelected()
    {
        // Draw a yellow sphere in the editor to show the trigger area
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 2f);
    }
}
