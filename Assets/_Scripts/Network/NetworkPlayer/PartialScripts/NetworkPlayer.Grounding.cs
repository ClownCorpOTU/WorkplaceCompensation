/*
using System;
using UnityEngine;

public partial class NetworkPlayer
{
    private bool isGrounded = false;
    private RaycastHit[] raycastHits = new RaycastHit[10];

    // Spherecast params
    private const float sphereCastRadius = 0.2f;
    private const float sphereCastMaxDistance = 0.1f;
    private readonly Vector3 sphereCastOffset = Vector3.up * 0.05f;

    private void GravityAndGrounding()
    {
        isGrounded = false; // Check if we are grounded

        int numberOfHits = Physics.SphereCastNonAlloc(
            rb.position + sphereCastOffset,
            sphereCastRadius,
            Vector3.down,
            raycastHits,
            sphereCastMaxDistance
        );

        for (int i = 0; i < numberOfHits; i++)
        {
            if (raycastHits[i].transform.root == transform) continue; // Ignore self hits

            isGrounded = true;
            break;
        }

        // Add extra gravity when not grounded
        if (!isGrounded) rb.AddForce(Vector3.down * 25f);
    }
}
*/

using UnityEngine;

public partial class NetworkPlayer
{
    [Header("Ground Detection")]
    [SerializeField] private float groundCheckRadius = 0.25f;
    [SerializeField] private float groundCheckDistance = 0.3f;
    [SerializeField] private Transform groundCheckOrigin; // assign foot or base transform in inspector

    private bool isGrounded = false;
    
    private readonly RaycastHit[] groundHits = new RaycastHit[10];
    private const float extraGravity = 25f;

    
    private void GravityAndGrounding()
    {
        isGrounded = false;
        
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
            
            isGrounded = true;
            break;
        }

        // Apply stronger gravity when not grounded
        if (!isGrounded)
            rb.AddForce(Vector3.down * extraGravity, ForceMode.Acceleration);

        Debug.DrawRay(origin, Vector3.down * groundCheckDistance, isGrounded ? Color.green : Color.red);
    }
}
