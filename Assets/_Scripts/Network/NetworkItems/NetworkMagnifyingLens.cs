using System;
using Fusion;
using UnityEngine;

public class NetworkMagnifyingLens : NetworkBehaviour
{
    [SerializeField] private GameObject eggPrefab;
    private bool hasHitPlayer;
    private NetworkGameManager networkGameManager;

    public override void Spawned()
    {
        networkGameManager = FindFirstObjectByType<NetworkGameManager>();
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
        print("hits");
        
        if (other.transform.root.TryGetComponent(out NetworkPlayer networkPlayer))
        {
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
}