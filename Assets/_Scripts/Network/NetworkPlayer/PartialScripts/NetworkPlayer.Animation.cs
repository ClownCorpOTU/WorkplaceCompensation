using UnityEngine;
using Fusion;

public partial class NetworkPlayer
{
    [SerializeField] private Animator animatedModel;
    [Networked, Capacity(10)] public NetworkArray<Quaternion> NetworkPhysicsSyncedRotations { get; }

    
    private void SyncAnimations(float localForwardVelocity)
    {
        if (!Object.HasStateAuthority) return;

        dustFXPrefab.SetActive(isGrounded);

        animatedModel.SetFloat("movementSpeed", localForwardVelocity * 0.4f);
        for (int i = 0; i < syncPhysicsObjects.Length; i++)
        {
            syncPhysicsObjects[i].UpdateJointFromAnimation();
            NetworkPhysicsSyncedRotations.Set(i, syncPhysicsObjects[i].transform.localRotation);
        }
        
        // Reset position if we fall off
        if (transform.position.y < -75f)
        {
            audioManager.Play("Death");
            playerRespawn.Respawn(false);
        }

        // Updating hand grabbing handlers
        playerGrab.AnimateHands(IsLiftingActive);
    }
}