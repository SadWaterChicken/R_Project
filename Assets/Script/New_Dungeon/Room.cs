using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(BoxCollider))]
[RequireComponent(typeof(Rigidbody))]
public class Room : MonoBehaviour
{
    public enum Directions
    {
        up,
        down,
        left,
        right
    }

    [System.Serializable]
    public class BossDefeatedEvent : UnityEvent<Room> { }

    [System.Serializable]
    public struct Doors
    {
        [HideInInspector]
        public bool active;

        public Directions direction;
        public Renderer doorRenderer;         // The 3D mesh to show/hide
        public Room leadsTo;
    }

    [SerializeField]
    public Renderer body;  // Changed from SpriteRenderer

    [SerializeField]
    public Renderer centerDec;  // Changed from SpriteRenderer

    private BoxCollider myCollider;

    public Doors[] roomDoors = new Doors[4];  // Now only 4 doors

    [HideInInspector]
    public bool collision;

    public int jumpsFromStart = -1;

    public bool isCleared = true;
    public bool isBossRoom = false;
    public bool isEventRoom = false;
    public System.Collections.Generic.List<GameObject> spawnedEnemies = new System.Collections.Generic.List<GameObject>();

    [HideInInspector]
    public bool hasSpawned = false;
    
    [HideInInspector]
    public bool generationComplete = false;

    public int roomID = -1;
    public bool isActiveRoom = false; // Tracks if the player is currently inside this room

    public System.Collections.Generic.List<Door> activeDoors = new System.Collections.Generic.List<Door>();

    public BossDefeatedEvent onBossDefeated = new BossDefeatedEvent();
    
    public event System.Action<Room> onPlayerEntered;

    // SetRoomActive: updates the room's active state and updates the global DungeonManager tracking
    public void SetRoomActive(bool state)
    {
        isActiveRoom = state;
        if (state && DungeonManager.Instance != null)
        {
            DungeonManager.Instance.currentActiveRoom = this;
        }
    }

    private void Awake()
    {
        myCollider = GetComponent<BoxCollider>();
        myCollider.isTrigger = true;
    }

    // CompleteEvent: publicly mark an event room as completed and clear it
    public void CompleteEvent()
    {
        if (!isCleared)
        {
            Debug.Log($"{gameObject.name} event completed.");
            ClearRoom();
        }
    }

    // RegisterSpawnedEnemy: track a spawned enemy instance for clear checks
    public void RegisterSpawnedEnemy(GameObject enemy)
    {
        if (enemy == null) return;

        if (!spawnedEnemies.Contains(enemy))
            spawnedEnemies.Add(enemy);

        isCleared = false;
    }

    // OnEnemyDied: called when a tracked enemy dies to update room state
    public void OnEnemyDied(GameObject enemy)
    {
        if (isCleared) return;

        if (enemy != null)
            spawnedEnemies.Remove(enemy);
        else
            spawnedEnemies.RemoveAll(item => item == null);

        if (spawnedEnemies.Count == 0)
            ClearRoom();
    }

    // ClearRoom: mark room cleared, unlock doors, and raise clear/boss events
    private void ClearRoom()
    {
        isCleared = true;
        Debug.Log($"{gameObject.name} cleared. Doors are now unlocked.");

        foreach (Door door in activeDoors)
        {
            door.SetLocked(false);
        }

        DungeonEvents.RaiseRoomCleared(this);

        if (isBossRoom)
        {
            Debug.Log("Boss defeated! Triggering reward and options.");
            DungeonEvents.RaiseBossDefeated(this);
        }
        else
        {
            // Optional: Give normal room reward here
        }
    }

    // OnTriggerEnter: detect player entry or generation-phase overlaps
    private void OnTriggerEnter(Collider col)  // Changed from OnTriggerEnter2D
    {
        if (!generationComplete)
        {
            // During generation phase: mark collision for overlap checking
            collision = true;
        }
        else if (col.CompareTag("Player") && !hasSpawned)
        {
            // Post-generation phase: trigger lazy spawn
            OnPlayerEnter();
        }
    }

    // OnPlayerEnter: invoked when player first enters to lock doors and raise entry events
    public void OnPlayerEnter()
    {
        if (hasSpawned) return;
        
        DungeonEvents.RaisePlayerEnteredRoom(this);
        onPlayerEntered?.Invoke(this);

        if (isCleared) return;

        foreach (Door door in activeDoors)
        {
            door.SetLocked(true);
        }
    }

    // AssignAllNeighbours: discover and open doors to neighbouring rooms using Virtual Grid O(1) lookup
    public void AssignAllNeighbours(RoomGenerator generator)
    {
        Vector2Int myGridPos = generator.WorldToGridPosition(transform.position);

        for (int i = 0; i < roomDoors.Length; i++)
        {
            Directions dir = roomDoors[i].direction;
            Vector2Int neighborGridOffset = Vector2Int.zero;
            
            switch (dir)
            {
                case Directions.up: neighborGridOffset = Vector2Int.up; break;
                case Directions.down: neighborGridOffset = Vector2Int.down; break;
                case Directions.left: neighborGridOffset = Vector2Int.left; break;
                case Directions.right: neighborGridOffset = Vector2Int.right; break;
            }

            Vector2Int expectedNeighborGridPos = myGridPos + neighborGridOffset;

            // Find if any room exists at expectedNeighborGridPos mathematically
            if (generator.roomGrid.TryGetValue(expectedNeighborGridPos, out Room hitRoomParent))
            {
                if (hitRoomParent != this)
                {
                    OpenDoor(i, hitRoomParent);
                    
                    // Open matching door on neighbor
                    int oppositeIndex = (int)GetOppositeDirection(dir);
                    
                    for (int k = 0; k < hitRoomParent.roomDoors.Length; k++)
                    {
                        if ((int)hitRoomParent.roomDoors[k].direction == oppositeIndex)
                        {
                            hitRoomParent.OpenDoor(k, this);
                            break;
                        }
                    }
                }
            }
        }
    }

    // OpenDoor: mark a door active and create its interactable representation
    private void OpenDoor(int i, Room neighbour)
    {
        if (roomDoors[i].active)
        {
            // If the door is already active (e.g., connected to a room that was destroyed and replaced by a Boss Room),
            // we update its connection instead of ignoring it.
            roomDoors[i].leadsTo = neighbour;
            
            foreach (Door door in activeDoors)
            {
                // Update the physical Door script if its old connected room was destroyed
                if (door.connectedRoom == null || door.connectedRoom.gameObject == null)
                {
                    door.connectedRoom = neighbour;
                }
            }
            return;
        }

        roomDoors[i].leadsTo = neighbour;
        roomDoors[i].active = true;
        
        if (roomDoors[i].doorRenderer != null)
            roomDoors[i].doorRenderer.enabled = true;
        else
            Debug.LogError($"{gameObject.name} - Door {i} renderer is NULL! Assign it in Inspector.");
        
        // Create an interactable Door GameObject at the door renderer's position
        CreateInteractableDoor(i, neighbour);
    }

    // CreateInteractableDoor: instantiate an interactable Door object for the mesh
    private void CreateInteractableDoor(int doorIndex, Room connectedRoom)
    {
        // Create a GameObject for the door interaction
        GameObject doorObj = new GameObject($"InteractableDoor_{doorIndex}_{(Directions)doorIndex}");
        
        // Position it at the door mesh's position
        if (roomDoors[doorIndex].doorRenderer != null)
            doorObj.transform.position = roomDoors[doorIndex].doorRenderer.transform.position;
        else
            doorObj.transform.position = transform.position;
        
        doorObj.transform.parent = transform;
        
        // Add Door script
        Door doorScript = doorObj.AddComponent<Door>();
        doorScript.SetConnection(this, connectedRoom);
        
        activeDoors.Add(doorScript);
    }

    // GetActiveDoorsAmount: count how many doors are active on this room
    public int GetActiveDoorsAmount()
    {
        int output = 0;
        foreach (Doors d in roomDoors)
            if (d.active)
                output++;
        return output;
    }

    // GetNeighbours: return a list of adjacent rooms connected by doors
    public System.Collections.Generic.List<Room> GetNeighbours()
    {
        System.Collections.Generic.List<Room> output = new System.Collections.Generic.List<Room>();
        foreach (Doors d in roomDoors)
            if (d.active)
                output.Add(d.leadsTo);
        return output;
    }

    // GetClosestToStartNeighbour: return neighbour closest to start by jumpsFromStart
    public Room GetClosestToStartNeighbour()
    {
        Room output = this;
        foreach (Doors d in roomDoors)
            if (d.active)
                if (output.jumpsFromStart >= d.leadsTo.jumpsFromStart)
                    output = d.leadsTo;
        return output;
    }

    // GetFurthestFromStartNeighbour: return neighbour furthest from start
    public Room GetFurthestFromStartNeighbour()
    {
        Room output = this;
        foreach (Doors d in roomDoors)
            if (d.active)
                if (output.jumpsFromStart < d.leadsTo.jumpsFromStart)
                    output = d.leadsTo;
        return output;
    }

    // IsCollidingForPooled: used during generation to detect overlapping rooms
    public bool IsCollidingForPooled(System.Collections.Generic.List<int> chunk, System.Collections.Generic.List<Room> rooms, Vector3 generatorPosition)  // Changed Vector2 to Vector3
    {
        bool roomsCollision = false;
        Vector3 me = transform.position;
        for (int i = chunk.Count - 1; i >= 0; i--)
        {
            Vector3 target = rooms[chunk[i]].transform.position;
            if (Mathf.Abs(target.z - me.z) > 0.01f
                || Mathf.Abs(target.x - me.x) > 0.01f) continue;
            if (rooms[chunk[i]] == this) continue;

            if ((me - target).sqrMagnitude < 0.2f)
            {
                roomsCollision = true;
                break;
            }
        }
        bool generatorCollision = (me - generatorPosition).sqrMagnitude < 0.01f;
        return roomsCollision || generatorCollision;
    }

    // GetOppositeDirection: helper to map a direction to its opposite
    public static Directions GetOppositeDirection(Directions d)
    {
        Directions output;
        switch (d)
        {
            case Directions.right:
                output = Directions.left;
                break;
            case Directions.left:
                output = Directions.right;
                break;
            case Directions.up:
                output = Directions.down;
                break;
            case Directions.down:
                output = Directions.up;
                break;
            default:
                output = Directions.up;
                break;
        }
        return output;
    }

    

}