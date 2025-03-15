using UnityEngine;

public class HidingSpot : MonoBehaviour
{
    [Header("Settings")]
    public float interactionRadius = 1.5f;
    public string playerTag = "Player";
    public SpriteRenderer visualIndicator;  // Optional visual cue when player can hide
    
    [Header("Visual Settings")]
    public bool changePlayerLayerWhenHiding = true;
    public string hiddenLayerName = "HiddenPlayer";
    public bool makePlayerTransparentWhenHiding = true;
    public float hiddenAlpha = 0.5f;
    
    // Private variables
    private PlayerController playerController;
    private SpriteRenderer playerSpriteRenderer;
    private int defaultPlayerLayer;
    private int hiddenLayer;
    private bool playerIsHiding = false;
    private Color originalPlayerColor;
    
    void Start()
    {
        // Initialize the hidden layer ID
        if (changePlayerLayerWhenHiding)
        {
            hiddenLayer = LayerMask.NameToLayer(hiddenLayerName);
            if (hiddenLayer == -1)
            {
                Debug.LogWarning("Hidden layer '" + hiddenLayerName + "' not found. Create this layer in your project settings.");
            }
        }
        
        // Hide the indicator initially
        if (visualIndicator != null)
        {
            visualIndicator.enabled = false;
        }
    }
    
    void Update()
    {
        // Check if player is in range
        GameObject player = GameObject.FindGameObjectWithTag(playerTag);
        if (player == null) return;
        
        float distanceToPlayer = Vector2.Distance(transform.position, player.transform.position);
        bool playerInRange = distanceToPlayer <= interactionRadius;
        
        // Handle interaction indicator
        if (visualIndicator != null)
        {
            visualIndicator.enabled = playerInRange && !playerIsHiding;
        }
        
        // Store controller reference if we don't have it
        if (playerController == null && playerInRange)
        {
            playerController = player.GetComponentInParent<PlayerController>();
            if (makePlayerTransparentWhenHiding)
            {
                playerSpriteRenderer = player.GetComponent<SpriteRenderer>();
                if (playerSpriteRenderer != null)
                {
                    originalPlayerColor = playerSpriteRenderer.color;
                }
            }
            
            if (changePlayerLayerWhenHiding)
            {
                defaultPlayerLayer = player.layer;
            }
        }
        
        // Check for player input to hide/unhide
        if (playerInRange && Input.GetKeyDown(KeyCode.W) && playerController != null)
        {
            if (!playerIsHiding)
            {
                HidePlayer();
            }
            else
            {
                UnhidePlayer();
            }
        }
        
        // If player moves too far while hiding, force unhide
        if (playerIsHiding && distanceToPlayer > interactionRadius + 0.5f)
        {
            UnhidePlayer();
        }
    }
    
    private void HidePlayer()
    {
        playerIsHiding = true;
        
        // Position the player behind the hiding spot
        playerController.transform.position = transform.position;
        
        // Disable movement
        playerController.DisableControls();
        
        // Make player "invisible" to enemies by changing layer
        if (changePlayerLayerWhenHiding && hiddenLayer != -1)
        {
            playerController.gameObject.layer = hiddenLayer;
        }
        
        // Make player semi-transparent
        if (makePlayerTransparentWhenHiding && playerSpriteRenderer != null)
        {
            Color hiddenColor = originalPlayerColor;
            hiddenColor.a = hiddenAlpha;
            playerSpriteRenderer.color = hiddenColor;
        }
        
        // Hide any indicator
        if (visualIndicator != null)
        {
            visualIndicator.enabled = false;
        }
    }
    
    private void UnhidePlayer()
    {
        playerIsHiding = false;
        
        // Re-enable movement
        playerController.EnableControls();
        
        // Restore original layer
        if (changePlayerLayerWhenHiding)
        {
            playerController.gameObject.layer = defaultPlayerLayer;
        }
        
        // Restore original visibility
        if (makePlayerTransparentWhenHiding && playerSpriteRenderer != null)
        {
            playerSpriteRenderer.color = originalPlayerColor;
        }
    }
    
    void OnDrawGizmosSelected()
    {
        // Show interaction radius in the editor
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, interactionRadius);
    }
}