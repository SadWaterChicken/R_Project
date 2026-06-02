using UnityEngine;
using System;

public class RoomEnemyTracker : MonoBehaviour
{
    private Room room;
    private Action<GameObject> releaseAction;
    private bool notified;

    public bool isSleeping = false;

    public void Initialize(Room targetRoom, Action<GameObject> onRelease)
    {
        room = targetRoom;
        releaseAction = onRelease;
        notified = false;
        isSleeping = false;
    }

    // Despawn: release or destroy the tracked enemy object
    public void Despawn()
    {
        if (releaseAction != null)
            releaseAction(gameObject);
        else
            Destroy(gameObject);
    }

    // OnDisable: treat disable as death notification when scene is loaded
    private void OnDisable()
    {
        if (!gameObject.scene.isLoaded) return; // Ignore on scene unload
        if (isSleeping) return; // Ignore if we intentionally put the boss to sleep for optimization
        NotifyDeath();
    }

    // OnDestroy: treat destroy as death notification when scene is loaded
    private void OnDestroy()
    {
        if (!gameObject.scene.isLoaded) return; // Ignore on scene unload
        NotifyDeath();
    }

    // NotifyDeath: inform room that this enemy died (once)
    private void NotifyDeath()
    {
        if (notified) return;
        notified = true;
        room?.OnEnemyDied(gameObject);
    }
}
