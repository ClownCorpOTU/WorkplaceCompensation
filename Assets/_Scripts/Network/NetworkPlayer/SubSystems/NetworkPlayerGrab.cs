using UnityEngine;
using UnityEngine.Animations.Rigging;

/// <summary>
/// This component talks to all the handGrabHandlers (1 on each hand).
/// Based on player input, it uses IK to raise or lower hands (the grab handlers handle actual grabbing).
/// </summary>
public class NetworkPlayerGrab : MonoBehaviour
{
    [HideInInspector] public Rigidbody CurrentlyGrabbedRigidbody;
    [HideInInspector] public HandSide CurrentlyGrabbedHandSide = HandSide.None;

    [Header("Joint Parameters")]
    [SerializeField] private ConfigurableJoint leftArmJoint;
    [SerializeField] private ConfigurableJoint rightArmJoint;
    [SerializeField] private float limpArmJointSpring = 7.5f;
    [SerializeField] private float limpArmJointDamper = 0.25f;
    
    [Header("IK Parameters")]
    [SerializeField] private TwoBoneIKConstraint leftHandGrabRig;
    [SerializeField] private TwoBoneIKConstraint rightHandGrabRig;
    [SerializeField] private Transform leftHand, rightHand;
    [SerializeField] private Transform leftHandTarget, rightHandTarget;
    [SerializeField] private Transform leftHandGrabTargetPos, rightHandGrabTargetPos;
    [SerializeField] private Transform leftHandLiftTargetPos, rightHandLiftTargetPos;
    [SerializeField] private float smoothTime = 0.15f; // Lower = snappier, higher = floatier
    
    [Header("Grab Magnetism Parameters")]
    [SerializeField] private float grabRadius = 0.5f;
    [SerializeField] private float magnetForce = 10f;
    [SerializeField] private LayerMask grabbableLayer;
    private Collider[] grabResults = new Collider[8];
    
    private NetworkPlayer networkPlayer;
    private HandGrabHandler[] handGrabHandlers;
    private float leftVelocity, rightVelocity;
    private float originalArmJointValue, originalArmDampingValue;
    private bool hasGrabbedBefore;
    private bool hasGrabbedVialBefore;
    
    
    public void Initialize(NetworkPlayer player)
    {
        networkPlayer = player;
        
        leftHandGrabRig.weight = 0f;
        rightHandGrabRig.weight = 0f;

        handGrabHandlers = player.gameObject.GetComponentsInChildren<HandGrabHandler>();
        
        // Both arm joints have the same strengths, so we'll just save one
        originalArmJointValue = leftArmJoint.slerpDrive.positionSpring;
        originalArmDampingValue = leftArmJoint.slerpDrive.positionDamper;
        
        CurrentlyGrabbedHandSide = HandSide.None;
    }
    
    public void AnimateHands(bool isLifting = false)
    {
        // 1. Cache the target positions based on state
        leftHandTarget.position = isLifting ? leftHandLiftTargetPos.position : leftHandGrabTargetPos.position;
        rightHandTarget.position = isLifting ? rightHandLiftTargetPos.position : rightHandGrabTargetPos.position;

        // 2. Cache inputs to avoid multiple property lookups
        bool sharedGrab = networkPlayer.IsGrabbingActive;
        float leftIntent = (networkPlayer.IsLeftHandGrabbingActive || sharedGrab) ? 1f : 0f;
        float rightIntent = (networkPlayer.IsRightHandGrabbingActive || sharedGrab) ? 1f : 0f;

        // 3. Smooth blend IK weights
        leftHandGrabRig.weight = Mathf.SmoothDamp(leftHandGrabRig.weight, leftIntent, ref leftVelocity, smoothTime);
        rightHandGrabRig.weight = Mathf.SmoothDamp(rightHandGrabRig.weight, rightIntent, ref rightVelocity, smoothTime);

        // 4. Update Physics Drives
        UpdateArmJointDrive(leftArmJoint, leftIntent);
        UpdateArmJointDrive(rightArmJoint, rightIntent);
        
        // 5. Create an invisible sphere around the hand that's trying to grab an item
        if (CurrentlyGrabbedHandSide != HandSide.Left) ApplyGrabMagnetism(leftHand, leftIntent, 0);
        if (CurrentlyGrabbedHandSide != HandSide.Right) ApplyGrabMagnetism(rightHand, rightIntent, 1);
    }

    private void UpdateArmJointDrive(ConfigurableJoint joint, float intent)
    {
        JointDrive drive = joint.slerpDrive;
        drive.positionSpring = Mathf.Lerp(limpArmJointSpring, originalArmJointValue, intent);
        drive.positionDamper = Mathf.Lerp(limpArmJointDamper, originalArmDampingValue, intent);
        joint.slerpDrive = drive;
    }

    private void ApplyGrabMagnetism(Transform handTransform, float intent, int leftOrRight = 0)
    {
        if (intent < 0.5f) return;
        
        // This grabs your own body parts (if Player is selected in the layer mask), but it's filtered out later
        int count = Physics.OverlapSphereNonAlloc(
            handTransform.position, 
            grabRadius, 
            grabResults,
            grabbableLayer
        );
        
        Rigidbody closestRb = null;
        float closestDist = float.MaxValue;

        // Find closest rigidbody
        for (int i = 0; i < count; i++)
        {
            // Skip if it's the player's own body parts
            if (grabResults[i].transform.root == transform.root) continue;
            
            if (grabResults[i].TryGetComponent(out Rigidbody rb))
            {
                // Skip if we're already holding this object
                if (rb == CurrentlyGrabbedRigidbody) continue;
                
                float dist = Vector3.Distance(handTransform.position, rb.worldCenterOfMass);

                if (dist < closestDist)
                {
                    closestRb = rb;
                    closestDist = dist;
                }
            }
        }

        if (closestRb == null || closestRb.isKinematic) return;
        
        Vector3 direction = (handTransform.position - closestRb.worldCenterOfMass).normalized;
        float forceMagnitude = magnetForce * (1f - (closestDist / grabRadius));
        closestRb.AddForce(-direction * forceMagnitude, ForceMode.Impulse);
        
        // === GAMEPLAY TUTORIALS === //
        // Gameplay tutorial step for grabbing boxes (Technically checks if we grabbed anything)
        if (!hasGrabbedBefore)
        {
            networkPlayer.RPC_TriggerTutorialEvent(networkPlayer.PlayerRefValue, (int)GameEvent.BoxGrabbed);
            hasGrabbedBefore = true;
        }
        
        // Gameplay tutorial step for grabbing vials for the first time
        if (!hasGrabbedVialBefore && closestRb.TryGetComponent(out Vial vial) && vial.Type == ObjectType.OutputVial)
        {
            networkPlayer.RPC_TriggerTutorialEvent(networkPlayer.PlayerRefValue, (int)GameEvent.VialsGrabbed);
            hasGrabbedVialBefore = true;
        }
    }

    public void ForceRelease()
    {
        CurrentlyGrabbedRigidbody = null;
        CurrentlyGrabbedHandSide = HandSide.None;
        
        leftHandGrabRig.weight = 0f;
        rightHandGrabRig.weight = 0f;
        
        foreach (var hand in handGrabHandlers)
        {
            hand.ReleaseJoint();
        }
    }
}