using UnityEngine;
using System.Collections.Generic;

public class EventRoom : MonoBehaviour
{
    private Room room;
    private PlayerBuffManager playerBuffManager;
    
    [SerializeField] private List<DungeonBuff> possibleBuffs = new List<DungeonBuff>();
    [SerializeField] private bool useThemeBuffs = true;
    
    private bool eventTriggered = false;

    // Start: initialize room and player buff manager, subscribe to enter event
    private void Start()
    {
        room = GetComponent<Room>();
        if (room == null)
        {
            Debug.LogError("[EventRoom] No Room component found on event room!");
            return;
        }

        playerBuffManager = PlayerBuffManager.Instance;
        if (playerBuffManager == null)
        {
            Debug.LogError("[EventRoom] PlayerBuffManager not found!");
            return;
        }

        // If no buffs assigned and useThemeBuffs is true, try to get from theme
        if (possibleBuffs.Count == 0 && useThemeBuffs)
        {
            LoadBuffsFromTheme();
        }

        // Subscribe to player enter event
        room.onPlayerEntered += OnPlayerEnteredEvent;
    }

    // OnPlayerEnteredEvent: handler called when player enters this event room
    private void OnPlayerEnteredEvent(Room eventRoom)
    {
        if (eventTriggered || !room.isEventRoom)
            return;

        TriggerRandomBuff();
    }

    // TriggerRandomBuff: choose a buff at random and apply it to the player
    private void TriggerRandomBuff()
    {
        if (possibleBuffs.Count == 0)
        {
            Debug.LogWarning("[EventRoom] No buffs available for this event room!");
            room.CompleteEvent();
            return;
        }

        // Randomly choose a buff
        DungeonBuff chosenBuff = possibleBuffs[Random.Range(0, possibleBuffs.Count)];
        
        // Apply buff to player
        playerBuffManager.ApplyBuff(chosenBuff);

        // Show UI popup (placeholder for now)
        ShowBuffPopup(chosenBuff);

        eventTriggered = true;
        
        // Complete the event (unlock doors, mark as cleared)
        room.CompleteEvent();
    }

    // ShowBuffPopup: debug UI placeholder that logs the buff received
    private void ShowBuffPopup(DungeonBuff buff)
    {
        Debug.Log($"[EventRoom] ╔════════════════════════════════════════╗");
        Debug.Log($"[EventRoom] ║ ✨ BUFF RECEIVED ✨                    ║");
        Debug.Log($"[EventRoom] ║ {buff.buffName.PadRight(36)} ║");
        Debug.Log($"[EventRoom] ║ {buff.description.PadRight(36)} ║");
        Debug.Log($"[EventRoom] ║ Effect: +{(buff.value * 100).ToString("F0")}% {buff.type.ToString().PadRight(22)} ║");
        Debug.Log($"[EventRoom] ╚════════════════════════════════════════╝");

        // TODO: Instantiate proper UI popup prefab in world space or canvas
    }

    // LoadBuffsFromTheme: placeholder to populate possibleBuffs from theme
    private void LoadBuffsFromTheme()
    {
        // TODO: Implement when theme system is ready
        // For now, just log a warning
        Debug.LogWarning("[EventRoom] useThemeBuffs is true but theme loading not implemented yet!");
    }

    // OnDestroy: unsubscribe from room event when destroyed
    private void OnDestroy()
    {
        if (room != null)
        {
            room.onPlayerEntered -= OnPlayerEnteredEvent;
        }
    }

    // SetPossibleBuffs: replace this room's buff pool with given list
    public void SetPossibleBuffs(List<DungeonBuff> buffs)
    {
        possibleBuffs = new List<DungeonBuff>(buffs);
        Debug.Log($"[EventRoom] Buff pool updated with {possibleBuffs.Count} buffs");
    }
}
