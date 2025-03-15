using UnityEngine;

public class PlayerCollisionHandler : MonoBehaviour
{
    // Reference to the interaction controller (should be on the same GameObject).
    public PlayerInteractionController interactionController;

    private void Awake()
    {
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
        // Check using layer or tag; here we assume objects to interact with are on the "Interactable" layer.
        if (other.gameObject.layer == LayerMask.NameToLayer("Interactable"))
        {
            // Set the current interactable object in the interaction controller.
            interactionController.SetCurrentInteractable(other.gameObject);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Interactable"))
        {
            // Only clear if the object leaving is the current interactable.
            if (interactionController.GetCurrentInteractable() == other.gameObject)
            {
                interactionController.SetCurrentInteractable(null);
            }
        }
    }
}
