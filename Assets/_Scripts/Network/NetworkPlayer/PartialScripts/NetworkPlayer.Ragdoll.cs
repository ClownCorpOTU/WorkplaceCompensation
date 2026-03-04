using System;
using Fusion;
using UnityEngine;

public partial class NetworkPlayer
{
    [SerializeField] private float ragdollTime = 3f; 
    //private bool isActiveRagdoll = true;
    //public bool IsActiveRagdoll => isActiveRagdoll;
    [Networked, OnChangedRender(nameof(ToggleRagdollComponents))] public NetworkBool IsActiveRagdoll { get; set; } = true;
    
    private float startSlerpPositionSpring;
    private float lastTimeBecameRagdoll;
    
    [Networked] private TickTimer waitBeforeRespawn { get; set; }

    public void CreateRespawnTimer()
    {
        // I don't want to add this directly to MakeRagdoll() in case it breaks something
        waitBeforeRespawn = TickTimer.CreateFromSeconds(Runner, ragdollTime);
    }

    public void FlattenAndMakeRagdoll()
    {
        var blobbyOriginalScale = transform.localScale;
            
        transform.localScale = new Vector3(
            blobbyOriginalScale.x,
            blobbyOriginalScale.y,
            0.1f
        );
            
        MakeRagdoll();
    }
    
    public void MakeRagdoll()
    {
        if (!Object.HasStateAuthority) return;
        
        // Set state
        IsActiveRagdoll = false;
        
        // Idk man maybe this will break something because I've been creating timers in other places like a doofus
        if (waitBeforeRespawn.ExpiredOrNotRunning(Runner))
            waitBeforeRespawn = TickTimer.CreateFromSeconds(Runner, ragdollTime);
        
        // Disable collider
        //mainCollider.enabled = false;
        
        // Update main join
        //JointDrive jointDrive = mainJoint.slerpDrive;
        //jointDrive.positionSpring = 0f;
        //mainJoint.slerpDrive = jointDrive;
        
        // Update joint rotations and send them to the clients
        //foreach (SyncPhysicsObject syncedObject in syncPhysicsObjects)
        //{
            //syncedObject.MakeRagdoll();
        //}
        
        // Play sound
        //audioManager.Play("Ragdoll", transform.position);
        
        // Make sure we're not carrying anything
        isGrabbingActive = false;

        //themeSong.EnableLowPassFilter(true);
        lastTimeBecameRagdoll = Runner.SimulationTime;
    }
    
    public void MakeActiveRagdoll()
    {
        if (!Object.HasStateAuthority) return;
        
        IsActiveRagdoll = true;
        waitBeforeRespawn = TickTimer.None;
        
        // Enable collider
        //mainCollider.enabled = true;
        
        // Update main join
        //JointDrive jointDrive = mainJoint.slerpDrive;
        //jointDrive.positionSpring = startSlerpPositionSpring;
        //mainJoint.slerpDrive = jointDrive;
        
        // Update joint rotations and send them to the clients
        //foreach (SyncPhysicsObject syncedObject in syncPhysicsObjects)
        //{
            //syncedObject.MakeActiveRagdoll();
        //}
        
        // Make sure we're not carrying anything
        isGrabbingActive = false;
    }

    public void OnPlayerBodyPartHit()
    {
        if (!IsActiveRagdoll) return;
        
        MakeRagdoll();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("MakeRagdoll"))
        {
            MakeRagdoll();
            //waitBeforeRespawn = TickTimer.CreateFromSeconds(Runner, ragdollTime);
            
            // Checking if the timer has expired in the main script since it derives from NetworkBehaviour
        }
    }

    private void OnCollisionEnter(Collision other)
    {
        //print(other.gameObject.name);
        //print(other.gameObject.GetComponent<NetworkBoulder>());
        //if (other.gameObject.TryGetComponent(out NetworkBoulder boulder)) flattenSignal++;
    }

    private void ToggleRagdollComponents()
    {
        if (mainCollider != null) mainCollider.enabled = IsActiveRagdoll;

        if (mainJoint != null)
        {
            JointDrive jointDrive = mainJoint.slerpDrive;
            jointDrive.positionSpring = IsActiveRagdoll ? startSlerpPositionSpring : 0f;
            mainJoint.slerpDrive = jointDrive;
        }

        if (IsActiveRagdoll)
        {
            transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y, 1f);
            themeSong.EnableLowPassFilter(false);
            
            foreach (SyncPhysicsObject syncedObject in syncPhysicsObjects)
            {
                syncedObject.MakeActiveRagdoll();
            }
        }
        else
        {
            audioManager.Play("Ragdoll", transform.position);
            themeSong.EnableLowPassFilter(true);
            
            foreach (SyncPhysicsObject syncedObject in syncPhysicsObjects)
            {
                syncedObject.MakeRagdoll();
            }
        }
    }
}