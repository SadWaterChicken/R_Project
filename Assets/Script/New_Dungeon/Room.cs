using UnityEngine;

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

    private void Awake()
    {
        myCollider = GetComponent<BoxCollider>();
        myCollider.isTrigger = true;
    }

    private void OnTriggerEnter(Collider col)  // Changed from OnTriggerEnter2D
    {
        collision = true;
    }

    public void AssignAllNeighbours(Vector3[] offsets)
    {
        BoxCollider col = GetComponent<BoxCollider>();
        Vector3 roomSize = col.size / 2f;
        
        for (int i = 0; i < roomDoors.Length; i++)
        {
            int dir = (int)roomDoors[i].direction;
            Vector3 offset = offsets[dir].normalized;
            
            // Calculate raycast origin based on direction
            Vector3 rayOrigin = transform.position;
            
            if (dir == 0) rayOrigin += Vector3.forward * roomSize.z;
            else if (dir == 1) rayOrigin -= Vector3.forward * roomSize.z;
            else if (dir == 2) rayOrigin -= Vector3.right * roomSize.x;
            else if (dir == 3) rayOrigin += Vector3.right * roomSize.x;
            
            if (Physics.Raycast(rayOrigin, offset, out RaycastHit hit, RoomGenerator.prefabsDistance))
            {
                Room hitRoomParent = hit.collider.GetComponentInParent<Room>();
                
                if (hitRoomParent == this || hitRoomParent == null)
                    continue;
                
                OpenDoor(i, hitRoomParent);
                
                // Open matching door on neighbor
                int oppositeIndex = (int)GetOppositeDirection((Directions)dir);
                
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

    private void OpenDoor(int i, Room neighbour)
    {
        roomDoors[i].leadsTo = neighbour;
        roomDoors[i].active = true;
        
        if (roomDoors[i].doorRenderer != null)
            roomDoors[i].doorRenderer.enabled = true;
        else
            Debug.LogError($"{gameObject.name} - Door {i} renderer is NULL! Assign it in Inspector.");
        
        // Create an interactable Door GameObject at the door renderer's position
        CreateInteractableDoor(i, neighbour);
    }

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
    }

    public int GetActiveDoorsAmount()
    {
        int output = 0;
        foreach (Doors d in roomDoors)
            if (d.active)
                output++;
        return output;
    }

    public System.Collections.Generic.List<Room> GetNeighbours()
    {
        System.Collections.Generic.List<Room> output = new System.Collections.Generic.List<Room>();
        foreach (Doors d in roomDoors)
            if (d.active)
                output.Add(d.leadsTo);
        return output;
    }

    public Room GetClosestToStartNeighbour()
    {
        Room output = this;
        foreach (Doors d in roomDoors)
            if (d.active)
                if (output.jumpsFromStart >= d.leadsTo.jumpsFromStart)
                    output = d.leadsTo;
        return output;
    }

    public Room GetFurthestFromStartNeighbour()
    {
        Room output = this;
        foreach (Doors d in roomDoors)
            if (d.active)
                if (output.jumpsFromStart < d.leadsTo.jumpsFromStart)
                    output = d.leadsTo;
        return output;
    }

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