using UnityEngine;
using UnityEngine.UI;

public class Interactor : MonoBehaviour
{
    [Header("Interaction Settings")]
    public float interactRange = 3f;
    
    [Tooltip("The UI Canvas or Button GameObject attached to this object that should appear when the player is close.")]
    public GameObject hintUI; 

    private bool inRange = false;
    private IInteractable interactable;

    private void Awake()
    {
        interactable = GetComponent<IInteractable>();
        
        // Ensure the hint UI is hidden by default
        if (hintUI != null) 
        {
            hintUI.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            inRange = true;
            if (hintUI != null)
            {
                hintUI.SetActive(true);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            inRange = false;
            if (hintUI != null)
            {
                hintUI.SetActive(false);
            }
        }
    }

    private void Update()
    {
        // If the player is in the collider zone and presses F, trigger the interaction
        if (inRange && Input.GetKeyDown(KeyCode.F))
        {
            TriggerInteraction();
        }
    }

    // You can link your UI Button's OnClick event directly to this function in the inspector!
    public void TriggerInteraction()
    {
        if (interactable == null)
        {
            interactable = GetComponent<IInteractable>();
        }

        if (interactable != null)
        {
            interactable.Interact();
        }
    }
}
