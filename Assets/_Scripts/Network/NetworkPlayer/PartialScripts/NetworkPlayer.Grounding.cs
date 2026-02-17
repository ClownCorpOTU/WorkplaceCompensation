using Fusion;
using UnityEngine;

public partial class NetworkPlayer
{
    [Header("Ground Detection")]
    [SerializeField] private float groundCheckRadius = 0.25f;
    [SerializeField] private float groundCheckDistance = 0.3f;
    [SerializeField] private Transform groundCheckOrigin; // assign foot or base transform in inspector

    [Networked, OnChangedRender(nameof(OnGroundedChange))]
    public NetworkBool IsGrounded { get; set; }
    
    private readonly RaycastHit[] groundHits = new RaycastHit[10];
    private const float extraGravity = 2.5f;


    private void OnGroundedChange()
    {
        // Do anything needed
    }
    
    private void GravityAndGrounding()
    {
        if (!Object.HasStateAuthority) return;

        bool groundedResult = false;
        
        Vector3 origin = groundCheckOrigin != null ? groundCheckOrigin.position : rb.position;
        origin += Vector3.up * 0.05f; // small offset to avoid self-intersection

        int hitCount = Physics.SphereCastNonAlloc(
            origin,
            groundCheckRadius,
            Vector3.down,
            groundHits,
            groundCheckDistance,
            ~0, // all layers
            QueryTriggerInteraction.Ignore
        );

        for (int i = 0; i < hitCount; i++)
        {
            var hit = groundHits[i];
            
            if (hit.transform.root == transform) continue; // ignore self
            if (Vector3.Angle(hit.normal, Vector3.up) > 60f) continue; // too steep

            groundedResult = true;
            break;
        }

        IsGrounded = groundedResult;

        // Apply some gravity when grounded so player doesn't feel floaty
        //if (isGrounded)
            //rb.AddForce(Vector3.down * extraGravity, ForceMode.Acceleration);

        Debug.DrawRay(origin, Vector3.down * groundCheckDistance, IsGrounded ? Color.green : Color.red);
    }
}
