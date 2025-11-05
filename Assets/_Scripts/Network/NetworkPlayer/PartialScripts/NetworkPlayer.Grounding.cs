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
            jumpConsumed = false;
            break;
        }
        
        // Add extra gravity when not grounded
        if (!isGrounded) rb.AddForce(Vector3.down * 25f);
    }
}