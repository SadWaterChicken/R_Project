using UnityEngine;

[DisallowMultipleComponent]
public class DungeonSackInput : MonoBehaviour
{
    private static DungeonSackInput s_Instance;

    [SerializeField] private DungeonSackUI ui;

    private void Awake()
    {
        // Ensure only one input handler toggles the dungeon sack
        if (s_Instance != null && s_Instance != this)
        {
            enabled = false;
            return;
        }
        s_Instance = this;

        // Prefer explicit reference on DungeonSack singleton when set
        if (ui == null)
            ui = DungeonSack.Instance?.dungeonSackUIReference;

        // Otherwise find it even if the UI object starts inactive
#if UNITY_2023_1_OR_NEWER
        if (ui == null)
            ui = Object.FindAnyObjectByType<DungeonSackUI>();
#else
        if (ui == null)
            ui = FindObjectOfType<DungeonSackUI>(true);
#endif
    }

    private void OnDestroy()
    {
        if (s_Instance == this) s_Instance = null;
    }

    private void Update()
    {
        // Using KeyCode.U for Dungeon Sack toggle (as requested)
        if (Input.GetKeyDown(KeyCode.U))
        {
            if (ui == null)
            {
                Debug.LogWarning("[DungeonSackInput] DungeonSackUI reference not found.");
                return;
            }
            ui.Toggle();
        }
    }
}
