using UnityEngine;
using System.Collections;

public class TriggerButton : MonoBehaviour
{
    [SerializeField] private GameObject blockade;
    
    [SerializeField]
    private Sprite pressedButtonSprite;
    
    [SerializeField]
    private float pressDuration = 0.5f; // Duration (in seconds) for which the pressed sprite is shown
    
    [SerializeField]
    private float blockadeFadeDuration = 1.0f; // Duration of the fade-out effect
    
    [SerializeField]
    private Color connectionColor = new Color(0.2f, 0.8f, 0.2f, 0.5f); // Color of connection line
    
    [SerializeField]
    private float connectionWidth = 0.1f; // Width of the connection line
    
    [SerializeField]
    private float pulsateSpeed = 1.0f; // Speed of the pulsating effect
    
    private SpriteRenderer spriteRenderer;
    private Sprite originalSprite;
    private Vector3 originalPosition;
    private LineRenderer connectionLine;
    
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
        
        // Create the visual connection between button and blockade
        CreateConnectionLine();
    }
    
    private void CreateConnectionLine()
    {
        if (blockade == null)
        {
            Debug.LogWarning("Cannot create connection line: blockade reference is not set.");
            return;
        }
        
        // Add a LineRenderer component to this GameObject
        connectionLine = gameObject.AddComponent<LineRenderer>();
        
        // Configure the LineRenderer
        connectionLine.startWidth = connectionWidth;
        connectionLine.endWidth = connectionWidth;
        connectionLine.material = new Material(Shader.Find("Sprites/Default"));
        
        // Set the sorting layer and order so that the line is rendered on top
        connectionLine.sortingLayerName = "Default";
        connectionLine.sortingOrder = 5;  // Value higher than the button's 4
        
        connectionLine.startColor = connectionColor;
        connectionLine.endColor = connectionColor;
        connectionLine.positionCount = 2;
        
        // Set the line's initial positions
        UpdateConnectionLine();
    }
    
    private void Update()
    {
        if (connectionLine != null && blockade != null)
        {
            UpdateConnectionLine();
            
            // Add subtle pulsating effect to the connection line
            float pulseValue = Mathf.PingPong(Time.time * pulsateSpeed, 0.5f) + 0.5f;
            Color pulseColor = new Color(connectionColor.r, connectionColor.g, connectionColor.b, 
                                         connectionColor.a * pulseValue);
            connectionLine.startColor = pulseColor;
            connectionLine.endColor = pulseColor;
        }
    }
    
    private void UpdateConnectionLine()
    {
        connectionLine.SetPosition(0, transform.position);
        connectionLine.SetPosition(1, blockade.transform.position);
    }
    
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Destroy the connection line
        if (connectionLine != null)
        {
            Destroy(connectionLine);
        }
        
        // Fade out and destroy the blockade if assigned
        if (blockade != null)
        {
            StartCoroutine(FadeOutBlockade());
        }
        else
        {
            Debug.LogWarning("Blockade reference is not set on the TriggerButton script.");
        }
        
        // Swap the sprite to the pressed version and adjust position temporarily
        if (spriteRenderer != null && pressedButtonSprite != null)
        {
            StartCoroutine(PressButton());
        }
    }
    
    private IEnumerator FadeOutBlockade()
    {
        SpriteRenderer blockadeRenderer = blockade.GetComponent<SpriteRenderer>();
        
        if (blockadeRenderer != null)
        {
            // Store the original color
            Color originalColor = blockadeRenderer.color;
            float elapsedTime = 0f;
            
            // Gradually fade out the blockade
            while (elapsedTime < blockadeFadeDuration)
            {
                elapsedTime += Time.deltaTime;
                float normalizedTime = elapsedTime / blockadeFadeDuration;
                
                // Update the alpha value
                Color newColor = originalColor;
                newColor.a = Mathf.Lerp(originalColor.a, 0f, normalizedTime);
                blockadeRenderer.color = newColor;
                
                yield return null;
            }
        }
        
        // Destroy the blockade after fading
        Destroy(blockade);
    }
    
    private IEnumerator PressButton()
    {
        // Change sprite to the pressed sprite
        spriteRenderer.sprite = pressedButtonSprite;
        // Adjust position on the x-axis
        transform.position = originalPosition + new Vector3(0.15f, 0f, 0f);
        
        // Wait for the specified duration
        yield return new WaitForSeconds(pressDuration);
        
        // Revert back to the original sprite and position
        spriteRenderer.sprite = originalSprite;
        transform.position = originalPosition;
    }
}