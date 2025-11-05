using UnityEngine;
using UnityEngine.Animations.Rigging;

/// <summary>
/// This component talks to all the handGrabHandlers (1 on each hand).
/// Based on player input, it uses IK to raise or lower hands (the grab handlers handle actual grabbing).
/// </summary>
public class NetworkPlayerGrab : MonoBehaviour
{
    [HideInInspector] public Rigidbody CurrentlyGrabbedRigidbody;

    [Header("Joint Parameters")]
    [SerializeField] private ConfigurableJoint leftArmJoint;
    [SerializeField] private ConfigurableJoint rightArmJoint;
    [SerializeField] private float limpArmJointSpring = 7.5f;
    [SerializeField] private float limpArmJointDamper = 0.25f;
    
    [Header("IK Parameters")]
    [SerializeField] private TwoBoneIKConstraint leftHandGrabRig;
    [SerializeField] private TwoBoneIKConstraint rightHandGrabRig;
    [SerializeField] private Transform leftHandTarget, rightHandTarget;
    [SerializeField] private Transform leftHandGrabTargetPos, rightHandGrabTargetPos;
    [SerializeField] private Transform leftHandLiftTargetPos, rightHandLiftTargetPos;
    [SerializeField] private float smoothTime = 0.15f; // Lower = snappier, higher = floatier
    
    
    private NetworkPlayer networkPlayer;
    private HandGrabHandler[] handGrabHandlers;
    private float leftVelocity, rightVelocity;
    private float originalArmJointValue, originalArmDampingValue;
    
    public void Initialize(NetworkPlayer player)
    {
        networkPlayer = player;
        
        leftHandGrabRig.weight = 0f;
        rightHandGrabRig.weight = 0f;

        handGrabHandlers = player.gameObject.GetComponentsInChildren<HandGrabHandler>();
        
        // Both arm joints have the same strengths, so we'll just save one
        originalArmJointValue = leftArmJoint.slerpDrive.positionSpring;
        originalArmDampingValue = leftArmJoint.slerpDrive.positionDamper;
    }

    public void AnimateHands(bool isLifting = false)
    {
        // Update IK targets
        if (isLifting)
        {
            leftHandTarget.position = leftHandLiftTargetPos.position;
            rightHandTarget.position = rightHandLiftTargetPos.position;
        }
        else
        {
            leftHandTarget.position = leftHandGrabTargetPos.position;
            rightHandTarget.position = rightHandGrabTargetPos.position;
        }

        // Compute hand intent
        float leftHandIntent = (networkPlayer.IsLeftHandGrabbingActive || networkPlayer.IsGrabbingActive) ? 1f : 0f;
        float rightHandIntent = (networkPlayer.IsRightHandGrabbingActive || networkPlayer.IsGrabbingActive) ? 1f : 0f;

        // Smoothly blend IK weights
        leftHandGrabRig.weight = Mathf.SmoothDamp(leftHandGrabRig.weight, leftHandIntent, ref leftVelocity, smoothTime);
        rightHandGrabRig.weight = Mathf.SmoothDamp(rightHandGrabRig.weight, rightHandIntent, ref rightVelocity, smoothTime);

        // Smoothly lerp arm joint stiffness and damping instead of snapping
        float targetSpringLeft = Mathf.Lerp(limpArmJointSpring, originalArmJointValue, leftHandIntent);
        float targetSpringRight = Mathf.Lerp(limpArmJointSpring, originalArmJointValue, rightHandIntent);
        
        float targetDampingLeft = Mathf.Lerp(limpArmJointDamper, originalArmDampingValue, leftHandIntent);
        float targetDampingRight = Mathf.Lerp(limpArmJointDamper, originalArmDampingValue, rightHandIntent);
        
        // Apply drive updates safely (copy → modify → assign)
        JointDrive leftDrive = leftArmJoint.slerpDrive;
        leftDrive.positionSpring = targetSpringLeft;
        leftDrive.positionDamper = targetDampingLeft;
        leftArmJoint.slerpDrive = leftDrive;
        
        JointDrive rightDrive = rightArmJoint.slerpDrive;
        rightDrive.positionSpring = targetSpringRight;
        rightDrive.positionDamper = targetDampingRight;
        rightArmJoint.slerpDrive = rightDrive;
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