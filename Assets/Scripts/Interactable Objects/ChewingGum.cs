using UnityEngine;

public class ChewingGum : Throwable
{
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Check if the collided object has a SecurityCamera component.
        SecurityCamera camera = collision.gameObject.GetComponent<SecurityCamera>();
        if (camera != null && camera.isActive)
        {
            camera.Deactivate();
            // Destroy the chewing gum after deactivating the camera.
            Destroy(gameObject);
        }
    }
}
