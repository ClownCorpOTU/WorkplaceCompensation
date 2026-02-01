using UnityEngine;
using Fusion;

public partial class NetworkPlayer
{
    [Networked, Capacity(10), HideInInspector] public NetworkArray<Quaternion> NetworkPhysicsSyncedRotations { get; }
    //[SerializeField] private SyncPhysicsObject headObject;
    
    private void SyncAnimations(float localForwardVelocity)
    {
        if (!Object.HasStateAuthority) return;

        /* Looks like it's better to always sync the head
        if (localForwardVelocity < 0.1f)
        {
            headObject.UpdateSyncing(true);
        }
        else
        {
            headObject.UpdateSyncing(false);
        }
        */

        animatedModel.SetFloat("movementSpeed", localForwardVelocity * 0.4f);
        for (int i = 0; i < syncPhysicsObjects.Length; i++)
        {
            syncPhysicsObjects[i].UpdateJointFromAnimation();
            NetworkPhysicsSyncedRotations.Set(i, syncPhysicsObjects[i].transform.localRotation);
        }
        
        // Reset position if we fall off
        if (transform.position.y < -75f)
        {
            audioManager.Play("Death", transform.position);
            playerRespawn.Respawn(false);
        }

        // Updating hand grabbing handlers
        playerGrab.AnimateHands(IsLiftingActive);
    }

    private void UpdateDustFX(float localForwardVelocity)
    {
        if (dustFXParticles == null) return;

        // Cache modules
        var emission = dustFXParticles.emission;
        var main = dustFXParticles.main;

        // Only show dust when grounded
        emission.enabled = isGrounded;
        if (!isGrounded)
            return;

        // Clamp velocity
        float clampedSpeed = Mathf.Clamp(localForwardVelocity, 0f, 15f);

        // Change emission rate - Makes dust "denser" the faster you move
        emission.rateOverDistance = new ParticleSystem.MinMaxCurve(
            Mathf.Lerp(rateOverDistanceRange.x, rateOverDistanceRange.y, clampedSpeed / 15f)
        );

        // Change particle size and speed - Dust spreads more when sprinting
        main.startSize = Mathf.Lerp(startSizeRange.x, startSizeRange.y, clampedSpeed / 15f);
        //main.startSpeed = Mathf.Lerp(startSpeedRange.x, startSpeedRange.y, clampedSpeed / 15f);
    }
}