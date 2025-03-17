using UnityEngine;

public class Throwable : MonoBehaviour
{
    [SerializeField] private float gravityScale = 1f;
    
    private Rigidbody2D rb;
    private Collider2D physicsCollider;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        physicsCollider = GetComponentInChildren<Collider2D>();
        
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }
        
        if (physicsCollider == null)
        {
            physicsCollider = gameObject.AddComponent<CircleCollider2D>();
        }
        
        // Configure physics properties
        rb.gravityScale = 0; // Start with no gravity
        rb.constraints = RigidbodyConstraints2D.FreezeRotation; // Optional: prevent rotation
    }

    public void PickUp()
    {
        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Kinematic; // Make kinematic when picked up
    }
    
    public void Launch(Vector2 direction, float force)
    {
        transform.SetParent(null);
        
        // Re-enable physics
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = gravityScale;
        
        // Apply the force
        rb.linearVelocity = direction * force;
    }
    
    public float GetGravity()
    {
        return gravityScale * Physics2D.gravity.magnitude;
    }
}