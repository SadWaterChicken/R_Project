#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

/// <summary>
/// Editor utility to build a simple demo scene setup: Player with PlayerCombat, TrainingDummy, UI canvas wired.
/// Places objects at sensible positions and attempts to wire references.
/// </summary>
public static class CombatDemoBuilder
{
    [MenuItem("Tools/Combat/Create Combat Demo Scene")]
    public static void CreateDemo()
    {
        // Create Player
        GameObject player = new GameObject("Player");
        player.transform.position = Vector3.zero;
        var pd = player.AddComponent<PlayerData>();
        var pc = player.AddComponent<PlayerCombat>();
        var rb = player.AddComponent<Rigidbody2D>();
        var col = player.AddComponent<BoxCollider2D>();
        var anim = player.AddComponent<Animator>();

        // Create attack point
        GameObject attackPoint = new GameObject("AttackPoint");
        attackPoint.transform.SetParent(player.transform);
        attackPoint.transform.localPosition = new Vector3(1f, 0f, 0f);

        // Create Dummy
        GameObject dummy = new GameObject("TrainingDummy");
        dummy.transform.position = new Vector3(2f, 0f, 0f);
        var td = dummy.AddComponent<TrainingDummy>();
        var dcol = dummy.AddComponent<BoxCollider2D>();

        // Create Canvas & UI
        GameObject canvasGO = new GameObject("Canvas");
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasGO.AddComponent<UnityEngine.UI.CanvasScaler>();
        canvasGO.AddComponent<UnityEngine.UI.GraphicRaycaster>();

        GameObject hud = new GameObject("HUD");
        hud.transform.SetParent(canvasGO.transform);
        var ui = hud.AddComponent<PlayerUIController>();

        // Try to wire some references
        pc.gameObject.name = "Player";
        pc.transform.position = player.transform.position;
        // assign attackPoint if the script exposed it
        var attackPointTr = attackPoint.transform;
        var pcType = typeof(PlayerCombat);
        var field = pcType.GetField("attackPoint", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (field != null)
        {
            field.SetValue(pc, attackPointTr);
        }

        // Wire PlayerData to UI
        var pdField = typeof(PlayerUIController).GetField("playerData", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (pdField != null)
        {
            pdField.SetValue(ui, pd);
        }

        Selection.activeGameObject = player;
        Debug.Log("Combat demo created. Place animations and wire visuals as needed.");
    }
}
#endif