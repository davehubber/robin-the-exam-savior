using UnityEngine;

public class SecurityCamera : MonoBehaviour
{
    [HideInInspector]
    public bool isActive = true;

    public void Deactivate()
    {
        if (!isActive) return;
        isActive = false;

        // Disable the physical collider so it no longer triggers collisions.
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.enabled = false;
        }

        // Change the appearance of the camera (e.g. set sprite to gray)
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.color = Color.gray;
        }

        // Disable the viewing area (child object) so it disappears.
        Transform viewArea = transform.Find("ViewingArea");
        if (viewArea != null)
        {
            viewArea.gameObject.SetActive(false);
        }
    }
}
