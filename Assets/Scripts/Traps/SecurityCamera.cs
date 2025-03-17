using UnityEngine;

public class SecurityCamera : MonoBehaviour
{
    [Header("Viewing Area Settings")]
    [SerializeField] private GameObject viewingArea; // Assign the child object representing the viewing area.
    [SerializeField] private Color idleColor = new Color(1f, 1f, 0f, 0.5f); // Yellow with transparency.
    [SerializeField] private Color alertColor = Color.red;
    [SerializeField] private string defeatMessage = "Caught by the security camera!";

    private SpriteRenderer viewingAreaRenderer;
    private bool isActive = true;

    private void Awake()
    {
        if (viewingArea != null)
        {
            viewingAreaRenderer = viewingArea.GetComponent<SpriteRenderer>();
            if (viewingAreaRenderer != null)
            {
                viewingAreaRenderer.color = idleColor;
            }
            else
            {
                Debug.LogWarning("SecurityCamera: No SpriteRenderer found on viewing area.");
            }
        }
        else
        {
            Debug.LogError("SecurityCamera: Viewing Area not assigned.");
        }
    }

    // This method is called by the ViewingAreaTrigger when the player enters the viewing area.
    public void OnPlayerDetected(GameObject player)
    {
        if (!isActive)
            return;

        if (viewingAreaRenderer != null)
        {
            viewingAreaRenderer.color = alertColor;
        }

        // Trigger game over with the custom defeat message.
        if (GameManager.Instance != null)
        {
            GameManager.Instance.GameOver(false, defeatMessage);
        }
    }

    // If the security camera collides with a BubbleGum object, it deactivates.
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("BubbleGum"))
        {
            // Destroy the BubbleGum object.
            Destroy(collision.gameObject);

            // Turn the camera grey.
            SpriteRenderer cameraRenderer = GetComponent<SpriteRenderer>();
            if (cameraRenderer != null)
            {
                cameraRenderer.color = Color.grey;
            }
            else
            {
                Debug.LogWarning("SecurityCamera: No SpriteRenderer found on the camera.");
            }
            
            // Deactivate further detection.
            DeactivateCamera();
        }
    }

    private void DeactivateCamera()
    {
        isActive = false;

        // Destroy the viewing area so it no longer detects the player.
        if (viewingArea != null)
        {
            Destroy(viewingArea);
        }

        // Optionally disable further behavior.
        this.enabled = false;
    }
}
