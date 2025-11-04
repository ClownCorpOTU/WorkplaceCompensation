using UnityEngine;
using UnityEngine.Animations.Rigging;

/// <summary>
/// This component talks to all the handGrabHandlers (1 on each hand).
/// Based on player input, it uses IK to raise or lower hands (the grab handlers handle actual grabbing).
/// </summary>
public class NetworkPlayerGrab : MonoBehaviour
{
    [HideInInspector] public Rigidbody CurrentlyGrabbedRigidbody;

    [SerializeField] private TwoBoneIKConstraint leftHandGrabRig, rightHandGrabRig;
    [SerializeField] private Transform leftHandTarget, rightHandTarget;
    [SerializeField] private Transform leftHandGrabTargetPos, rightHandGrabTargetPos;
    [SerializeField] private Transform leftHandLiftTargetPos, rightHandLiftTargetPos;
    [SerializeField] private float smoothTime = 0.15f; // Lower = snappier, higher = floatier
    
    private NetworkPlayer networkPlayer;
    private HandGrabHandler[] handGrabHandlers;
    private float leftVelocity, rightVelocity;
    
    public void Initialize(NetworkPlayer player)
    {
        networkPlayer = player;
        
        leftHandGrabRig.weight = 0f;
        rightHandGrabRig.weight = 0f;

        handGrabHandlers = player.gameObject.GetComponentsInChildren<HandGrabHandler>();
    }

    public void AnimateHands(bool isLifting=false)
    {
        switch (isLifting)
        {
            case true:
                leftHandTarget.position = leftHandLiftTargetPos.position;
                rightHandTarget.position = rightHandLiftTargetPos.position;
                break;
            case false:
                leftHandTarget.position = leftHandGrabTargetPos.position;
                rightHandTarget.position = rightHandGrabTargetPos.position;
                break;
        }
        
        // Left hand
        float leftHand = (networkPlayer.IsLeftHandGrabbingActive || networkPlayer.IsGrabbingActive) ? 1f : 0f;
        leftHandGrabRig.weight = Mathf.SmoothDamp(leftHandGrabRig.weight, leftHand, ref leftVelocity, smoothTime);

        // Right hand
        float rightHand = (networkPlayer.IsRightHandGrabbingActive || networkPlayer.IsGrabbingActive) ? 1f : 0f;
        rightHandGrabRig.weight = Mathf.SmoothDamp(rightHandGrabRig.weight, rightHand, ref rightVelocity, smoothTime);
    }

    public void ForceRelease()
    {
        CurrentlyGrabbedRigidbody = null;
        leftHandGrabRig.weight = 0f;
        rightHandGrabRig.weight = 0f;
        
        foreach (var hand in handGrabHandlers)
        {
            hand.ReleaseJoint();
        }
    }
}