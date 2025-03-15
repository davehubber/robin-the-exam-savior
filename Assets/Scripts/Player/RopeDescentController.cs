/*using UnityEngine;

public class RopeDescentController : MonoBehaviour
{
    // Flags and movement settings
    private bool isOnRope = false;
    private bool isInitialEntry = false; // Flag for initial rope entry
    private Vector2 ropeAnchor; // The hole's center position
    private float maxDescent;   // Lowest Y position allowed
    private Vector3 entryPosition; // Store initial position when entering rope

    public float baseDescentSpeed = 1f;    // constant descent speed when no input is given
    public float acceleratedSpeed = 2f;    // speed when S (for down) or W (for up) is held
    public LayerMask groundLayer; // Layer mask for ground objects

    // References
    private Rigidbody2D rb;
    private PlayerController playerController; // to disable normal controls
    private LineRenderer ropeLine;             // for drawing the rope
    private Collider2D playerCollider;         // player's collider

    // For storing and restoring rotation
    private Quaternion originalRotation;
    private int originalLayerValue; // Original layer for collision restoration

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        playerController = GetComponent<PlayerController>();
        playerCollider = GetComponent<Collider2D>();

        // Create a new GameObject for the rope line
        GameObject ropeObj = new GameObject("RopeLine");
        ropeLine = ropeObj.AddComponent<LineRenderer>();
        ropeLine.positionCount = 2;
        ropeLine.startWidth = 0.1f;
        ropeLine.endWidth = 0.1f;
        // Optionally set a material and color; ensure its sorting order is behind the player.
        ropeLine.sortingOrder = -1;
        ropeLine.enabled = false;
    }

    void Update()
    {
        if (isOnRope)
        {
            // Check for W key release outside of movement handling
            if (isInitialEntry && Input.GetKeyUp(KeyCode.W))
            {
                isInitialEntry = false;
            }
            
            HandleRopeMovement();
            UpdateRopeLine();
        }
    }

    /// <summary>
    /// Call this method to enter rope mode.
    /// </summary>
    /// <param name="anchor">The center of the hole.</param>
    /// <param name="maxDescend">The lowest Y value the player can descend to.</param>
    public void EnterRopeMode(Vector2 anchor, float maxDescend)
    {
        if (isOnRope) return; // already on rope

        // Store current position for later restoration
        entryPosition = transform.position;
        
        isOnRope = true;
        isInitialEntry = true; // Mark that we just entered the rope
        ropeAnchor = anchor;
        this.maxDescent = maxDescend;

        // Disable normal player controls
        if (playerController != null)
            playerController.DisableControls();

        // Save the player's original rotation
        originalRotation = transform.rotation;
        // Rotate the player so he is horizontal
        transform.rotation = Quaternion.Euler(0, 0, 90);

        // Snap player's horizontal position to the rope anchor
        transform.position = new Vector3(ropeAnchor.x, transform.position.y, transform.position.z);

        // Enable the rope line
        ropeLine.enabled = true;

        // Disable collisions with the ground layer
        DisableGroundCollisions();
    }

    /// <summary>
    /// Disable collisions between player and ground layer
    /// </summary>
    private void DisableGroundCollisions()
    {
        originalLayerValue = gameObject.layer;
        
        // Create a temporary layer for the player that doesn't collide with ground
        // You'll need to set up your Physics2D layer collision matrix in Unity's settings
        // Create a "PlayerOnRope" layer that doesn't collide with the ground layer
        int playerOnRopeLayer = LayerMask.NameToLayer("PlayerOnRope"); 
        
        // If the layer exists, use it
        if (playerOnRopeLayer != -1)
        {
            gameObject.layer = playerOnRopeLayer;
        }
        else
        {
            Debug.LogWarning("PlayerOnRope layer not found. Please create this layer in Unity's Layer settings.");
            // As a fallback, you could use Physics2D.IgnoreLayerCollision here
            Physics2D.IgnoreLayerCollision(gameObject.layer, LayerMask.NameToLayer("Ground"), true);
        }
    }

    /// <summary>
    /// Restore original collision settings
    /// </summary>
    private void RestoreCollisions()
    {
        // Restore original layer
        gameObject.layer = originalLayerValue;
        
        // If we used IgnoreLayerCollision as fallback, restore it here
        if (LayerMask.NameToLayer("PlayerOnRope") == -1)
        {
            Physics2D.IgnoreLayerCollision(gameObject.layer, LayerMask.NameToLayer("Ground"), false);
        }
    }

    /// <summary>
    /// Handle vertical movement while on the rope.
    /// </summary>
    private void HandleRopeMovement()
    {
        float moveDir; // positive for upward movement, negative for downward
        
        // If player is pressing W right at rope entry and we're still in initial entry mode
        if (isInitialEntry && Input.GetKey(KeyCode.W))
        {
            // Consume the W keypress during initial entry
            moveDir = -1f; // Default descent
        }
        // Normal movement handling after initial entry period
        else if (!isInitialEntry && Input.GetKey(KeyCode.W))
        {
            moveDir = 1f; // climb up
        }
        else if (Input.GetKey(KeyCode.S))
        {
            moveDir = -1f; // accelerate descent
        }
        else
        {
            // Default constant descent if no key is pressed
            moveDir = -1f;
        }

        // Use accelerated speed if input is held; otherwise, use base descent speed
        float speed = (!isInitialEntry && Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.S)) 
                     ? acceleratedSpeed 
                     : baseDescentSpeed;

        // Compute new vertical position
        Vector2 newPos = rb.position + new Vector2(0, moveDir * speed * Time.deltaTime);

        // Clamp the vertical position between the rope anchor (top) and the maximum descent
        float clampedY = Mathf.Clamp(newPos.y, maxDescent, ropeAnchor.y - 0.5f); // Add a small offset to prevent immediate exit
        newPos.y = clampedY;

        rb.MovePosition(newPos);

        // Handle rope exit with W key only when not in initial entry mode and near the top
        if (!isInitialEntry && Input.GetKey(KeyCode.W) && 
            newPos.y >= (ropeAnchor.y - 0.6f)) // Slightly below the anchor
        {
            // Allow exit only when W is pressed and we're near the top
            ExitRopeMode(true);
        }
    }

    /// Update the rope line renderer to draw the rope from the anchor to the player.
    private void UpdateRopeLine()
    {
        ropeLine.SetPosition(0, new Vector3(ropeAnchor.x, ropeAnchor.y, 0));
        ropeLine.SetPosition(1, transform.position);
    }

    /// <summary>
    /// Exit rope mode and restore normal player controls.
    /// </summary>
    /// <param name="exitFromTop">Whether the player is exiting from the top of the rope.</param>
    public void ExitRopeMode(bool exitFromTop = false)
    {
        isOnRope = false;
        isInitialEntry = false;
        ropeLine.enabled = false;
        
        // Restore collisions with ground
        RestoreCollisions();
        
        // If exiting from the top, position player just above the hole
        if (exitFromTop)
        {
            transform.position = new Vector3(ropeAnchor.x, ropeAnchor.y + 0.5f, transform.position.z);
        }
        else
        {
            // Return to the exact position player had when entering the rope
            transform.position = entryPosition;
        }
        
        // Restore the player's original rotation
        transform.rotation = originalRotation;
        
        // Re-enable normal controls
        if (playerController != null)
            playerController.EnableControls();
    }

    /// <summary>
    /// Public method to force exit rope mode (can be called from other scripts)
    /// </summary>
    public void ForceExitRope()
    {
        if (isOnRope)
        {
            ExitRopeMode(false); // Return to original position
        }
    }

    /// <summary>
    /// Check if player is currently on rope
    /// </summary>
    public bool IsOnRope()
    {
        return isOnRope;
    }
}
*/