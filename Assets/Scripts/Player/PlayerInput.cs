using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInput : MonoBehaviour
{
    private PlayerControls playerControls;

    [Header("Components")]
    [SerializeField]
    private MovementComponent movementComponent;

    /// <summary>
    /// Initialize the Input Handler
    /// </summary>
    void Awake()
    {
        playerControls = new PlayerControls();

        if (movementComponent == null)
        {
            movementComponent = GetComponent<MovementComponent>();
        }
    }

    /// <summary>
    /// Connect the input actions to the correct movement signals
    /// </summary>
    void OnEnable()
    {
        playerControls.Enable();

        playerControls.Player.Jump.performed += OnJumpPerformed;

        playerControls.Player.Jump.canceled += OnJumpCanceled;
    }

    /// <summary>
    /// Disconnect the input actions to avoid memory leaks
    /// </summary>
    void OnDisable()
    {
        playerControls.Disable();

        playerControls.Player.Jump.performed -= OnJumpPerformed;
        playerControls.Player.Jump.canceled -= OnJumpCanceled;
    }

    /// <summary>
    /// Updates every frame
    /// </summary>
    void Update()
    {
        HandleMovementInput();
    }

    /// <summary>
    /// Read the input to give to the movement component
    /// </summary>
    void HandleMovementInput()
    {
        if (movementComponent == null) return;

        Vector2 moveInput = playerControls.Player.Move.ReadValue<Vector2>();
        float hInput = moveInput.x;

        movementComponent.Move(hInput);
    }

    /// <summary>
    /// Call the movememnt component jump function when jump is pressed
    /// </summary>
    /// <param name="context"></param>
    void OnJumpPerformed(InputAction.CallbackContext context)
    {
        movementComponent?.Jump();
    }

    /// <summary>
    /// Call the movement component cancel jump function when jump is released
    /// </summary>
    /// <param name="context"></param>
    void OnJumpCanceled(InputAction.CallbackContext context)
    {
        movementComponent?.CancelJump();
    }
}
