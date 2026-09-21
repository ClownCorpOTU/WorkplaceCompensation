using Fusion;
using UnityEngine;

public partial class NetworkPlayer
{
    [Header("Ground Detection")]
    [SerializeField] private float extraGravity = 7.5f;
    [SerializeField] private float groundCheckRadius = 0.25f;
    [SerializeField] private float groundCheckDistance = 0.3f;
    [SerializeField] private Transform groundCheckOrigin; // assign foot or base transform in inspector
    
    [Networked, OnChangedRender(nameof(OnGroundedChange))]
    public NetworkBool IsGrounded { get; set; }
    
    private readonly RaycastHit[] groundHits = new RaycastHit[10];


    private void OnGroundedChange()
    {
        // Do anything needed
    }
    
    private void GravityAndGrounding()
    {
        if (!Object.HasStateAuthority) return;

        // If we are moving UP quickly (jumping), skip grounding for a few ticks
        if (!jumpCooldown.ExpiredOrNotRunning(Runner))
        {
            IsGrounded = false;
            return;
        }
        
        Vector3 origin = groundCheckOrigin != null ? groundCheckOrigin.position : rb.position;
        origin += Vector3.up * 0.1f; // small offset to avoid self-intersection

        int hitCount = Physics.SphereCastNonAlloc(
            origin,
            groundCheckRadius,
            Vector3.down,
            groundHits,
            groundCheckDistance,
            ~0, // all layers
            QueryTriggerInteraction.Ignore
        );

        bool foundGround = false;
        for (int i = 0; i < hitCount; i++)
        {
            var hit = groundHits[i];
            
            if (hit.transform.root == transform) continue; // ignore self
            if (Vector3.Angle(hit.normal, Vector3.up) > 60f) continue; // too steep

            foundGround = true;
            break;
        }

        IsGrounded = foundGround;

        // Apply extra gravity in air to prevent floatiness
        if (!IsGrounded)
        {
            // If gravity is 9.81, multiplier is 1. If it's 3.7 (Mars), multiplier is ~0.37
            float gravityMultiplier = Mathf.Abs(Physics.gravity.y) / 9.81f;
        
            // Apply scaled extra gravity
            //rb.AddForce(Vector3.down * (extraGravity * gravityMultiplier), ForceMode.Acceleration);
            rb.AddForce(Vector3.down * (extraGravity), ForceMode.Acceleration);
        }

        Debug.DrawRay(origin, Vector3.down * groundCheckDistance, IsGrounded ? Color.green : Color.red);
    }
}
