using System;
using System.Collections.Generic;
using Fusion;
using UnityEngine;

public class NetworkMagnifyingLens : NetworkBehaviour
{
    [SerializeField] private GameObject eggPrefab;

    [Header("Timing Settings")] 
    [SerializeField] private float playerCookTime = 2.0f;
    [SerializeField] private float fossilCookTime = 1.3f;
    
    private bool hasHitPlayer;
    private NetworkGameManager networkGameManager;
    private AudioManager audioManager;
    
    // Tracks Object ID -> Time spent cooking
    private Dictionary<NetworkId, float> cookTrackers = new Dictionary<NetworkId, float>();

    public override void Spawned()
    {
        networkGameManager = FindFirstObjectByType<NetworkGameManager>();
        audioManager = FindFirstObjectByType<AudioManager>();
    }

    private void OnTriggerEnter(Collider other)
    {
        switch (other.gameObject.tag)
        {
            case "Player":
                if (!hasHitPlayer) HitPlayer(other);
                break;
            case "Fossil":
                CookFossil(other);
                break;
            default:
                break;
        }
    }

    private void HitPlayer(Collider other)
    {
        hasHitPlayer = true;
        
        if (other.transform.root.TryGetComponent(out NetworkPlayer networkPlayer))
        {
            RPC_BurnPlayer(networkPlayer);
            networkPlayer.MakeRagdoll();
            hasHitPlayer = false;
        }
    }
    
    private void CookFossil(Collider other)
    {
        // Give point to whoever was holding the fossil last
        if (other.gameObject.TryGetComponent(out GrabbedByTracker grabTracker))
            networkGameManager.AddScore(grabTracker.LastHeldBy, 1);
        
        // Despawn fossil
        NetworkObject no = other.gameObject.GetComponent<NetworkObject>();
        if (no != null) Runner.Despawn(no);
        
        // Spawn egg
        Runner.Spawn(eggPrefab, other.transform.position, Quaternion.identity);
    }
    
    
    [Rpc(RpcSources.StateAuthority, RpcTargets.All, TickAligned = false)]
    private void RPC_Play(string audioName, Vector3 position)
    {
        print("Playing!");
        if (audioManager != null) audioManager.Play(audioName, position);
    }
    
    [Rpc(RpcSources.StateAuthority, RpcTargets.All, TickAligned = false)]
    private void RPC_BurnPlayer(NetworkPlayer networkPlayer)
    {
        networkPlayer.GetComponent<DissolvingController>().BeginFx();
        if (audioManager != null) audioManager.Play("PlayerBurn", networkPlayer.transform.position);
    }
}