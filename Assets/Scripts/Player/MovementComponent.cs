using UnityEditor.U2D.Aseprite;
using UnityEngine;

/// <summary>
/// Provides movement capabilities for the object
/// </summary>
public class MovementComponent : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private GrappleComponent grappleComponent;

    [Header("Movement")]
    public float moveSpeed = 5f;
    public float acceleration = 10f;
    public float deacceleration = 50f;
    public float airDrag = 0.1f;

    [Header("Jump")]
    public float jumpForce = 10f;
    [Range(0f, 1f)] public float jumpCancelMultiplier = 0.9f;
    public float jumpBufferMaxTime = 0.1f;
    private float jumpBufferTimer;

    [Header("Coyote Time")]
    public float coyoteMaxTime = 0.1f;
    private float coyoteTimer;

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
    /// Updates Timers every frame
    /// </summary>
    void Update()
    {
        UpdateCoyoteTimer();
        UpdateJumpBufferTimer();
    }
    
    /// <summary>
    /// Whenever the player is not grounded, the coyote timer is decreased.
    /// We use this to replace any IsGrounded calls else where to use the coyote timer
    /// </summary>
    private void UpdateCoyoteTimer()
    {
        if (IsGrounded())
        {
            coyoteTimer = coyoteMaxTime;
        }
        else
        {
            coyoteTimer -= Time.deltaTime;
        }
    }

    /// <summary>
    /// Having a jump buffer timer allows the player to press the jump button slightly
    /// early and have it still work.
    /// </summary>
    private void UpdateJumpBufferTimer()
    {
        if(jumpBufferTimer > 0)
        {
            jumpBufferTimer -= Time.deltaTime;
        }

        if (jumpBufferTimer > 0f && coyoteTimer > 0f)
        {
            ExecuteJump();
        }
    }

    /// <summary>
    /// Move the character horizontally using provided input.
    /// If the character is currently grappled, the movement will be influenced by the grapple mechanics.
    /// TODO: Clean this and put in multiple modular methods
    /// </summary>
    /// <param name="hInput">The horizontal input value. -1 is full left, 1 is full right.</param>
    public void Move(float hInput)
    {
        if (!canMove) return;

        float hVelocity = 0;

        if(grappleComponent && grappleComponent.IsGrappleTensionActive())
        {
            if(!IsGrounded())
            {
                grappleComponent.ApplySwingForce(hInput);
                return;
            }

            if (grappleComponent.IsGrappleTensionActive())
            {
                float dirToAnchor = Mathf.Sign(grappleComponent.springJoint.connectedAnchor.x - rb.position.x);

                if(Mathf.Abs(hInput) < 0.1f || Mathf.Sign(hInput) != dirToAnchor)
                {
                    rb.linearVelocity = new Vector2(rb.linearVelocity.x * grappleComponent.grappleDrag, rb.linearVelocity.y);
                    return;
                }
            }
        }

        float targetSpeed = hInput * moveSpeed;
        float currentHSpeed = rb.linearVelocity.x;

        if (!IsGrounded() && Mathf.Abs(currentHSpeed) > Mathf.Abs(targetSpeed))
        {
            if (hInput == 0 || Mathf.Sign(hInput) == Mathf.Sign(currentHSpeed))
            {
                float hVelocityDecay = Mathf.MoveTowards(currentHSpeed, targetSpeed, airDrag * Time.fixedDeltaTime);
                rb.linearVelocity = new Vector2(hVelocityDecay, rb.linearVelocity.y);
                return;
            }
        }

        float accelerationRate = Mathf.Abs(targetSpeed) > 0.1f ? acceleration : deacceleration;

        hVelocity = Mathf.MoveTowards(rb.linearVelocity.x, targetSpeed, accelerationRate * Time.fixedDeltaTime);
        rb.linearVelocity = new Vector2(hVelocity, rb.linearVelocity.y);

        if (Mathf.Abs(hInput) > 0.1f)
        {
            facingDirection = Mathf.Sign(hInput);
        }
    }

    /// <summary>
    /// Causes the character to jump if grounded, else we start the jump buffer timer
    /// </summary>
    public void Jump()
    {
        if (!canMove) return;

        jumpBufferTimer = jumpBufferMaxTime;

        if(coyoteTimer > 0f)
        {
            ExecuteJump();
        }
        else if (grappleComponent && grappleComponent.currentState is GrappleComponent.GrappleState.Hooked)
        {
            if(grappleComponent.springJoint.connectedAnchor.y > rb.position.y)
            {
                ExecuteJump();
                grappleComponent.Unhook();
            }
        }
    }

    /// <summary>
    /// Simply update the linear velocity of the character. 
    /// This approach may be limited later if the jump becomes more complex
    /// </summary>
    private void ExecuteJump()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        jumpBufferTimer = 0f;
        coyoteTimer = 0f;
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