using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// This script sits on each hand, and checks if something has collided with it. If it has, it tries to grab it.
/// When it stops grabbing something, it throws it away from the player.
/// </summary>

public enum HandSide { Left, Right, None }

public class HandGrabHandler : MonoBehaviour
{
    [SerializeField] private HandSide handSide;

    private Rigidbody rb;
    private FixedJoint fixedJoint; // Created dynamically
    private NetworkPlayer networkPlayer;
    private NetworkPlayerGrab playerGrab;
    
    private void Awake()
    {
        networkPlayer = transform.root.GetComponent<NetworkPlayer>();
        playerGrab = networkPlayer.GetComponent<NetworkPlayerGrab>();
        rb = GetComponent<Rigidbody>();

        // Change solver iterations to prevent joint from flexing too much
        rb.solverIterations = 250;
    }

    private void OnCollisionEnter(Collision other)
    {
        if (IsThisHandActive()) TryCarryObject(other);
    }

    private void FixedUpdate()
    {
        // If this hand is no longer active, but still holding something, then release the object
        if (!IsThisHandActive() && fixedJoint != null)
        {
            ReleaseJoint();
        }
    }

    private bool IsThisHandActive()
    {
        return handSide switch
        {
            HandSide.Left => networkPlayer.IsLeftHandGrabbingActive || networkPlayer.IsGrabbingActive,
            HandSide.Right => networkPlayer.IsRightHandGrabbingActive || networkPlayer.IsGrabbingActive,
            _ => false
        };
    }

    private bool TryCarryObject(Collision other)
    {
        if (!networkPlayer.Object.HasStateAuthority) return false; // Only state authority can carry objects
        if (!networkPlayer.IsActiveRagdoll) return false; // Only active ragdoll can carry
        if (fixedJoint != null) return false; // Already holding something
        if (!other.collider.TryGetComponent(out Rigidbody otherRB)) return false; // Only Rigidbodies can be grabbed
        if (other.transform.root == networkPlayer.transform) return false; // Can't grab yourself
        
        // Attach joint
        fixedJoint = gameObject.AddComponent<FixedJoint>();
        fixedJoint.connectedBody = otherRB;
        fixedJoint.autoConfigureConnectedAnchor = false;
        
        // Transform collision point from world to local space
        fixedJoint.connectedAnchor = other.transform.InverseTransformPoint(other.GetContact(0).point);

        playerGrab.CurrentlyGrabbedRigidbody = otherRB;
        playerGrab.CurrentlyGrabbedHandSide = handSide;
            
        if (other.gameObject.TryGetComponent(out Vial v))
            v.OnGrabbedBy(networkPlayer);
        
        return true;
    }

    public void ReleaseJoint()
    {
        if (fixedJoint == null) return;
        
        // Apply throw force if still attached
        if (fixedJoint.connectedBody != null)
        {
            float forceAmountMultiplier = 0.5f;

            // Check if we're grabbing onto another player, and if they're ragdolled or not
            if (fixedJoint.connectedBody.transform.root.TryGetComponent(out NetworkPlayer otherPlayer))
            {
                forceAmountMultiplier = otherPlayer.IsActiveRagdoll ? 7f : 15f;
            }
            
            // Apply force to throw away the object
            fixedJoint.connectedBody.AddForce((networkPlayer.transform.forward + Vector3.up * 0.25f)
                                              * forceAmountMultiplier, ForceMode.Impulse);
        }
        
        // Destroy joint
        Destroy(fixedJoint);
        fixedJoint = null;

        playerGrab.CurrentlyGrabbedRigidbody = null;
        playerGrab.CurrentlyGrabbedHandSide = HandSide.None;
    }
}