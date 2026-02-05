using System;
using Fusion;
using UnityEngine;

public partial class NetworkPlayer
{
    [SerializeField] private float ragdollTime = 3f; 
    private bool isActiveRagdoll = true;
    public bool IsActiveRagdoll => isActiveRagdoll;
    
    private float startSlerpPositionSpring;
    private float lastTimeBecameRagdoll;
    
    [Networked] private TickTimer waitBeforeRespawn { get; set; }

    public void CreateRespawnTimer()
    {
        // I don't want to add this directly to MakeRagdoll() in case it breaks something
        waitBeforeRespawn = TickTimer.CreateFromSeconds(Runner, ragdollTime);
    }
    
    public void MakeRagdoll()
    {
        if (!Object.HasStateAuthority) return;
        
        // Disable collider
        mainCollider.enabled = false;
        
        // Update main join
        JointDrive jointDrive = mainJoint.slerpDrive;
        jointDrive.positionSpring = 0f;
        mainJoint.slerpDrive = jointDrive;
        
        // Update joint rotations and send them to the clients
        foreach (SyncPhysicsObject syncedObject in syncPhysicsObjects)
        {
            syncedObject.MakeRagdoll();
        }
        
        // Play sound
        audioManager.Play("Ragdoll", transform.position);
        
        // Make sure we're not carrying anything
        isGrabbingActive = false;

        themeSong.EnableLowPassFilter(true);
        lastTimeBecameRagdoll = Runner.SimulationTime;
        isActiveRagdoll = false;
        
        // Idk man maybe this will break something because I've been creating timers in other places like a doofus
        if (waitBeforeRespawn.ExpiredOrNotRunning(Runner))
            waitBeforeRespawn = TickTimer.CreateFromSeconds(Runner, ragdollTime);
    }
    
    public void MakeActiveRagdoll()
    {
        if (!Object.HasStateAuthority) return;
        
        // Enable collider
        mainCollider.enabled = true;
        
        // Update main join
        JointDrive jointDrive = mainJoint.slerpDrive;
        jointDrive.positionSpring = startSlerpPositionSpring;
        mainJoint.slerpDrive = jointDrive;
        
        // Update joint rotations and send them to the clients
        foreach (SyncPhysicsObject syncedObject in syncPhysicsObjects)
        {
            syncedObject.MakeActiveRagdoll();
        }
        
        // Make sure we're not carrying anything
        isGrabbingActive = false;
        
        themeSong.EnableLowPassFilter(false);
        isActiveRagdoll = true;
    }

    public void OnPlayerBodyPartHit()
    {
        if (!isActiveRagdoll) return;
        
        MakeRagdoll();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("MakeRagdoll"))
        {
            MakeRagdoll();
            waitBeforeRespawn = TickTimer.CreateFromSeconds(Runner, ragdollTime);
            
            // Checking if the timer has expired in the main script since it derives from NetworkBehaviour
        }
    }
}