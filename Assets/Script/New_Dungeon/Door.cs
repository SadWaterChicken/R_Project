using UnityEngine;

public class Door : MonoBehaviour, IInteractable
{
    public Room connectedRoom;
    public Room parentRoom;
    private Collider doorCollider;

    private void Start()
    {
        doorCollider = GetComponent<Collider>();
        if (doorCollider != null)
            doorCollider.isTrigger = true;
    }

    public void Interact()
    {
        if (connectedRoom == null)
        {
            Debug.LogWarning("Door has no connected room!");
            return;
        }

        // Teleport player to the connected room's center
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            player.transform.position = connectedRoom.transform.position;
            Debug.Log($"Teleported to {connectedRoom.gameObject.name}");
        }
    }

    public void SetConnection(Room from, Room to)
    {
        parentRoom = from;
        connectedRoom = to;
    }
}
