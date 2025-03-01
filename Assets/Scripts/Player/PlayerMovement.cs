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
    float yInput;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start() {
        
    }

    // Update is called once per frame
    void Update() {
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
        yInput = Input.GetAxis("Vertical");
    }

    void MoveWithInput() {
        if(Math.Abs(xInput) > 0) {
            float increment = xInput * acceleration;
            float newSpeed = Math.Clamp(body.linearVelocityX + increment, -groundSpeed, groundSpeed);
            body.linearVelocity = new Vector2(newSpeed, body.linearVelocityY);

            float direction = Math.Sign(xInput);
            transform.localScale = new Vector3(direction, 1, 1);
        }
    }

    void HandleJump() {
        if(Input.GetButtonDown("Jump") && grounded) {
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
}
