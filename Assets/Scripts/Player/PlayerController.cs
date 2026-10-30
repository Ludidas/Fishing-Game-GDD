using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 8f;
    [Tooltip("Upward speed applied the moment you jump.")]
    [SerializeField] private float jumpVelocity = 12f;
    [Tooltip("Upward speed is multiplied by this if jump is released early. Lower = shorter taps.")]
    [SerializeField, Range(0f, 1f)] private float jumpCutMultiplier = 0.5f;

    [Header("Ground Check")]
    [Tooltip("Empty child object placed at the player's feet.")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.15f;
    [SerializeField] private LayerMask groundLayer;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;

    private float moveInput;
    private bool jumpPressed;
    private bool jumpReleased;
    private bool isGrounded;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.freezeRotation = true;
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        if (groundCheck == null)
            Debug.LogWarning($"{name}: PlayerController has no Ground Check assigned, so jumping won't work.", this);
    }

    private void Update()
    {
        ReadInput();
        FlipSprite();
    }

    private void FixedUpdate()
    {
        isGrounded = groundCheck != null && Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        Vector2 velocity = rb.linearVelocity;
        velocity.x = moveInput * moveSpeed;

        if (jumpPressed && isGrounded)
            velocity.y = jumpVelocity;

        // Variable jump height: letting go early cuts the jump short
        if (jumpReleased && velocity.y > 0f)
            velocity.y *= jumpCutMultiplier;

        rb.linearVelocity = velocity;

        jumpPressed = false;
        jumpReleased = false;
    }

    private void ReadInput()
    {
        moveInput = 0f;
        Keyboard keyboard = Keyboard.current;
        Gamepad gamepad = Gamepad.current;

        if (keyboard != null)
        {
            if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) moveInput -= 1f;
            if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) moveInput += 1f;
            if (keyboard.spaceKey.wasPressedThisFrame) jumpPressed = true;
            if (keyboard.spaceKey.wasReleasedThisFrame) jumpReleased = true;
        }

        if (gamepad != null)
        {
            float stick = gamepad.leftStick.x.ReadValue();
            if (Mathf.Abs(stick) > 0.2f) moveInput = stick;
            if (gamepad.buttonSouth.wasPressedThisFrame) jumpPressed = true;
            if (gamepad.buttonSouth.wasReleasedThisFrame) jumpReleased = true;
        }
    }

    // Assumes the sprite faces right by default
    private void FlipSprite()
    {
        if (spriteRenderer == null || Mathf.Approximately(moveInput, 0f)) return;
        spriteRenderer.flipX = moveInput < 0f;
    }

    // Draws the ground check circle in the Scene view (green = grounded)
    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;
        Gizmos.color = isGrounded ? Color.green : Color.red;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}
