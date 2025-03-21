using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyPatrol : MonoBehaviour
{
    [Header("Patrol Settings")]
    [SerializeField] private Transform pointA;
    [SerializeField] private Transform pointB;
    [SerializeField] private float speed = 2f;
    [SerializeField] private float reachThreshold = 0.1f;

    [Header("Player Detection Settings")]
    [SerializeField] private float detectionRange = 5f;
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private string defeatMessage = "CAUGHT BY GUARD!";
    [SerializeField] private float pathMarginX = 0.5f;
    [SerializeField] private float pathMarginY = 1.0f;

    private Rigidbody2D rb;
    private Transform currentTarget;
    private Vector3 originalPosition;
    private Transform playerTransform;
    private bool playerCaught = false;

    private Animator animator;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        rb.constraints = RigidbodyConstraints2D.FreezeRotation | RigidbodyConstraints2D.FreezePositionY;

        currentTarget = pointB;
        originalPosition = transform.position;

        GameObject playerObj = GameObject.FindGameObjectWithTag(playerTag).transform.parent.gameObject;
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
        }
    }

    private void Update()
    {
        if ((GameManager.Instance != null && GameManager.Instance.isGameOver) || playerCaught)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        Patrol();
        DetectPlayer();

        animator.SetFloat("xVelocity", Mathf.Abs(rb.linearVelocity.x));
    }

    private void Patrol()
    {
        float direction = Mathf.Sign(currentTarget.position.x - transform.position.x);
        rb.linearVelocity = new Vector2(direction * speed, rb.linearVelocity.y);

        if (Mathf.Abs(direction) > 0.01f)
        {
            Vector3 scale = transform.localScale;
            scale.x = Mathf.Abs(scale.x) * (direction > 0 ? 1 : -1);
            transform.localScale = scale;
        }

        if (Mathf.Abs(transform.position.x - currentTarget.position.x) <= reachThreshold)
        {
            currentTarget = (currentTarget == pointB) ? pointA : pointB;
            Flip();
        }

        if (Mathf.Abs(transform.position.y - originalPosition.y) > 0.01f)
        {
            Vector3 fixedPos = transform.position;
            fixedPos.y = originalPosition.y;
            transform.position = fixedPos;
        }
    }

    private void DetectPlayer()
    {
        if (playerTransform == null) return;

        if (playerTransform.GetComponent<PlayerInteractionController>().isHidden) return;

        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);
        if (distanceToPlayer <= detectionRange && IsPlayerInPatrolPath() && IsFacingPlayer())
        {
            CatchPlayer();
        }
    }

    private bool IsPlayerInPatrolPath()
    {
        float minX = Mathf.Min(pointA.position.x, pointB.position.x) - pathMarginX;
        float maxX = Mathf.Max(pointA.position.x, pointB.position.x) + pathMarginX;
        float minY = Mathf.Min(pointA.position.y, pointB.position.y) - pathMarginY;
        float maxY = Mathf.Max(pointA.position.y, pointB.position.y) + pathMarginY;

        Vector3 playerPos = playerTransform.position;
        return playerPos.x >= minX && playerPos.x <= maxX && playerPos.y >= minY && playerPos.y <= maxY;
    }

    private bool IsFacingPlayer()
    {
        bool enemyFacingRight = transform.localScale.x > 0;
        bool playerIsRight = playerTransform.position.x > transform.position.x;
        return enemyFacingRight == playerIsRight;
    }

    private void CatchPlayer()
    {
        if (playerCaught) return;

        playerCaught = true;
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
        if (pointA != null && pointB != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(pointA.position, reachThreshold);
            Gizmos.DrawWireSphere(pointB.position, reachThreshold);
            Gizmos.DrawLine(pointA.position, pointB.position);
        }

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}
