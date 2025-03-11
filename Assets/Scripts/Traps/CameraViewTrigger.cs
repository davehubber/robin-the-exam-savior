using UnityEngine;

public class CameraViewTrigger : MonoBehaviour
{
    private SecurityCamera securityCamera;

    private void Start()
    {
        // Get the SecurityCamera component from the parent.
        securityCamera = GetComponentInParent<SecurityCamera>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // If the camera is active and the player enters the viewing area, trigger game over.
        if (securityCamera != null && securityCamera.isActive && other.CompareTag("Player"))
        {
            Debug.Log("Player caught by security camera!");
            GameManager.Instance.GameOver(false);
        }
    }
}
