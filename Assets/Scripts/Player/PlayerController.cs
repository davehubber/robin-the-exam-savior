/*using System;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float acceleration;
    public float groundSpeed;
    public float jumpSpeed;
    [Range(0f, 1f)]
    public float groundDecay;
    public Rigidbody2D body;
    public BoxCollider2D groundCheck;
    public LayerMask groundMask;
    public bool grounded;
    float xInput;
    private Portal currentPortal;

    private bool isInTrap = false;
    private float slowdownFactor = 0.15f;

    [Header("Throwing")]
    public float maxThrowForce = 20f;
    public float throwForceMultiplier = 1f;
    public Transform throwPoint;
    public float pickupRadius = 1f;
    public LayerMask throwableLayer;
    public GameObject aimIndicator;
    public LineRenderer trajectoryLine;

    [Header("Game Integration")]
    public bool canInteractWithObjects = true;
    
    // State variables
    private Throwable heldObject;
    private bool isAiming = false;
    private float currentThrowForce = 0f;
    private bool canMove = true;

    private bool isSpeedBoosted = false;
    private float speedBoostMultiplier = 1f;
    private float speedBoostEndTime = 0f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start() {
        if (throwPoint == null)
        {
            // Create throw point if not assigned
            GameObject throwPointObj = new GameObject("ThrowPoint");
            throwPointObj.transform.parent = transform;
            throwPointObj.transform.localPosition = new Vector3(0.5f, 0.2f, 0); // Adjust as needed
            throwPoint = throwPointObj.transform;
        }
        
        if (trajectoryLine == null)
        {
            trajectoryLine = gameObject.AddComponent<LineRenderer>();
            trajectoryLine.positionCount = 30;
            trajectoryLine.startWidth = 0.1f;
            trajectoryLine.endWidth = 0.1f;
            trajectoryLine.enabled = false;
        }
        
        if (aimIndicator != null)
        {
            aimIndicator.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update() {
        // Check if game is over
        if (GameManager.Instance != null && GameManager.Instance.isGameOver) {
            DisableControls();
            return;
        }

        // Handle throwable pickup - prioritize over portal use
        if (Input.GetKeyDown(KeyCode.W)) {
            Throwable nearestThrowable = FindNearestThrowable();
            
            if (heldObject == null && nearestThrowable != null) {
                // If there's a throwable nearby, pick it up
                PickupThrowable(nearestThrowable);
            } 
            else if (currentPortal != null) {
                // Only use portal if no throwable was picked up
                if (currentPortal.exitPoint != null) {
                    transform.position = currentPortal.exitPoint.position;
                }
            }
        }
        
        // Handle throw state toggle
        if (Input.GetKeyDown(KeyCode.T)) {
            if (heldObject != null) {
                isAiming = !isAiming;
                canMove = !isAiming;
                
                if (aimIndicator != null) {
                    aimIndicator.SetActive(isAiming);
                }
                
                trajectoryLine.enabled = isAiming;
            }
        }
        
        // Handle throwing
        if (isAiming) {
            HandleAiming();
            
            if (Input.GetMouseButtonDown(0)) {
                ThrowObject();
                isAiming = false;
                canMove = true;
                
                if (aimIndicator != null) {
                    aimIndicator.SetActive(false);
                }
                trajectoryLine.enabled = false;
            }
        }

        if (isSpeedBoosted && Time.time >= speedBoostEndTime)
        {
            isSpeedBoosted = false;
            speedBoostMultiplier = 1f; // Reset speed
        }
        
        // Only get movement input if we can move
        if (canMove) {
            GetInput();
            HandleJump();
        } else {
            xInput = 0; // Reset input when aiming
        }
    }

    void FixedUpdate() {
        if (canMove) {
            MoveWithInput();
        }
        CheckGround();
        ApplyFriction();
    }

    void GetInput() {
        xInput = Input.GetAxis("Horizontal");
    }

    void MoveWithInput() {
        if(Math.Abs(xInput) > 0) {
            float increment = xInput * acceleration;
            float speed = (isInTrap ? groundSpeed * slowdownFactor : groundSpeed) * speedBoostMultiplier;
            float newSpeed = Mathf.Clamp(body.linearVelocityX + increment, -speed, speed);
            body.linearVelocity = new Vector2(newSpeed, body.linearVelocity.y);
            float direction = Math.Sign(xInput);
            Vector3 currentScale = transform.localScale;
            transform.localScale = new Vector3(Mathf.Abs(currentScale.x) * direction, currentScale.y, currentScale.z);
        }
    }

    void HandleJump() {
        if(Input.GetButtonDown("Jump") && grounded && !isInTrap) {
            body.linearVelocity = new Vector2(body.linearVelocity.x, jumpSpeed);
        }
    }

    void CheckGround() {
        grounded = Physics2D.OverlapAreaAll(groundCheck.bounds.min, groundCheck.bounds.max, groundMask).Length > 0;
    }

    void ApplyFriction() {
        if (grounded && xInput == 0 && body.linearVelocity.y <= 0) {
            body.linearVelocity *= groundDecay;
        }
    }

    // Detect portal triggers.
    private void OnTriggerEnter2D(Collider2D collision) {
        if(collision.CompareTag("Portal")) {
            Portal portal = collision.GetComponent<Portal>();
            if(portal != null) {
                currentPortal = portal;
            }
        }

        if (collision.CompareTag("SlowdownTrap")) {
            isInTrap = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision) {
        if(collision.CompareTag("Portal")) {
            Portal portal = collision.GetComponent<Portal>();
            if(portal != null && portal == currentPortal) {
                currentPortal = null;
            }
        }

        if (collision.CompareTag("SlowdownTrap")) {
            isInTrap = false;
        }
    }
    
    // Throwable-related methods
    private Throwable FindNearestThrowable() {
        Collider2D[] nearbyThrowables = Physics2D.OverlapCircleAll(transform.position, pickupRadius, throwableLayer);
        
        if (nearbyThrowables.Length > 0) {
            // Find closest throwable
            Throwable closest = null;
            float closestDistance = float.MaxValue;
            
            foreach (Collider2D coll in nearbyThrowables) {
                Throwable throwable = coll.GetComponent<Throwable>();
                if (throwable != null && !throwable.IsPickedUp()) {
                    float distance = Vector2.Distance(transform.position, coll.transform.position);
                    if (distance < closestDistance) {
                        closest = throwable;
                        closestDistance = distance;
                    }
                }
            }
            
            return closest;
        }
        
        return null;
    }
    
    private void PickupThrowable(Throwable throwable) {
        if (throwable != null) {
            heldObject = throwable;
            heldObject.PickUp();
            heldObject.transform.position = throwPoint.position;
            heldObject.transform.SetParent(throwPoint);
        }
    }
    
    private void HandleAiming() {
        // Get mouse position in world coordinates
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0;
        
        // Calculate direction and force
        Vector2 direction = (mousePos - throwPoint.position).normalized;
        
        // Calculate force based on distance (clamped)
        float distance = Vector2.Distance(mousePos, throwPoint.position);
        currentThrowForce = Mathf.Clamp(distance * throwForceMultiplier, 0, maxThrowForce);
        
        // Visual feedback for aiming
        if (aimIndicator != null) {
            aimIndicator.transform.position = throwPoint.position;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            aimIndicator.transform.rotation = Quaternion.Euler(0, 0, angle);
            
            // Scale indicator based on force
            float scaleRatio = currentThrowForce / maxThrowForce;
            aimIndicator.transform.localScale = new Vector3(
                scaleRatio * 2f, 
                0.2f, 
                1f
            );
        }
        
        // Update direction line instead of trajectory
        UpdateDirectionLine(throwPoint.position, mousePos);
    }

    private void UpdateDirectionLine(Vector2 startPos, Vector2 endPos) {
        // Set only two points for the line renderer
        trajectoryLine.positionCount = 2;
        
        // Set the start and end points
        trajectoryLine.SetPosition(0, startPos);
        trajectoryLine.SetPosition(1, endPos);
        
        // Optionally adjust line width based on throw force
        float widthRatio = currentThrowForce / maxThrowForce;
        trajectoryLine.startWidth = 0.1f * widthRatio;
        trajectoryLine.endWidth = 0.05f * widthRatio;
    }
    
    private void ThrowObject() {
        if (heldObject != null) {
            // Calculate throw direction from player to mouse
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0;
            
            Vector2 direction = ((Vector2)mousePos - (Vector2)throwPoint.position).normalized;
            Vector2 throwForce = direction * currentThrowForce;
            
            // Detach the object
            heldObject.transform.SetParent(null);
            
            // Apply the throw
            heldObject.Throw(throwForce);
            
            // Reset
            heldObject = null;
            currentThrowForce = 0f;
        }
    }
    
    public void ReleaseHeldObject() {
        if (heldObject != null) {
            heldObject.transform.SetParent(null);
            heldObject.Release();
            heldObject = null;
            
            // Reset states
            isAiming = false;
            canMove = true;
            if (aimIndicator != null) {
                aimIndicator.SetActive(false);
            }
            trajectoryLine.enabled = false;
        }
    }

    public void EnableControls() {
        canMove = true;
        canInteractWithObjects = true;
    }

    public void DisableControls() {
        canMove = false;
        canInteractWithObjects = false;
        ReleaseHeldObject();
    }

    public void ActivateSpeedBoost(float multiplier, float duration)
    {
        if (!isSpeedBoosted) // Apply boost only if not already active
        {
            isSpeedBoosted = true;
            speedBoostMultiplier = multiplier;
            speedBoostEndTime = Time.time + duration;
        }
    }

    void OnDrawGizmosSelected() {
        // Draw pickup radius in editor
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, pickupRadius);
    }
}
*/