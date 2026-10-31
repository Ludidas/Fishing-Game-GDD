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

    [Header("Visuals")]
    public LineRenderer lineRenderer;

    [Header("Grapple Settings")]
    public float maxGrappleDistance = 15f;
    public float reelInSpeed = 5f;
    public float reelOutSpeed = 2f;
    public float swingForce = 2f;
    public LayerMask grappleLayer;

    private float currentGrappleLength;
    private float currentMaxGrappleLength;
    private Vector2 aimDirection;

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
        if (currentState == GrappleState.Hooked && lineRenderer)
        {
            lineRenderer.SetPosition(0, transform.position);
            lineRenderer.SetPosition(1, springJoint.connectedAnchor);
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

        RaycastHit2D hit = Physics2D.Raycast(transform.position, aimDirection, maxGrappleDistance, grappleLayer);

        if (hit.collider != null)
        {
            springJoint.connectedAnchor = hit.point;
            currentGrappleLength = Vector2.Distance(transform.position, hit.point);
            currentMaxGrappleLength = currentGrappleLength;
            springJoint.distance = currentGrappleLength;
            springJoint.enabled = true;

            if(lineRenderer != null) lineRenderer.enabled = true;

            currentState = GrappleState.Hooked;
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

        if(lineRenderer != null) lineRenderer.enabled = false;
        springJoint.enabled = false;
    }

    public void ReelIn()
    {
        if(currentState != GrappleState.Hooked) return;

        currentGrappleLength -= reelInSpeed * Time.deltaTime;

        currentGrappleLength = Mathf.Clamp(currentGrappleLength, 0f, currentMaxGrappleLength);
    }

    public void ReelOut()
    {
        if(currentState != GrappleState.Hooked) return;

        currentGrappleLength += reelOutSpeed * Time.deltaTime;

        currentGrappleLength = Mathf.Clamp(currentGrappleLength, 0f, currentMaxGrappleLength);
    }
}
