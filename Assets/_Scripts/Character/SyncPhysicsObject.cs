using System;
using UnityEngine;

/// <summary>
/// This script syncs the joint rotation with the animated rigidbody.
/// </summary>
public class SyncPhysicsObject : MonoBehaviour
{
    [SerializeField] private Rigidbody animatedRB;
    [SerializeField] private bool syncAnimation;
    
    private Rigidbody rb;
    private ConfigurableJoint joint;
    private Quaternion startLocalRotation;
    private float startSlerpPositionSpring;

    
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        joint = GetComponent<ConfigurableJoint>();
        
        startLocalRotation = joint.transform.localRotation;
        startSlerpPositionSpring = joint.slerpDrive.positionSpring;
    }

    public void UpdateSyncing(bool update)
    {
        syncAnimation = update;
    }

    public void UpdateJointFromAnimation()
    {
        if (!syncAnimation) return;
        
        //ConfigurableJointExtensions.SetTargetRotationLocal(joint, animatedRB.transform.localRotation, startLocalRotation);
        joint.SetTargetRotationLocal(animatedRB.transform.localRotation, startLocalRotation);
    }
    
    public void MakeRagdoll()
    {
        JointDrive jointDrive = joint.slerpDrive;
        jointDrive.positionSpring = 1f;
        joint.slerpDrive = jointDrive;
    }
    
    public void MakeActiveRagdoll()
    {
        JointDrive jointDrive = joint.slerpDrive;
        jointDrive.positionSpring = startSlerpPositionSpring;
        joint.slerpDrive = jointDrive;
    }
}