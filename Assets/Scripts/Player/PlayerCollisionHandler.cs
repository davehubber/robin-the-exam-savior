using UnityEngine;

public class PlayerCollisionHandler : MonoBehaviour
{
    // Reference to the movement controller (should be on the same GameObject).
    public MovementController movementController;
    // Reference to the interaction controller (should be on the same GameObject).
    public PlayerInteractionController interactionController;

    private void Awake()
    {
        if (movementController == null)
        {
            movementController = GetComponent<MovementController>();
            if (movementController == null)
            {
                Debug.LogError("PlayerCollisionHandler requires a MovementController component.");
            }
        }

        if (interactionController == null)
        {
            interactionController = GetComponent<PlayerInteractionController>();
            if (interactionController == null)
            {
                Debug.LogError("PlayerCollisionHandler requires a PlayerInteractionController component.");
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Handle trap interaction: activate slowdown when entering a SlowdownTrap.
        if (other.CompareTag("SlowdownTrap"))
        {
            movementController.SetSlowDown(true);
        }
        // Check using layer or tag; here we assume objects to interact with are on the "Interactable" layer.
        else if (other.gameObject.layer == LayerMask.NameToLayer("Interactable"))
        {
            // Set the current interactable object in the interaction controller.
            interactionController.SetCurrentInteractable(other.gameObject);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        // Reset slowdown when leaving the trap.
        if (other.CompareTag("SlowdownTrap"))
        {
            movementController.SetSlowDown(false);
        }
        else if (other.gameObject.layer == LayerMask.NameToLayer("Interactable"))
        {
            // Only clear if the object leaving is the current interactable.
            if (interactionController.GetCurrentInteractable() == other.gameObject)
            {
                interactionController.SetCurrentInteractable(null);
            }
        }
    }
}
