using UnityEngine;

[DisallowMultipleComponent]
public class InventoryInput : MonoBehaviour
{
    private static InventoryInput s_Instance;

    [SerializeField] private InventoryUI ui;

    private void Awake()
    {
        // Ensure only one input handler toggles the inventory
        if (s_Instance != null && s_Instance != this)
        {
            enabled = false;
            return;
        }
        s_Instance = this;

        // Prefer explicit reference on Inventory singleton when set
        if (ui == null)
            ui = Inventory.Instance?.inventoryUIReference;

        // Otherwise find it even if the UI object starts inactive
#if UNITY_2023_1_OR_NEWER
        if (ui == null)
            ui = Object.FindAnyObjectByType<InventoryUI>();
#else
        if (ui == null)
            ui = FindObjectOfType<InventoryUI>(true);
#endif
    }

    private void OnDestroy()
    {
        if (s_Instance == this) s_Instance = null;
    }

    private void Update()
    {
        // Using KeyCode.I for Inventory toggle (consistent with PlayerController key bindings)
        if (Input.GetKeyDown(KeyCode.I))
        {
            if (ui == null)
            {
                Debug.LogWarning("[InventoryInput] InventoryUI reference not found.");
                return;
            }
            ui.Toggle();
        }
    }
}