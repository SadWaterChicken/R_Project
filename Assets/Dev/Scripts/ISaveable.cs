using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ISaveable
{
    // Unique id for this instance (assign in editor or generate at Awake)
    string SaveId { get; }

    // Short prefab/type key
    string PrefabKey { get; }

    // Capture snapshot of this entity
    EntitySnapshot CaptureState();

    // Restore snapshot
    void RestoreState(EntitySnapshot snapshot);
}
