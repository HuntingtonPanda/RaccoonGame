using UnityEngine;
using UnityEngine.InputSystem;

public class InteractDetect : MonoBehaviour
{
    private IInteractable interactableInRange = null;
    public GameObject interactionIcon;
    
    void Start()
    {
        // Don't show the interaction icon at first
        interactionIcon.SetActive(false);
    }

    public void onInteract(InputAction.CallbackContext context)
    {
        // If there's an interactable in range, then we interact when pressing the input
        if (context.performed)
        {
            interactableInRange?.Interact();
            if (!interactableInRange.canInteract())
            {
                interactionIcon.SetActive(false);
            }
        }
    }

    // When entering collision, if there is an interactable object and we CAN interact with it, turn on the icon 
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out IInteractable interactable) && interactable.canInteract())
        {
            interactableInRange = interactable;
            interactionIcon.SetActive(true);
        }
    }

    // When exiting collision, if the interacted object was the same object that was in range, turn off the icon
    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out IInteractable interactable) && interactable == interactableInRange)
        {
            interactableInRange = null;
            interactionIcon.SetActive(false);
        }
    }
}
