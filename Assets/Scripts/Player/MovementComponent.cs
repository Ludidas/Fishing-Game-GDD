using UnityEditor.U2D.Aseprite;
using UnityEngine;

public class MovementComponent : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float acceleration = 10f;
    public float deacceleration = 50f;

    [Header("Jump")]
    public float jumpForce = 10f;
    [Range(0f, 1f)] public float jumpCancelMultiplier = 0.9f;

    [Header("Rigidbody")]
    public Rigidbody2D rb;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundCheckWidth = 1f;
    public float groundCheckHeight = 0.2f;
    public LayerMask groundLayer;


    private bool canMove = true;
    private bool isGrounded;
    private float facingDirection = 1f;

    /// <summary>
    /// Move the character horizontally using provided input
    /// </summary>
    /// <param name="hInput">The horizontal input value. -1 is full left, 1 is full right.</param>
    public void Move(float hInput)
    {
        if (!canMove) return;

        float targetSpeed = hInput * moveSpeed;
        float accelerationRate = Mathf.Abs(targetSpeed) > 0.1f ? acceleration : deacceleration;

        float hVelocity = Mathf.MoveTowards(rb.linearVelocity.x, targetSpeed, accelerationRate * Time.fixedDeltaTime);
        rb.linearVelocity = new Vector2(hVelocity, rb.linearVelocity.y);

        if (Mathf.Abs(hInput) > 0.1f)
        {
            facingDirection = Mathf.Sign(hInput);
        }
    }

    /// <summary>
    /// Simply update the linear velocity of the character. 
    /// This approach may be limited later if the jump becomes more complex
    /// </summary>
    public void Jump()
    {
        if (!canMove) return;

        if (IsGrounded())
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }
    }

    /// <summary>
    /// Cancel the jump using a multiplier to make it smoother
    /// </summary>
    public void CancelJump()
    {
        if (rb.linearVelocity.y > 0)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * jumpCancelMultiplier);
        }
    }

    /// <summary>
    /// Checks if a collider is intersecting with a ground layer object
    /// </summary>
    /// <returns>True if grounded, false otherwise</returns>
    public bool IsGrounded()
    {
        Collider2D hit = Physics2D.OverlapBox(groundCheck.position, new Vector2(groundCheckWidth, groundCheckHeight), 0f, groundLayer);
        return hit != null;
    }

    /// <summary>
    /// Enables character movement
    /// </summary>
    public void EnableMovement()
    {
        canMove = true;
    }

    /// <summary>
    /// Disable character movement
    /// </summary>
    public void DisableMovement()
    {
        canMove = false;

        rb.linearVelocity = new Vector2(0f, 0f);
    }
}