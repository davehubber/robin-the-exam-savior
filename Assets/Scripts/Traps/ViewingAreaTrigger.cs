using UnityEngine;

public class ViewingAreaTrigger : MonoBehaviour
{
    private SecurityCamera securityCamera;

    private void Awake()
    {
        // Assumes the viewing area is a child of the security camera.
        securityCamera = GetComponentInParent<SecurityCamera>();
        if (securityCamera == null)
        {
            Debug.LogError("ViewingAreaTrigger: No SecurityCamera found in parent.");
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // When the player enters the viewing area, notify the security camera.
        if (other.CompareTag("Player"))
        {
            securityCamera?.OnPlayerDetected(other.gameObject.transform.parent.gameObject);
        }
    }
}
