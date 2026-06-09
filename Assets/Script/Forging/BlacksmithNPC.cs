using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// NPC Blacksmith that handles forging interaction
/// Place this on an NPC GameObject with collider and IInteractable implementation
/// </summary>
public class BlacksmithNPC : MonoBehaviour, IInteractable
{
    [Header("NPC Settings")]
    [SerializeField] private string npcName = "Blacksmith";
    [SerializeField] private string interactPrompt = "Press E to Forge";

    [Header("UI Reference")]
    [SerializeField] private ForgeUI forgeUI;

    [Header("Forging Data")]
    [SerializeField] private TextAsset forgingDataJson; // JSON file with recipes and materials

    public string GetInteractPrompt()
    {
        return interactPrompt;
    }

    public void Interact()
    {
        OpenForgeUI();
    }

    private void Start()
    {
    }

    private void OpenForgeUI()
    {
        if (forgeUI == null)
        {
            Debug.LogError("[BlacksmithNPC] ForgeUI not assigned!");
            return;
        }

        forgeUI.Init(npcName);
        Time.timeScale = 0f; // Pause game during forging
    }
}
