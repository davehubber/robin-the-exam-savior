using UnityEngine;

public class PlayerInteractionController : MonoBehaviour
{
    // The interactable object currently in range (set by the collision handler).
    private GameObject currentInteractable;

    // --- Throwing mechanic fields ---
    [Header("Throwable Settings")]
    [SerializeField] private MovementController movementController;
    [SerializeField] private Transform throwPoint;
    [SerializeField] private float maxThrowForce = 20f;
    [SerializeField] private float throwForceMultiplier = 1f;
    [SerializeField] private LineRenderer aimLine;
    [SerializeField] private float minLineWidth = 0.05f;
    [SerializeField] private float maxLineWidth = 0.2f;

    private bool isAiming = false;
    private float currentThrowForce = 0f;
    // The throwable currently held by the player (if any).
    private Throwable heldThrowable = null;

    // --- Hiding mechanic fields ---
    public bool isHidden = false;
    private int originalSortingOrder;
    private SpriteRenderer playerSprite;

    // --- Rope mechanic fields ---
    // Indicates whether the player is currently using the rope.
    private bool isOnRope = false;
    // When entering rope mode, we ignore the current W input until released.
    private bool ropeIgnoreW = true;
    // The position the player will return to when leaving the rope.
    private Vector3 ropeExitPosition;
    // The RopeHole's transform (its center).
    private Transform ropeHoleTransform;
    // Store the original rotation so it can be restored.
    private Quaternion originalRotation;
    // Parameters for rope movement (set these in the inspector as needed).
    [Header("Rope Settings")]
    public float ropeDefaultFallSpeed = 0.5f;   // Slow fall speed by default.
    public float ropeClimbSpeed = 1f;         // Upward (and fast downward) speed when holding W/S.
    public float maxRopeDepth = 5f;           // Maximum distance below the RopeHole center.
    
    // A separate LineRenderer for rendering the rope.
    [SerializeField] private LineRenderer ropeLine;

    // Cached Rigidbody2D reference for rope mode movement.
    private Rigidbody2D rb;

    private void Awake()
    {
        // Setup throw point if not already assigned.
        if (throwPoint == null)
        {
            GameObject throwPointObj = new GameObject("ThrowPoint");
            throwPointObj.transform.SetParent(transform);
            throwPointObj.transform.localPosition = new Vector3(0.5f, 0.2f, 0f);
            throwPoint = throwPointObj.transform;
        }

        // Setup the aim line if not already assigned.
        if (aimLine == null)
        {
            aimLine = gameObject.AddComponent<LineRenderer>();
            aimLine.positionCount = 2; // Only need two points: start and end.
            aimLine.enabled = false;

            // Set up a simple material for the line.
            aimLine.material = new Material(Shader.Find("Sprites/Default"));
            aimLine.startColor = new Color(1f, 1f, 1f, 0.8f);
            aimLine.endColor = new Color(1f, 1f, 1f, 0.8f);
        }

        // Setup ropeLine if not already assigned.
        if (ropeLine == null)
        {
            GameObject ropeLineObj = new GameObject("RopeLine");
            ropeLineObj.transform.SetParent(transform);
            ropeLine = ropeLineObj.AddComponent<LineRenderer>();
            ropeLine.positionCount = 2;
            ropeLine.enabled = false;
            ropeLine.material = new Material(Shader.Find("Sprites/Default"));
            ropeLine.startColor = new Color(0.54f, 0.27f, 0.07f, 1.0f);
            ropeLine.endColor = new Color(0.72f, 0.52f, 0.04f, 1.0f);
            ropeLine.startWidth = 0.1f;
            ropeLine.endWidth = 0.1f;
        }

        if (movementController == null)
        {
            movementController = GetComponent<MovementController>();
        }

        playerSprite = GetComponentInChildren<SpriteRenderer>();
        if (playerSprite != null)
        {
            originalSortingOrder = playerSprite.sortingOrder;
        }
        else
        {
            Debug.LogWarning("PlayerInteractionController: No SpriteRenderer found on the player.");
        }

        rb = GetComponent<Rigidbody2D>();
    }

    // Called by the collision handler to update the current interactable.
    public void SetCurrentInteractable(GameObject interactable)
    {
        currentInteractable = interactable;
    }

    public GameObject GetCurrentInteractable()
    {
        return currentInteractable;
    }

    private void Update()
    {
        // If the player is on the rope, process rope-specific input and movement.
        if (isOnRope)
        {
            ProcessRopeMovement();
            return;
        }

        // Normal interaction: When "W" is pressed, perform the interaction.
        if (Input.GetKeyDown(KeyCode.W))
        {
            HandleInteraction();
        }

        // If holding a throwable, handle aiming and throwing.
        if (heldThrowable != null)
        {
            // Toggle aiming mode with T.
            if (Input.GetKeyDown(KeyCode.T))
            {
                ToggleAiming();
            }

            if (isAiming)
            {
                HandleAiming();

                // If left mouse button is pressed, throw the held object.
                if (Input.GetMouseButtonDown(0))
                {
                    ThrowHeldThrowable();
                }
            }
        }
    }

    // Determines the interaction based on the current interactable's tag.
    private void HandleInteraction()
    {
        if (currentInteractable == null)
            return;

        // Portal interaction.
        if (currentInteractable.CompareTag("Portal"))
        {
            Portal portal = currentInteractable.GetComponent<Portal>();
            if (portal != null && portal.exitPoint != null)
            {
                transform.position = portal.exitPoint.position;
            }
        }
        // Hiding spot interaction.
        else if (currentInteractable.CompareTag("HidingSpot"))
        {
            ToggleHiding();
        }
        // Throwable pickup.
        else if (currentInteractable.GetComponent<Throwable>() != null)
        {
            if (heldThrowable == null)
            {
                PickupThrowable(currentInteractable);
            }
        }
        // RopeHole interaction.
        else if (currentInteractable.CompareTag("RopeHole"))
        {
            EnterRopeMode(currentInteractable);
        }
    }

    // Picks up a throwable object.
    private void PickupThrowable(GameObject throwableObject)
    {
        Throwable throwable = throwableObject.GetComponent<Throwable>();
        if (throwable != null)
        {
            heldThrowable = throwable;
            throwable.transform.SetParent(throwPoint);
            throwable.transform.localPosition = Vector3.zero;
            throwable.PickUp();
        }
    }

    private void ToggleAiming()
    {
        isAiming = !isAiming;
        if (isAiming)
        {
            aimLine.enabled = true;
            
            // Disable movement while aiming.
            if (movementController != null)
                movementController.DisableMovement();
        }
        else
        {
            aimLine.enabled = false;
            
            // Re-enable movement when not aiming.
            if (movementController != null)
                movementController.EnableMovement();
        }
    }

    private void ToggleHiding()
    {
        // Hiding logic here.
        SpriteRenderer hidingSpotSprite = currentInteractable.GetComponent<SpriteRenderer>();
        if (hidingSpotSprite == null)
        {
            Debug.LogWarning("PlayerInteractionController: HidingSpot does not have a SpriteRenderer.");
            return;
        }

        if (!isHidden)
        {
            isHidden = true;
            if (movementController != null)
                movementController.DisableMovement();

            if (playerSprite != null)
                playerSprite.sortingOrder = hidingSpotSprite.sortingOrder - 1;
        }
        else
        {
            isHidden = false;
            if (movementController != null)
                movementController.EnableMovement();

            if (playerSprite != null)
                playerSprite.sortingOrder = originalSortingOrder;
        }
    }

    private void HandleAiming()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0;
        
        Vector2 direction = (mousePos - throwPoint.position).normalized;
        float distance = Vector2.Distance(mousePos, throwPoint.position);
        
        currentThrowForce = Mathf.Clamp(distance * throwForceMultiplier, 0, maxThrowForce);
        
        float normalizedForce = currentThrowForce / maxThrowForce;
        float lineWidth = Mathf.Lerp(minLineWidth, maxLineWidth, normalizedForce);
        
        aimLine.startWidth = lineWidth;
        aimLine.endWidth = lineWidth;
        aimLine.SetPosition(0, throwPoint.position);
        aimLine.SetPosition(1, mousePos);
    }

    private void ThrowHeldThrowable()
    {
        if (heldThrowable != null)
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0;
            Vector2 direction = (mousePos - throwPoint.position).normalized;
            
            heldThrowable.Launch(direction, currentThrowForce);
            heldThrowable = null;
            
            isAiming = false;
            aimLine.enabled = false;

            if (movementController != null)
                movementController.EnableMovement();
        }
    }

    // ========================
    // Rope Mode Implementation
    // ========================

    // Called when the player interacts with a RopeHole.
    private void EnterRopeMode(GameObject ropeHole)
    {
        // Begin rope mode.
        isOnRope = true;
        ropeIgnoreW = true; // Ignore the current W press.
        ropeExitPosition = transform.position; // Record original position.
        ropeHoleTransform = ropeHole.transform;
        originalRotation = transform.rotation; // Save original rotation.

        // Snap player to the center of the RopeHole.
        transform.position = ropeHoleTransform.position;

        // Flip player's rotation based on facing direction.
        if (transform.localScale.x > 0)
            transform.rotation = Quaternion.Euler(0, 0, -90);
        else
            transform.rotation = Quaternion.Euler(0, 0, 90);

        // Disable normal movement.
        if (movementController != null)
            movementController.DisableMovement();

        // Disable ground collisions (assumes ground is on the "Ground" layer).
        int playerLayer = gameObject.layer;
        int groundLayer = LayerMask.NameToLayer("Ground");
        Physics2D.IgnoreLayerCollision(playerLayer, groundLayer, true);

        // Make the Rigidbody kinematic to avoid physics interference.
        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Kinematic;

        // Enable rope line rendering.
        ropeLine.enabled = true;
        ropeLine.positionCount = 2;
        ropeLine.SetPosition(0, ropeHoleTransform.position);
        ropeLine.SetPosition(1, transform.position);

        // Optionally, clear currentInteractable so that the RopeHole won't re-trigger immediately.
        currentInteractable = null;
    }

    // Process input and movement while on the rope.
    private void ProcessRopeMovement()
    {
        // Once the player releases W, stop ignoring it.
        if (ropeIgnoreW && !Input.GetKey(KeyCode.W))
        {
            ropeIgnoreW = false;
        }

        float verticalSpeed = 0f;
        // If W is held (after the initial press is cancelled), move upward.
        if (!ropeIgnoreW && Input.GetKey(KeyCode.W))
        {
            verticalSpeed = ropeClimbSpeed;
        }
        // If S is held, move downward faster.
        else if (Input.GetKey(KeyCode.S))
        {
            verticalSpeed = -ropeClimbSpeed;
        }
        else
        {
            // Default slow fall.
            verticalSpeed = -ropeDefaultFallSpeed;
        }

        // Compute new vertical position using rb.MovePosition for smoother movement.
        Vector2 pos = rb.position;
        pos.y += verticalSpeed * Time.fixedDeltaTime;
        //pos.y += verticalSpeed * Time.deltaTime;
        float upperBound = ropeHoleTransform.position.y;
        float lowerBound = ropeHoleTransform.position.y - maxRopeDepth;
        pos.y = Mathf.Clamp(pos.y, lowerBound, upperBound);
        rb.MovePosition(pos);

        // Update rope line so it stretches from the hole to the player.
        ropeLine.SetPosition(1, pos);

        // Use a threshold to check if the player has reached the top.
        if (Mathf.Abs(pos.y - upperBound) < 0.01f && Input.GetKey(KeyCode.W) && !ropeIgnoreW)
        {
            ExitRopeMode();
        }
    }

    // Called when the rope mode is exited.
    private void ExitRopeMode()
    {
        isOnRope = false;

        // Disable rope line.
        ropeLine.enabled = false;

        // Re-enable ground collisions.
        int playerLayer = gameObject.layer;
        int groundLayer = LayerMask.NameToLayer("Ground");
        Physics2D.IgnoreLayerCollision(playerLayer, groundLayer, false);

        // Restore the original rotation.
        transform.rotation = originalRotation;

        // Restore Rigidbody settings.
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.linearVelocity = Vector2.zero;

        // Teleport the player back to the position they were at before entering rope mode.
        transform.position = ropeExitPosition;

        // Re-enable normal movement.
        if (movementController != null)
            movementController.EnableMovement();
    }

}
