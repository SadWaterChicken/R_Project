using UnityEngine;

public class Interactor : MonoBehaviour
{
    public float InteractRange = 3f;
    private IInteractable nearbyInteractable;

    void Update()
    {
        // Check for nearby interactable objects
        Collider[] colliders = Physics.OverlapSphere(transform.position, InteractRange);
        nearbyInteractable = null;
        
        foreach (Collider collider in colliders)
        {
            if (collider.TryGetComponent(out IInteractable interactable))
            {
                nearbyInteractable = interactable;
                break;
            }
        }
        
        // If near an interactable and press E, interact
        if (Input.GetKeyDown(KeyCode.F) && nearbyInteractable != null)
        {
            nearbyInteractable.Interact();
        }
    }
}
