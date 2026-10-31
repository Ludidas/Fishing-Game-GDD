using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Handles input from the player
/// </summary>
public class PlayerInput : MonoBehaviour
{
    private PlayerControls playerControls;

    public Camera mainCam;

    [Header("Components")]
    [SerializeField]
    private MovementComponent movementComponent;
    [SerializeField]
    private GrappleComponent grappleComponent;

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

        if (grappleComponent == null)
        {
            grappleComponent = GetComponent<GrappleComponent>();
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

        playerControls.Player.Hook.performed += OnHookPerformed;

        playerControls.Player.Melee.performed += OnMeleePerformed;
    }

    /// <summary>
    /// Disconnect the input actions to avoid memory leaks
    /// </summary>
    void OnDisable()
    {
        playerControls.Disable();

        playerControls.Player.Jump.performed -= OnJumpPerformed;
        playerControls.Player.Jump.canceled -= OnJumpCanceled;

        playerControls.Player.Hook.performed -= OnHookPerformed;

        playerControls.Player.Melee.performed -= OnMeleePerformed;
    }

    /// <summary>
    /// Updates every frame
    /// </summary>
    void Update()
    {
        HandleMovementInput();
        HandleGrappleInput();
        HandleReelInput();
    }

    /// <summary>
    /// Read the input to give to the movement component
    /// </summary>
    void HandleMovementInput()
    {
        if (movementComponent == null) return;

        Vector2 moveInput = playerControls.Player.Move.ReadValue<Vector2>();
        float hInput = moveInput.x;
        if (Mathf.Abs(hInput) < 0.1f)
        {
            hInput = 0f;
        }

        movementComponent?.Move(hInput);
    }

    /// <summary>
    /// Sets the aim direction based on the input.
    /// If gamepad has been detected last, it will override the mouse position
    /// </summary>
    void HandleGrappleInput()
    {
        if(mainCam == null) return;

        Vector2 aimInput = playerControls.Player.Aim.ReadValue<Vector2>();
        Vector2 aimDirection = Vector2.zero;

        InputDevice activeDevice = playerControls.Player.Aim.activeControl?.device;

        if (activeDevice == null) return;

        if(activeDevice is Gamepad)
        {
            if(aimInput.sqrMagnitude > 0.1f)
            {
                aimDirection = aimInput.normalized;
            }
        }
        else
        {
            Vector3 mouseWorldPosition = mainCam.ScreenToWorldPoint(new Vector3(aimInput.x, aimInput.y, mainCam.nearClipPlane));
            aimDirection = (mouseWorldPosition - transform.position).normalized;
        }

        grappleComponent?.SetAimDirection(aimDirection);
    }

    void HandleReelInput()
    {
        if(grappleComponent == null) return;

        float reelInput;
        float reelInInput = playerControls.Player.ReelIn.ReadValue<float>();
        float reelOutInput = playerControls.Player.ReelOut.ReadValue<float>();
        reelInput = reelInInput - reelOutInput;

        if(reelInput > 0)
        {
            grappleComponent?.ReelIn();
        }
        else if(reelInput < 0)
        {
            grappleComponent?.ReelOut();
        }
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

    /// <summary>
    /// Call the grapple component to hook or unhook from a surface
    /// </summary>
    /// <param name="context"></param>
    void OnHookPerformed(InputAction.CallbackContext context)
    {
        grappleComponent?.HookHandler();
    }

    void OnMeleePerformed(InputAction.CallbackContext context)
    {
        //TODO
    }
}
