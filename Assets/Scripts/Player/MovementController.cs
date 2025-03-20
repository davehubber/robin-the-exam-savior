using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class MovementController : MonoBehaviour
{
    #region Movement Settings
    [Header("Movement Settings")]
    [SerializeField] private float acceleration = 10f;
    [SerializeField] private float groundSpeed = 5f;
    [SerializeField] private float jumpSpeed = 10f;
    [SerializeField, Range(0f, 1f)] private float groundDecay = 0.9f;
    [SerializeField] private float slowdownFactor = 0.15f;
    #endregion

    #region Ground Check
    [Header("Ground Check")]
    [SerializeField] private BoxCollider2D groundCheck;
    [SerializeField] private LayerMask groundMask;
    #endregion

    #region Speed Boost
    private bool isSpeedBoosted = false;
    private float speedBoostMultiplier = 4f;
    private float speedBoostEndTime = 0f;
    #endregion

    #region State Variables
    private Rigidbody2D rb;
    private float horizontalInput;
    private bool movementEnabled = true;
    private bool isSlowDownActive = false;
    #endregion

    private Animator animator;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        if (groundCheck == null)
        {
            Debug.LogWarning($"{nameof(MovementController)} on {gameObject.name} has no groundCheck assigned.");
        }
    }

    private void Update()
    {
        if (!movementEnabled) return;

        // Read horizontal movement input.
        horizontalInput = Input.GetAxis("Horizontal");

        // Process jump if grounded and not slowed.
        if (Input.GetButtonDown("Jump") && IsGrounded() && !isSlowDownActive)
        {
            animator.SetBool("isJumping", !IsGrounded());
            Jump();
        }

        CheckSpeedBoost();
    }

    private void FixedUpdate()
    {
        if (!movementEnabled) return;

        ApplyMovement();
        ApplyFriction();

        animator.SetFloat("xVelocity", Mathf.Abs(rb.linearVelocity.x));
        animator.SetFloat("yVelocity", rb.linearVelocity.y);

        if (IsGrounded())
        {
            animator.SetBool("isJumping", false);
        }

        if (isSpeedBoosted) {
            animator.SetFloat("isSpeedBoosted", 1f);
        } else {
            animator.SetFloat("isSpeedBoosted", 0f);
        }
    }

    #region Movement Methods
    private void ApplyMovement()
    {
        // Calculate the effective maximum speed.
        float effectiveSpeed = (isSlowDownActive ? groundSpeed * slowdownFactor : groundSpeed) * speedBoostMultiplier;
        // Apply acceleration over fixed time to ensure frame-rate independence.
        float newVelocityX = Mathf.Clamp(rb.linearVelocity.x + horizontalInput * acceleration * Time.fixedDeltaTime,
                                         -effectiveSpeed, effectiveSpeed);
        rb.linearVelocity = new Vector2(newVelocityX, rb.linearVelocity.y);

        // Flip sprite based on movement direction.
        if (Mathf.Abs(horizontalInput) > 0.1f)
        {
            Vector3 scale = transform.localScale;
            scale.x = Mathf.Abs(scale.x) * Mathf.Sign(horizontalInput);
            transform.localScale = scale;
        }
    }

    private void Jump()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpSpeed);
        animator.SetBool("isJumping", true);
    }

    private void ApplyFriction()
    {
        // Only apply friction when grounded and there is no horizontal input.
        if (IsGrounded() && Mathf.Approximately(horizontalInput, 0f))
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x * groundDecay, rb.linearVelocity.y);
        }
    }

    private bool IsGrounded()
    {
        if (groundCheck == null) return false;
        // Use OverlapBox to detect ground within the bounds of the groundCheck collider.
        Collider2D hit = Physics2D.OverlapBox(groundCheck.bounds.center, groundCheck.bounds.size, 0f, groundMask);
        return hit != null;
    }
    #endregion

    #region Speed Boost Methods
    private void CheckSpeedBoost()
    {
        if (isSpeedBoosted && Time.time >= speedBoostEndTime)
        {
            isSpeedBoosted = false;
            speedBoostMultiplier = 1f;
        }
    }

    /// <summary>
    /// Call this method to activate a temporary speed boost.
    /// </summary>
    /// <param name="multiplier">The factor by which to multiply the max speed.</param>
    /// <param name="duration">Duration of the boost in seconds.</param>
    public void ActivateSpeedBoost(float multiplier, float duration)
    {
        isSpeedBoosted = true;
        speedBoostMultiplier = multiplier;
        speedBoostEndTime = Time.time + duration;
    }
    #endregion

    #region Movement Enable/Disable & Slowdown State
    /// <summary>
    /// Enables movement input and physics.
    /// </summary>
    public void EnableMovement() => movementEnabled = true;

    /// <summary>
    /// Disables movement and resets input and velocity.
    /// </summary>
    public void DisableMovement()
    {
        movementEnabled = false;
        horizontalInput = 0f;
        rb.linearVelocity = Vector2.zero;
    }

    /// <summary>
    /// Sets the slowdown state (for example, when in a trap).
    /// </summary>
    /// <param name="active">True if slowdown is active; otherwise false.</param>
    public void SetSlowDown(bool active) => isSlowDownActive = active;
    #endregion

    #region Debugging
    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(groundCheck.bounds.center, groundCheck.bounds.size);
        }
    }
    #endregion
}
