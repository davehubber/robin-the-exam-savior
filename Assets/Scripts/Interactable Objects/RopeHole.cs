using UnityEngine;

public class RopeHole : MonoBehaviour
{
    public float maxDescent = -10f; // Maximum descent depth.
    
    private bool playerInRange = false;
    private PlayerController playerController; // Reference to the player's controller.

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Look for the PlayerController on the parent of the collider.
        PlayerController pc = other.GetComponentInParent<PlayerController>();
        if (pc != null)
        {
            playerInRange = true;
            playerController = pc;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        PlayerController pc = other.GetComponentInParent<PlayerController>();
        if (pc != null)
        {
            playerInRange = false;
        }
    }

    private void Update()
    {
        // Check input in Update, so it’s reliably detected.
        if (playerInRange && Input.GetKeyDown(KeyCode.W))
        {
            // Get the RopeDescentController from the player's parent.
            RopeDescentController ropeController = playerController.GetComponent<RopeDescentController>();
            if (ropeController != null)
            {
                ropeController.EnterRopeMode(transform.position, maxDescent);
            }
        }
    }
}
