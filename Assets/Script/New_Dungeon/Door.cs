using UnityEngine;

public class Door : MonoBehaviour, IInteractable
{
    public Room connectedRoom;
    public Room parentRoom;
    private Collider doorCollider;
    private GameObject currentPlayer = null;
    public KeyCode interactKey = KeyCode.F;
    public float interactionRange = 3f;  // Range to detect player

    public bool allowToUseDoor = true;
    public Renderer doorLockIndicator;

    private static float lastInteractTime = -1f;
    private const float interactCooldown = 0.2f;

    private void Start()
    {
        doorCollider = GetComponent<Collider>();
        if (doorCollider != null)
            doorCollider.isTrigger = true;
    }

    // Update: checks for player input/interaction range each frame
    private void Update()
    {
        // Find player in range
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);
            
            // If player is in range and presses the interact key
            if (distanceToPlayer <= interactionRange && Input.GetKeyDown(interactKey))
            {
                if (Time.time < lastInteractTime + interactCooldown) return;
                lastInteractTime = Time.time;

                currentPlayer = player;
                Interact();
            }
        }
    }

    // Interact: handles player interaction with the door and teleports player
    public void Interact()
    {
        if (connectedRoom == null)
        {
            Debug.LogWarning("Door has no connected room!");
            return;
        }

        if (!allowToUseDoor)
        {
            Debug.Log("Door is locked! Cannot pass.");
            // Optional: Give UI feedback that the doors are locked.
            return;
        }

        // Teleport player to the connected room
        if (currentPlayer != null)
        {
            // Try to find the door in the connected room that links back here, spawn player in front of it
            Vector3 teleportPos = connectedRoom.transform.position;
            foreach (var door in connectedRoom.roomDoors)
            {
                if (door.active && door.leadsTo == parentRoom && door.doorRenderer != null)
                {
                    // Calculate a position slightly in front of the destination door
                    Vector3 doorPos = door.doorRenderer.transform.position;
                    Vector3 directionToCenter = (connectedRoom.transform.position - doorPos).normalized;
                    teleportPos = doorPos + directionToCenter * 2f; 
                    teleportPos.y = currentPlayer.transform.position.y; // Keep player height consistent
                    break;
                }
            }

            currentPlayer.transform.position = teleportPos;
            Debug.Log($"Teleported to {connectedRoom.gameObject.name}");
            
            // Update active room states
            if (parentRoom != null) parentRoom.SetRoomActive(false);
            if (connectedRoom != null) connectedRoom.SetRoomActive(true);

            // Explicitly notify the room that the player has entered,
            // as Unity's OnTriggerEnter can be flaky when teleporting directly inside a trigger.
            connectedRoom.OnPlayerEnter();
        }
    }

    // SetConnection: record parent and connected room references for this door
    public void SetConnection(Room from, Room to)
    {
        parentRoom = from;
        connectedRoom = to;
    }

    // SetLocked: lock/unlock the door and update visual indicator
    public void SetLocked(bool locked)
    {
        allowToUseDoor = !locked;
        if (doorLockIndicator != null)
        {
            doorLockIndicator.enabled = locked;
        }
    }
}
