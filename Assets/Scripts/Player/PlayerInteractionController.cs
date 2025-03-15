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
    [SerializeField] private LineRenderer trajectoryLine;
    [SerializeField] private GameObject aimIndicator;
    [SerializeField] private float trajectoryLineLengthMultiplier = 1f;

    private bool isAiming = false;
    private float currentThrowForce = 0f;
    // The throwable currently held by the player (if any).
    private Throwable heldThrowable = null;

    // --- Hiding mechanic fields ---
    public bool isHidden = false;
    private int originalSortingOrder;
    private SpriteRenderer playerSprite;

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

        // Setup the trajectory line if not already assigned.
        if (trajectoryLine == null)
        {
            trajectoryLine = gameObject.AddComponent<LineRenderer>();
            trajectoryLine.positionCount = 2;
            trajectoryLine.startWidth = 0.1f;
            trajectoryLine.endWidth = 0.1f;
            trajectoryLine.enabled = false;
        }

        if (aimIndicator != null)
        {
            aimIndicator.SetActive(false);
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
        // When "W" is pressed, perform the interaction.
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

        print(currentInteractable);

        // Example: if the interactable is a portal.
        if (currentInteractable.CompareTag("Portal"))
        {
            Portal portal = currentInteractable.GetComponent<Portal>();
            if (portal != null && portal.exitPoint != null)
            {
                transform.position = portal.exitPoint.position;
            }
        }
        else if (currentInteractable.CompareTag("HidingSpot"))
        {
            ToggleHiding();
        }
        else if (currentInteractable.GetComponent<Throwable>() != null)
        {
            if (heldThrowable == null)
            {
                PickupThrowable(currentInteractable);
            }
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
        }
    }

    private void ToggleAiming()
    {
        isAiming = !isAiming;
        if (isAiming)
        {
            if (aimIndicator != null)
                aimIndicator.SetActive(true);
            trajectoryLine.enabled = true;
            
            // Disable movement while aiming
            if (movementController != null)
                movementController.DisableMovement();
        }
        else
        {
            if (aimIndicator != null)
                aimIndicator.SetActive(false);
            trajectoryLine.enabled = false;
            
            // Re-enable movement when not aiming
            if (movementController != null)
                movementController.EnableMovement();
        }
    }

    private void ToggleHiding()
    {
        // Retrieve the hiding spot's SpriteRenderer to adjust sorting order.
        SpriteRenderer hidingSpotSprite = currentInteractable.GetComponent<SpriteRenderer>();
        if (hidingSpotSprite == null)
        {
            Debug.LogWarning("PlayerInteractionController: HidingSpot does not have a SpriteRenderer.");
            return;
        }

        if (!isHidden)
        {
            // Enter hiding: disable movement and adjust sorting order.
            isHidden = true;
            if (movementController != null)
                movementController.DisableMovement();

            // Set player's sprite sorting order to one below the hiding spot.
            if (playerSprite != null)
                playerSprite.sortingOrder = hidingSpotSprite.sortingOrder - 1;
        }
        else
        {
            // Exit hiding: enable movement and restore sorting order.
            isHidden = false;
            if (movementController != null)
                movementController.EnableMovement();

            if (playerSprite != null)
                playerSprite.sortingOrder = originalSortingOrder;
        }
    }

    private void HandleAiming()
    {
        // Get mouse position in world coordinates.
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0;
        Vector2 direction = (mousePos - throwPoint.position).normalized;
        float distance = Vector2.Distance(mousePos, throwPoint.position);
        currentThrowForce = Mathf.Clamp(distance * throwForceMultiplier, 0, maxThrowForce);

        if (aimIndicator != null)
        {
            aimIndicator.transform.position = throwPoint.position;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            aimIndicator.transform.rotation = Quaternion.Euler(0, 0, angle);
        }

        Vector3 endPos = throwPoint.position + (Vector3)(direction * currentThrowForce * trajectoryLineLengthMultiplier);
        trajectoryLine.SetPosition(0, throwPoint.position);
        trajectoryLine.SetPosition(1, endPos);
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
            if (aimIndicator != null)
                aimIndicator.SetActive(false);
            trajectoryLine.enabled = false;

            // Re-enable movement after throwing
            if (movementController != null)
                movementController.EnableMovement();
        }
    }
}
