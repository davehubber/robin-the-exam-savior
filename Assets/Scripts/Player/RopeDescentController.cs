using UnityEngine;

public class RopeDescentController : MonoBehaviour
{
    // Flags and movement settings
    private bool isOnRope = false;
    private bool ignoreUpInput = false; // New flag to ignore initial W press
    private Vector2 ropeAnchor; // The hole's center position
    private float maxDescent;   // Lowest Y position allowed

    public float baseDescentSpeed = 1f;    // constant descent speed when no input is given
    public float acceleratedSpeed = 2f;    // speed when S (for down) or W (for up) is held

    // References
    private Rigidbody2D rb;
    private PlayerController playerController; // to disable normal controls
    private LineRenderer ropeLine;             // for drawing the rope

    // For storing and restoring rotation
    private Quaternion originalRotation;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        playerController = GetComponent<PlayerController>();

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

        isOnRope = true;
        ropeAnchor = anchor;
        this.maxDescent = maxDescend;

        // Disable normal player controls.
        if (playerController != null)
            playerController.DisableControls();

        // Save the player's original rotation.
        originalRotation = transform.rotation;
        // Rotate the player so he is horizontal.
        transform.rotation = Quaternion.Euler(0, 0, 90);

        // Snap player's horizontal position to the rope anchor.
        transform.position = new Vector3(ropeAnchor.x, transform.position.y, transform.position.z);

        // Enable the rope line.
        ropeLine.enabled = true;

        // Set flag to ignore upward input so that the initial W press is consumed.
        ignoreUpInput = true;
    }

    /// <summary>
    /// Handle vertical movement while on the rope.
    /// </summary>
    private void HandleRopeMovement()
    {
        // Release the ignore flag once the player releases the W key.
        if (ignoreUpInput && Input.GetKeyUp(KeyCode.W))
        {
            ignoreUpInput = false;
        }
        
        float moveDir; // positive for upward movement, negative for downward

        // Process input only if we are not ignoring the W key.
        if (!ignoreUpInput && Input.GetKey(KeyCode.W))
        {
            moveDir = 1f; // climb up
        }
        else if (Input.GetKey(KeyCode.S))
        {
            moveDir = -1f; // accelerate descent
        }
        else
        {
            // Default constant descent if no key is pressed.
            moveDir = -1f;
        }

        print(moveDir);

        // Use accelerated speed if input is held; otherwise, use base descent speed.
        float speed = (!ignoreUpInput && Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.S)) ? acceleratedSpeed : baseDescentSpeed;

        print(speed);
        print(rb.position.y);

        // Compute new vertical position.
        Vector2 newPos = rb.position + new Vector2(0, moveDir * speed * Time.deltaTime);

        print(newPos.y);

        // Clamp the vertical position between the rope anchor (top) and the maximum descent.
        newPos.y = Mathf.Clamp(newPos.y, maxDescent, ropeAnchor.y);

        print(newPos.y);
        print(ropeAnchor.y);

        rb.MovePosition(newPos);

        // If the player reaches near the rope anchor (top), exit rope mode.
        if (newPos.y > ropeAnchor.y)
        {
            ExitRopeMode();
        }
    }

    /// Update the rope line renderer to draw the rope from the anchor to the player.
    private void UpdateRopeLine()
    {
        ropeLine.SetPosition(0, new Vector3(ropeAnchor.x, ropeAnchor.y, 0));
        ropeLine.SetPosition(1, transform.position);
    }

    /// Exit rope mode and restore normal player controls.
    public void ExitRopeMode()
    {
        isOnRope = false;
        ropeLine.enabled = false;
        // Restore the player's original rotation.
        transform.rotation = originalRotation;
        // Re-enable normal controls.
        if (playerController != null)
            playerController.EnableControls();
    }
}
