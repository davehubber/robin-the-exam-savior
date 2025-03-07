using UnityEngine;

public class Throwable : MonoBehaviour
{
    public float mass = 1f;
    private Rigidbody2D rb;
    private Collider2D coll;
    private bool isPickedUp = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        coll = GetComponent<Collider2D>();
        
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
            rb.mass = mass;
        }
        
        if (coll == null)
        {
            gameObject.AddComponent<CircleCollider2D>();
        }
    }

    public void PickUp()
    {
        isPickedUp = true;
        rb.simulated = false; // Disable physics when picked up
    }
    
    public void Release()
    {
        isPickedUp = false;
        rb.simulated = true; // Re-enable physics when thrown
    }

    public void Throw(Vector2 force)
    {
        Release();
        rb.AddForce(force, ForceMode2D.Impulse);
    }

    public bool IsPickedUp() {
        return isPickedUp;
    }
}
