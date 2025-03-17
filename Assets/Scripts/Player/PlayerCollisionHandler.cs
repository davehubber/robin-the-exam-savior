using UnityEngine;

public class PlayerCollisionHandler : MonoBehaviour
{
    public MovementController movementController;
    public PlayerInteractionController interactionController;

    private void Awake()
    {
        if (movementController == null)
            movementController = GetComponent<MovementController>();
            
        if (interactionController == null)
            interactionController = GetComponent<PlayerInteractionController>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("SlowdownTrap"))
        {
            movementController.SetSlowDown(true);
        }
        else if (other.gameObject.layer == LayerMask.NameToLayer("Interactable"))
        {
            interactionController.SetCurrentInteractable(other.gameObject);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("SlowdownTrap"))
        {
            movementController.SetSlowDown(false);
        }
        else if ((other.gameObject.layer == LayerMask.NameToLayer("Interactable")) && 
                 interactionController.GetCurrentInteractable() == other.gameObject)
        {
            interactionController.SetCurrentInteractable(null);
        }
    }
}