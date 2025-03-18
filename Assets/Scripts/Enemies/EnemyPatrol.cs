using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyPatrol : MonoBehaviour
{
    #region Patrol Settings
    [Header("Patrol Settings")]
    [SerializeField] private Transform pointA;
    [SerializeField] private Transform pointB;
    [SerializeField] private float speed = 2f;
    [SerializeField] private float reachThreshold = 0.1f; // How close before switching target
    #endregion

    #region Player Detection Settings
    [Header("Player Detection Settings")]
    [SerializeField] private float detectionRange = 5f;
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private string defeatMessage = "CAUGHT BY GUARD!";
    [SerializeField] private float pathMarginX = 0.5f; // Extra margin around patrol path (X axis)
    [SerializeField] private float pathMarginY = 1.0f; // Extra margin around patrol path (Y axis)

    #endregion

    private Rigidbody2D rb;
    private Transform currentTarget;
    private Vector3 originalPosition;
    private Transform playerTransform;
    private MovementController playerMovementController;
    private bool playerCaught = false;

    private Animator animator;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        if (rb != null)
        {
            // Freeze rotation and keep Y position fixed for horizontal patrol.
            rb.constraints = RigidbodyConstraints2D.FreezeRotation | RigidbodyConstraints2D.FreezePositionY;
        }

        if (pointA == null || pointB == null)
        {
            Debug.LogError("EnemyPatrol: Patrol points are not assigned.");
        }
        else
        {
            currentTarget = pointB;
        }
        
        originalPosition = transform.position;

        // Locate the player by tag.
        GameObject playerObj = GameObject.FindGameObjectWithTag(playerTag).transform.parent.gameObject;
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
            playerMovementController = playerObj.GetComponent<MovementController>();
        }
        else
        {
            Debug.LogError("EnemyPatrol: Player not found with tag " + playerTag);
        }
    }

    private void Update()
    {
        // Stop patrolling if the game is over or if the player has already been caught.
        if ((GameManager.Instance != null && GameManager.Instance.isGameOver) || playerCaught)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        Patrol();
        DetectPlayer();

        animator.SetFloat("xVelocity", Mathf.Abs(rb.linearVelocity.x));
    }

    #region Patrol Movement
    private void Patrol()
    {
        // Move horizontally towards the current target.
        float direction = Mathf.Sign(currentTarget.position.x - transform.position.x);
        rb.linearVelocity = new Vector2(direction * speed, rb.linearVelocity.y);

        // Flip sprite to face movement direction.
        if (Mathf.Abs(direction) > 0.01f)
        {
            Vector3 scale = transform.localScale;
            scale.x = Mathf.Abs(scale.x) * (direction > 0 ? 1 : -1);
            transform.localScale = scale;
        }

        // Check if the enemy has reached (or is close enough to) the target.
        if (Mathf.Abs(transform.position.x - currentTarget.position.x) <= reachThreshold)
        {
            currentTarget = (currentTarget == pointB) ? pointA : pointB;
            Flip();
        }

        // Ensure the enemy's Y position remains fixed.
        if (Mathf.Abs(transform.position.y - originalPosition.y) > 0.01f)
        {
            Vector3 fixedPos = transform.position;
            fixedPos.y = originalPosition.y;
            transform.position = fixedPos;
        }
    }
    #endregion

    #region Player Detection
    private void DetectPlayer()
    {
        if (playerTransform == null)
            return;
        
        // Optionally skip detection if the player is hidden.
        if (playerTransform.GetComponent<PlayerInteractionController>().isHidden)
            return;

        // Use overall distance for detection.
        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);
        if (distanceToPlayer <= detectionRange)
        {
            if (IsPlayerInPatrolPath() && IsFacingPlayer())
            {
                CatchPlayer();
            }
        }
    }

    private bool IsPlayerInPatrolPath()
    {
        // Define a rectangular patrol area between pointA and pointB with added margins.
        float minX = Mathf.Min(pointA.position.x, pointB.position.x) - pathMarginX;
        float maxX = Mathf.Max(pointA.position.x, pointB.position.x) + pathMarginX;
        float minY = Mathf.Min(pointA.position.y, pointB.position.y) - pathMarginY;
        float maxY = Mathf.Max(pointA.position.y, pointB.position.y) + pathMarginY;

        Vector3 playerPos = playerTransform.position;
        return playerPos.x >= minX && playerPos.x <= maxX && playerPos.y >= minY && playerPos.y <= maxY;
    }

    private bool IsFacingPlayer()
    {
        // Determine facing direction by local scale (assumes sprite is facing right by default).
        bool enemyFacingRight = transform.localScale.x > 0;
        bool playerIsRight = playerTransform.position.x > transform.position.x;
        return enemyFacingRight == playerIsRight;
    }
    #endregion

    private void CatchPlayer()
    {
        if (playerCaught)
            return;
        
        playerCaught = true;

        // Disable player's movement controls.
        if (playerMovementController != null)
        {
            playerMovementController.DisableMovement();
        }
        
        // Trigger game over with the defeat message.
        if (GameManager.Instance != null)
        {
            GameManager.Instance.GameOver(false, defeatMessage);
        }
    }

    private void Flip()
    {
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    private void OnDrawGizmos()
    {
        // Draw patrol points and path.
        if (pointA != null && pointB != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(pointA.position, reachThreshold);
            Gizmos.DrawWireSphere(pointB.position, reachThreshold);
            Gizmos.DrawLine(pointA.position, pointB.position);
        }
        
        // Draw detection range.
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}
