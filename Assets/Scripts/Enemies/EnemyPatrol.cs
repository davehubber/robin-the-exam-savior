using UnityEngine;

public class EnemyPatrol : MonoBehaviour
{
    public GameObject pointA;
    public GameObject pointB;

    private Rigidbody2D rb;
    private Transform currentPoint;
    private Vector3 originalY;

    public float speed;
    public bool isMoving;
    public float collisionRadius = 0.5f;

    [Header("Player Detection")]
    public float detectionRange = 5f;    // How far the enemy can see
    public LayerMask playerLayer;        // Layer that contains the player
    public string playerTag = "Player";  // Tag of the player object
    public string hiddenLayerName = "HiddenPlayer"; // Layer for hidden players
    public string defeatMessage = "CAUGHT BY GUARD!"; // Custom message when player is caught

    private Transform playerTransform;
    private bool playerDetected = false;
    private int hiddenLayer;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        currentPoint = pointB.transform;
        isMoving = true;
        
        // Store original Y position
        originalY = transform.position;

        // Find player reference
        GameObject player = GameObject.FindGameObjectWithTag(playerTag);
        if (player != null)
        {
            playerTransform = player.transform;
        }
        
        // Get hidden layer
        hiddenLayer = LayerMask.NameToLayer(hiddenLayerName);
        
        // Ensure rigidbody constraints are set properly
        if (rb != null)
        {
            rb.constraints = RigidbodyConstraints2D.FreezeRotation | RigidbodyConstraints2D.FreezePositionY;
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Skip updates if game is already over
        if (GameManager.Instance != null && GameManager.Instance.isGameOver)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        // Handle patrol movement
        if (isMoving)
        {
            HandlePatrol();
        }

        // Check for player detection
        DetectPlayer();
    }

    private void HandlePatrol()
    {
        // Set linearVelocity based on current target point, but only on X axis
        if (currentPoint.position == pointB.transform.position)
        {
            rb.linearVelocity = new Vector2(speed, 0);
        }
        else
        {
            rb.linearVelocity = new Vector2(-speed, 0);
        }

        // Check if reached destination point
        Vector2 currentPos2D = new Vector2(transform.position.x, transform.position.y);
        Vector2 targetPos2D = new Vector2(currentPoint.position.x, currentPoint.position.y);
        
        float distanceX = Mathf.Abs(currentPos2D.x - targetPos2D.x);
        
        if (distanceX < collisionRadius)
        {
            if (currentPoint == pointB.transform)
            {
                Flip();
                currentPoint = pointA.transform;
            }
            else if (currentPoint == pointA.transform)
            {
                Flip();
                currentPoint = pointB.transform;
            }
        }
        
        // Ensure Y position stays constant
        if (Mathf.Abs(transform.position.y - originalY.y) > 0.01f)
        {
            Vector3 fixedPosition = transform.position;
            fixedPosition.y = originalY.y;
            transform.position = fixedPosition;
        }
    }

    private void DetectPlayer()
    {
        // Skip detection if player reference is missing
        if (playerTransform == null) return;
        print(playerTransform);
        // Skip detection if player is hiding
        if (playerTransform.gameObject.layer == hiddenLayer) return;

        // Calculate distance to player
        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);

        // Check if player is within detection range
        if (distanceToPlayer <= detectionRange)
        {
            // Check if player is in the path between pointA and pointB
            bool playerInPath = IsPointInPath(playerTransform.position);

            if (playerInPath)
            {
                // Check if enemy is facing the player (based on relative x position and local scale)
                bool isFacingPlayer = IsFacingPlayer();

                if (isFacingPlayer)
                {
                    // Player has been spotted and is in the enemy's line of sight
                    if (!playerDetected)
                    {
                        playerDetected = true;
                        CatchPlayer();
                    }
                }
            }
        }
    }

    private bool IsPointInPath(Vector3 point)
    {
        // Create a slightly expanded rect/area between pointA and pointB to represent the patrol path
        float minX = Mathf.Min(pointA.transform.position.x, pointB.transform.position.x) - 0.5f;
        float maxX = Mathf.Max(pointA.transform.position.x, pointB.transform.position.x) + 0.5f;
        float minY = Mathf.Min(pointA.transform.position.y, pointB.transform.position.y) - 1.0f;
        float maxY = Mathf.Max(pointA.transform.position.y, pointB.transform.position.y) + 1.0f;

        // Check if the point is within this rectangular area
        return (point.x >= minX && point.x <= maxX && point.y >= minY && point.y <= maxY);
    }

    private bool IsFacingPlayer()
    {
        bool enemyFacingRight = transform.localScale.x > 0;
        bool playerIsRightOfEnemy = playerTransform.position.x > transform.position.x;

        // Enemy is facing the player if both conditions match
        return (enemyFacingRight == playerIsRightOfEnemy);
    }

    private void CatchPlayer()
    {
        // Access player controller and disable controls
        PlayerController playerController = playerTransform.GetComponent<PlayerController>();
        if (playerController != null)
        {
            playerController.DisableControls();
        }

        // Call game over with custom message
        if (GameManager.Instance != null)
        {
            GameManager.Instance.GameOver(false, defeatMessage);
        }

        // Stop the enemy movement
        isMoving = false;
        rb.linearVelocity = Vector2.zero;
    }

    private void Flip()
    {
        Vector3 localScale = transform.localScale;
        localScale.x *= -1;
        transform.localScale = localScale;
    }

    private void OnDrawGizmos()
    {
        // Draw patrol path
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(pointA.transform.position, collisionRadius);
        Gizmos.DrawWireSphere(pointB.transform.position, collisionRadius);
        Gizmos.DrawLine(pointA.transform.position, pointB.transform.position);

        // Draw detection range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}