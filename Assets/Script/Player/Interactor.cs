using UnityEngine;

/// <summary>
/// DEPRECATED: Use PlayerController instead.
/// This script is kept for backward compatibility but is no longer the primary interaction handler.
/// 
/// PlayerController now handles all interaction detection and input processing.
/// All interactable objects should implement IInteractable interface.
/// 
/// To remove: Delete this script and ensure objects use IInteractable with PlayerController.
/// </summary>

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

        // If near an interactable and press F, interact
        if (Input.GetKeyDown(KeyCode.F) && nearbyInteractable != null)
        {
            nearbyInteractable.Interact();
        }
    }
}
