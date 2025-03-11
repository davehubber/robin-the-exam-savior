using UnityEngine;

public class SecurityCamera : MonoBehaviour
{
    [HideInInspector]
    public bool isActive = true;

    // Called by the chewing gum to deactivate the camera.
    public void Deactivate()
    {
        if (!isActive) return;
        isActive = false;

        // Optionally, disable the physical collider so it no longer triggers collisions.
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.enabled = false;
        }

        // Optionally change the appearance (e.g., make the sprite gray) to indicate deactivation.
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.color = Color.gray;
        }

        Debug.Log("Security camera deactivated.");
    }
}
