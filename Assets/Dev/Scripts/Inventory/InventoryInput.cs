using UnityEngine;

public class InventoryInput : MonoBehaviour
{
    private InventoryUI ui;

    void Start()
    {
        // Prefer explicit inspector reference on Inventory singleton when set.
        ui = Inventory.Instance?.inventoryUIReference;
        if (ui != null) return;

        // Use new API if available, else fallback to the older API.
#if UNITY_2023_1_OR_NEWER
        ui = Object.FindFirstObjectByType<InventoryUI>();
#else
        ui = Object.FindObjectOfType<InventoryUI>();
#endif
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            ui?.Toggle();
        }
    }
}