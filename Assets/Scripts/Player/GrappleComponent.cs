using UnityEngine;

public class GrappleComponent : MonoBehaviour
{
    public enum GrappleState
    {
        Idle,
        Hooking,
        Hooked
    }
    public GrappleState currentState = GrappleState.Idle;

    [Header("Grapple Settings")]
    public float maxGrappleDistance = 15f;
    public float reelInSpeed = 5f;
    public float reelOutSpeed = 2f;
    public LayerMask grappleLayer;

    private Vector2 hookPoint;
    private float currentGrappleLength;
    private Vector2 aimDirection;

    void FixedUpdate()
    {
        DebugViewer.Log("Grapple State", currentState.ToString());
        DebugViewer.Log("Aim Direction", aimDirection.ToString("F2"));
        
        if (currentState == GrappleState.Hooked)
        {
            DebugViewer.Log("Rope Length", currentGrappleLength.ToString("F1"));
        }
        else
        {
            DebugViewer.RemoveLog("Rope Length");
        }
    }

    public void SetAimDirection(Vector2 direction)
    {
        aimDirection = direction;
    }

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

    public void Hook()
    {
        if (aimDirection == Vector2.zero) return;

        RaycastHit2D hit = Physics2D.Raycast(transform.position, aimDirection, maxGrappleDistance, grappleLayer);

        if (hit.collider != null)
        {
            Debug.DrawLine(transform.position, hit.point, Color.black, 5f);

            hookPoint = hit.point;
            currentGrappleLength = Vector2.Distance(transform.position, hookPoint);
            currentState = GrappleState.Hooked;
        }
    }

    public void Unhook()
    {
        currentState = GrappleState.Idle;
    }
}
