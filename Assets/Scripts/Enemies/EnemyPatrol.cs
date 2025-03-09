using UnityEngine;

public class EnemyPatrol : MonoBehaviour
{
    public GameObject pointA;
    public GameObject pointB;

    private Rigidbody2D rb;

    private Transform currentPoint;

    public float speed;

    public bool isMoving;

    public float collisionRadius = 0.5f;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        currentPoint = pointB.transform;
        isMoving = true;
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 point = currentPoint.position - transform.position;
        if (currentPoint.position == pointB.transform.position)
        {
            rb.linearVelocity = new Vector2(speed, 0);
        }
        else
        {
            rb.linearVelocity = new Vector2(-speed, 0);
        }

        if (Vector2.Distance(transform.position, currentPoint.position) < collisionRadius && currentPoint == pointB.transform)
        {
            Flip();
            currentPoint = pointA.transform;
        }
        if (Vector2.Distance(transform.position, currentPoint.position) < collisionRadius && currentPoint == pointA.transform)
        {
            Flip();
            currentPoint = pointB.transform;
        }
    }

    private void Flip() {
        Vector3 localScale = transform.localScale;
        localScale.x *= -1;
        transform.localScale = localScale;
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(pointA.transform.position, collisionRadius);
        Gizmos.DrawWireSphere(pointB.transform.position, collisionRadius);
        Gizmos.DrawLine(pointA.transform.position, pointB.transform.position);
    }
}
