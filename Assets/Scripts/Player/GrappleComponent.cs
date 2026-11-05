using UnityEngine;

/// <summary>
/// The component that allows the object to use the grapple mechanics.
/// </summary>
public class GrappleComponent : MonoBehaviour
{
    public enum GrappleState
    {
        Idle,
        Hooking,
        Hooked
    }
    public GrappleState currentState = GrappleState.Idle;

    [Header("Physics")]
    public Rigidbody2D rb;
    public SpringJoint2D springJoint;
    public float grappleDrag = 0.9f;
    public float grappleJumpMultiplier = 0.9f;

    [Header("Visuals")]
    public LineRenderer lineRenderer;

    [Header("Grapple Settings")]
    public float maxGrappleDistance = 15f;
    public float reelInSpeed = 5f;
    public float reelOutSpeed = 2f;
    public float swingForce = 2f;
    public LayerMask grappleLayer;

    [Header("Hook Travel Settings")]
    public float hookSpeed = 40f;

    private float currentGrappleLength;
    private float currentMaxGrappleLength;
    private Vector2 aimDirection;
    private Vector2 hookTipPosition;
    private Vector2 hookTravelDirection;
    private bool wasGrappledAbove;

    /// <summary>
    /// Initialize the lineRenderer (TODO: this is temporary until we have a sprite for the fishing line)
    /// </summary>
    void Start()
    {
        if (lineRenderer != null) lineRenderer.enabled = false;
    }

    /// <summary>
    /// Update the lineRenderer's start and end points each frame
    /// </summary>
    void Update()
    {
        if (lineRenderer == null) return;

        if (currentState == GrappleState.Hooked)
        {
            lineRenderer.SetPosition(0, transform.position);
            lineRenderer.SetPosition(1, springJoint.connectedAnchor);
        }
        else if (currentState == GrappleState.Hooking)
        {
            lineRenderer.SetPosition(0, transform.position);
            lineRenderer.SetPosition(1, hookTipPosition);
        }
    }

    /// <summary>
    /// Update the spring joint distance and log relevant debug information
    /// </summary>
    void FixedUpdate()
    {
        DebugViewer.Log("Grapple State", currentState.ToString());
        DebugViewer.Log("Aim Direction", aimDirection.ToString("F2"));
        
        if (currentState == GrappleState.Hooked)
        {
            DebugViewer.Log("Rope Length", currentGrappleLength.ToString("F1"));
            SetSpringJointDistance();
        }
        else if (currentState == GrappleState.Hooking)
        {
            UpdateHookTravel();
        }
        else
        {
            DebugViewer.RemoveLog("Rope Length");
        }
    }

    /// <summary>
    /// Updates the spring joint based on the distance between the player and the grapple point
    /// If the player is closer than the current grapple length, the spring joint is disabled.
    /// This enables the player to walk over the joint without it pushing on the player.
    /// </summary>
    public void SetSpringJointDistance()
    {
        if (currentState != GrappleState.Hooked) return;

        float actualDist = Vector2.Distance(rb.position, springJoint.connectedAnchor);
        DebugViewer.Log("Actual Distance", actualDist.ToString("F1"));

        if(actualDist > currentGrappleLength)
        {
            springJoint.distance = Mathf.Min(actualDist, currentGrappleLength);
            springJoint.enabled = true;
        }
        else
        {
            springJoint.enabled = false;
        }
    }

    /// <summary>
    /// Sets the aim direction for the grapple component.
    /// </summary>
    /// <param name="direction">The direction in which the player is aiming.</param>
    public void SetAimDirection(Vector2 direction)
    {
        aimDirection = direction;
    }

    /// <summary>
    /// Checks if the grapple tension is currently active.
    /// </summary>
    /// <returns>True if the grapple tension is active, false otherwise.</returns>
    public bool IsGrappleTensionActive()
    {
        if (currentState != GrappleState.Hooked) return false;

        return Vector2.Distance(rb.position, springJoint.connectedAnchor) >= currentGrappleLength;
    }

    /// <summary>
    /// Applies a horizontal swing force to the player when hooked.
    /// </summary>
    /// <param name="hInput">The horizontal input to apply the swing force.</param>
    public void ApplySwingForce(float hInput)
    {
        if(currentState == GrappleState.Hooked && Mathf.Abs(hInput) > 0.1f)
        {
            rb.AddForce(new Vector2(hInput * swingForce, 0f));
        }
    }

    /// <summary>
    /// Handles the hook state transitions for the grapple component.
    /// </summary>
    public void HookHandler()
    {
        switch(currentState)
        {
            case GrappleState.Idle:
                Hook();
                break;
            case GrappleState.Hooking:
            case GrappleState.Hooked:
                Unhook();
                break;
        }
    }

    /// <summary>
    /// Hooks the grapple to a surface in the aim direction.
    /// This just uses a simple raycast at the moment.
    /// TODO: In the future, there should be some sort of animation of the line going out.
    /// </summary>
    public void Hook()
    {
        if (aimDirection == Vector2.zero) return;

        hookTravelDirection = aimDirection.normalized;
        hookTipPosition = transform.position;
        currentState = GrappleState.Hooking;

        if (lineRenderer != null) lineRenderer.enabled = true;
    }

    /// <summary>
    /// Attaches the grapple to a specific point
    /// </summary>
    /// <param name="point">The point to attach the grapple to.</param>
    private void AttachHook(Vector2 point)
    {
        springJoint.connectedAnchor = point;
        currentGrappleLength = Vector2.Distance(transform.position, point);
        currentMaxGrappleLength = currentGrappleLength;
        springJoint.distance = currentGrappleLength;
        springJoint.enabled = true;

        wasGrappledAbove = point.y > rb.position.y;
        currentState = GrappleState.Hooked;
    }

    /// <summary>
    /// Updates the hook's position each frame
    /// </summary>
    private void UpdateHookTravel()
    {
        float hookDistance = hookSpeed * Time.fixedDeltaTime;

        RaycastHit2D hit = Physics2D.Raycast(hookTipPosition, hookTravelDirection, hookDistance, grappleLayer);
        if(hit.collider != null)
        {
            AttachHook(hit.point);
            return;
        }

        hookTipPosition += hookTravelDirection * hookDistance;

        if (Vector2.Distance(transform.position, hookTipPosition) > maxGrappleDistance)
        {
            Unhook();
        }
    }

    /// <summary>
    /// Unhooks the grapple from the surface and resets its state.
    /// </summary>
    public void Unhook()
    {
        currentState = GrappleState.Idle;

        currentGrappleLength = 0;
        currentMaxGrappleLength = 0;
        wasGrappledAbove = false;

        if(lineRenderer != null) lineRenderer.enabled = false;
        springJoint.enabled = false;
    }

    /// <summary>
    /// Shorten the distance of the grapple
    /// </summary>
    public void ReelIn()
    {
        if(currentState != GrappleState.Hooked) return;

        currentGrappleLength -= reelInSpeed * Time.deltaTime;

        currentGrappleLength = Mathf.Clamp(currentGrappleLength, 0f, currentMaxGrappleLength);
    }

    /// <summary>
    /// Lengthen the distance of the grapple
    /// </summary>
    public void ReelOut()
    {
        if(currentState != GrappleState.Hooked) return;

        currentGrappleLength += reelOutSpeed * Time.deltaTime;

        currentGrappleLength = Mathf.Clamp(currentGrappleLength, 0f, currentMaxGrappleLength);
    }

    /// <summary>
    /// Checks if the grapple was hooked above the player.
    /// </summary>
    /// <returns></returns>
    public bool WasGrappledAbove()
    {
        return wasGrappledAbove;
    }
}
