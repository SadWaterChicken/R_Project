using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Toggle hiển thị giữa Overworld Minimap và Dungeon Map.
/// Là UI manager thuần — không chứa logic collider hay gameplay.
///
/// Gắn vào: HUD Canvas hoặc một Manager GameObject persistent.
/// </summary>
public class MapModeController : MonoBehaviour
{
    // -------------------------------------------------------------------------
    // Inspector
    // -------------------------------------------------------------------------
    [Header("UI Panels")]
    [Tooltip("Root panel chứa overworld minimap (Camera RenderTexture + icon)")]
    public GameObject overworldMinimapPanel;

    [Tooltip("Root panel chứa dungeon map (rooms, corridors)")]
    public GameObject dungeonMapPanel;

    [Header("Mode")]
    [Tooltip("Mode khởi đầu khi game load")]
    public MapMode initialMode = MapMode.Overworld;

    [Header("Events")]
    public UnityEvent onEnterOverworld;
    public UnityEvent onEnterDungeon;

    // -------------------------------------------------------------------------
    // Public enum
    // -------------------------------------------------------------------------
    public enum MapMode { Overworld, Dungeon }

    // -------------------------------------------------------------------------
    // Runtime
    // -------------------------------------------------------------------------
    public MapMode CurrentMode { get; private set; }

    // -------------------------------------------------------------------------
    // Unity
    // -------------------------------------------------------------------------
    void Start()
    {
        ApplyMode(initialMode, fireEvents: false);
    }

    // -------------------------------------------------------------------------
    // Public API
    // -------------------------------------------------------------------------

    /// <summary>Chuyển sang Dungeon map. Gọi từ DungeonEntrance.</summary>
    public void EnterDungeon()
    {
        if (CurrentMode == MapMode.Dungeon) return;
        ApplyMode(MapMode.Dungeon);
    }

    /// <summary>Chuyển về Overworld minimap. Gọi khi thoát dungeon.</summary>
    public void ExitDungeon()
    {
        if (CurrentMode == MapMode.Overworld) return;
        ApplyMode(MapMode.Overworld);
    }

    /// <summary>Toggle nhanh (có thể bind vào phím M).</summary>
    public void ToggleMode()
    {
        ApplyMode(CurrentMode == MapMode.Overworld ? MapMode.Dungeon : MapMode.Overworld);
    }

    /// <summary>Ẩn/hiện toàn bộ minimap HUD (bind vào phím H).</summary>
    public void SetMapVisible(bool visible)
    {
        overworldMinimapPanel?.SetActive(visible && CurrentMode == MapMode.Overworld);
        dungeonMapPanel?.SetActive(visible && CurrentMode == MapMode.Dungeon);
    }

    // -------------------------------------------------------------------------
    // Private
    // -------------------------------------------------------------------------
    void ApplyMode(MapMode mode, bool fireEvents = true)
    {
        CurrentMode = mode;

        bool inDungeon = mode == MapMode.Dungeon;

        if (overworldMinimapPanel) overworldMinimapPanel.SetActive(!inDungeon);
        if (dungeonMapPanel) dungeonMapPanel.SetActive(inDungeon);

        if (!fireEvents) return;

        if (inDungeon) onEnterDungeon?.Invoke();
        else onEnterOverworld?.Invoke();
    }
}
