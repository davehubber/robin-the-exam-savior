using UnityEngine;

public class CameraViewTrigger : MonoBehaviour
{
    private SecurityCamera securityCamera;
    private SpriteRenderer sr;

    private void Start()
    {
        // Get the SecurityCamera component from the parent.
        securityCamera = GetComponentInParent<SecurityCamera>();
        // Get the SpriteRenderer attached to the viewing area (if any)
        sr = GetComponent<SpriteRenderer>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Only trigger if the camera is active and the collider is the player.
        if (securityCamera != null && securityCamera.isActive && other.CompareTag("Player"))
        {
            // Change the viewing area color to red
            if (sr != null)
            {
                sr.color = Color.red;
            }

            // Trigger game over with a custom message
            GameManager.Instance.GameOver(false, "CAUGHT BY CAMERA!");
        }
    }
}
