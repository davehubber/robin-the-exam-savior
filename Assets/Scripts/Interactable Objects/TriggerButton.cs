using UnityEngine;
using System.Collections;

public class TriggerButton : MonoBehaviour
{
    [SerializeField]
    private GameObject blockade;

    [SerializeField]
    private Sprite pressedButtonSprite;

    [SerializeField]
    private float pressDuration = 0.5f; // Duration (in seconds) for which the pressed sprite is shown

    private SpriteRenderer spriteRenderer;
    private Sprite originalSprite;
    private Vector3 originalPosition;

    private void Awake()
    {
        // Get the SpriteRenderer component attached to this GameObject.
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            originalSprite = spriteRenderer.sprite;
        }
        else
        {
            Debug.LogWarning("No SpriteRenderer found on the button object!");
        }
        
        // Store the original position of the button.
        originalPosition = transform.position;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Destroy the blockade if assigned.
        if (blockade != null)
        {
            Destroy(blockade);
        }
        else
        {
            Debug.LogWarning("Blockade reference is not set on the TriggerButton script.");
        }

        // Swap the sprite to the pressed version and adjust position temporarily.
        if (spriteRenderer != null && pressedButtonSprite != null)
        {
            StartCoroutine(PressButton());
        }
    }

    private IEnumerator PressButton()
    {
        // Change sprite to the pressed sprite.
        spriteRenderer.sprite = pressedButtonSprite;
        // Adjust position on the x-axis.
        transform.position = originalPosition + new Vector3(0.15f, 0f, 0f);

        // Wait for the specified duration.
        yield return new WaitForSeconds(pressDuration);

        // Revert back to the original sprite and position.
        spriteRenderer.sprite = originalSprite;
        transform.position = originalPosition;
    }
}
