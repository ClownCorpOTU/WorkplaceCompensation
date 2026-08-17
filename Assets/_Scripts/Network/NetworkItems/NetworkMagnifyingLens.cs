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
    
    //[Networked] private NetworkBool hasHitPlayer { get; set; }
    //[Networked] private byte burnSignal { get; set; }

    private ChangeDetector changes;
    private NetworkGameManager networkGameManager;
    
    // Tracks Object ID -> Time spent cooking
    private Dictionary<NetworkId, float> cookTrackers = new Dictionary<NetworkId, float>();

    public override void Spawned()
    {
        networkGameManager = FindFirstObjectByType<NetworkGameManager>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!Object.HasStateAuthority) return;
    
        if (other.CompareTag("Fossil"))
        {
            CookFossil(other);
        }
        else if (other.transform.root.TryGetComponent(out NetworkPlayer player))
        {
            player.Burn();
        }
    }
    
    private void CookFossil(Collider other)
    {
        // Give point to whoever was holding the fossil last
        if (other.gameObject.TryGetComponent(out GrabbedByTracker grabTracker))
            networkGameManager.AddScore(grabTracker.LastHeldBy, 2);
        
        // Despawn fossil
        NetworkObject no = other.gameObject.GetComponent<NetworkObject>();
        if (no != null) Runner.Despawn(no);
        
        // Spawn egg
        Runner.Spawn(eggPrefab, other.transform.position, Quaternion.identity);
    }
}