using UnityEngine;

public class Door : MonoBehaviour, IInteractable
{
    public Room connectedRoom;
    public Room parentRoom;
    private Collider doorCollider;
    private GameObject currentPlayer = null;
    public KeyCode interactKey = KeyCode.F;
    public float interactionRange = 3f;  // Range to detect player

    private void Start()
    {
        doorCollider = GetComponent<Collider>();
        if (doorCollider != null)
            doorCollider.isTrigger = true;
    }

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
                currentPlayer = player;
                Interact();
            }
        }
    }

    public void Interact()
    {
        if (connectedRoom == null)
        {
            Debug.LogWarning("Door has no connected room!");
            return;
        }

        // Teleport player to the connected room's center
        if (currentPlayer != null)
        {
            currentPlayer.transform.position = connectedRoom.transform.position;
            Debug.Log($"Teleported to {connectedRoom.gameObject.name}");
        }
    }

    public void SetConnection(Room from, Room to)
    {
        parentRoom = from;
        connectedRoom = to;
    }
}
