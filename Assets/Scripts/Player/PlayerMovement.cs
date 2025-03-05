using System;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
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
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start() {
        
    }

    // Update is called once per frame
    void Update() {
        if (currentPortal != null && Input.GetKeyDown(KeyCode.W)) {
            if (currentPortal.exitPoint != null) {
                transform.position = currentPortal.exitPoint.position;
            }
        }
        GetInput();
        HandleJump();
    }

    void FixedUpdate() {
        MoveWithInput();
        CheckGround();
        ApplyFriction();
    }

    void GetInput() {
        xInput = Input.GetAxis("Horizontal");
    }

    void MoveWithInput() {
        if (Mathf.Abs(xInput) > 0) {
            float increment = xInput * acceleration;
            float speed = isInTrap ? groundSpeed * slowdownFactor : groundSpeed;
            float newSpeed = Mathf.Clamp(body.linearVelocityX + increment, -speed, speed);
            body.linearVelocity = new Vector2(newSpeed, body.linearVelocityY);

            float direction = Mathf.Sign(xInput);
            transform.localScale = new Vector3(direction, 1, 1);
        }
    }


    void HandleJump() {
        if(Input.GetButtonDown("Jump") && grounded && !isInTrap) {
            body.linearVelocity = new Vector2(body.linearVelocityX, jumpSpeed);
        }
    }

    void CheckGround() {
        grounded = Physics2D.OverlapAreaAll(groundCheck.bounds.min, groundCheck.bounds.max, groundMask).Length > 0;
    }

    void ApplyFriction() {
        if (grounded && xInput == 0 && body.linearVelocityY <= 0) {
            body.linearVelocity *= groundDecay;
        }
    }

    // Detect portal triggers
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
}
