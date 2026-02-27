using System;
using System.Threading.Tasks;
using Fusion;
using UnityEngine;
using UnityEngine.Serialization;

public class NetworkBoulder : NetworkBehaviour
{
    [SerializeField] private float selfDestructTime = 12f;
    [SerializeField] private GameObject breakVfxPrefab;
    
    [Networked] private byte breakSignal { get; set; } // Networked byte to signal the "Break" event
    [Networked] private TickTimer selfDestructTimer { get; set; }

    private ChangeDetector changes;
    private AudioManager audioManager;
    private bool isDespawning = false;
    private Vector3 boulderScale;

    public override void Spawned()
    {
        audioManager = FindFirstObjectByType<AudioManager>();
        changes = GetChangeDetector(ChangeDetector.Source.SimulationState);
        boulderScale = transform.localScale;
        
        if (Object.HasStateAuthority) selfDestructTimer = TickTimer.CreateFromSeconds(Runner, selfDestructTime);
    }
    
    public override void FixedUpdateNetwork()
    {
        if (Object.HasStateAuthority && selfDestructTimer.Expired(Runner) && !isDespawning)
        {
            DespawnBoulder();
        }
    }

    public override void Render()
    {
        // This detects the change on both Host and Client
        foreach (var change in changes.DetectChanges(this))
        {
            if (change == nameof(breakSignal) && breakSignal > 0)
                TriggerBreakEffects();
        }
    }

    private void OnCollisionEnter(Collision other)
    {
        if (!Object.HasStateAuthority) return;
        
        RPC_Play("RockImpact", transform.position);

        if (other.gameObject.CompareTag("Player"))
        {
            // Look in the object OR its parents for the NetworkPlayer script
            if (other.gameObject.TryGetComponent<NetworkPlayer>(out var player))
            {
                player.FlattenAndMakeRagdoll();
            }
            else if (other.transform.root.TryGetComponent<NetworkPlayer>(out var rootPlayer))
            {
                rootPlayer.FlattenAndMakeRagdoll();
            }
            else
            {
                Utils.DebugLogError("Cannot find player network component on " + other.gameObject.name + " or " + other.transform.root.name);
            }
        }
    }

    private async void DespawnBoulder()
    {
        isDespawning = true;

        breakSignal++; // Signal break to all clients
        await Task.Delay(150); // Wait a few ticks to make sure signal reaches all clients
        
        if (Object != null && Runner != null && Runner.IsRunning)
            Runner.Despawn(Object);
    }

    private void TriggerBreakEffects()
    {
        if (audioManager != null) audioManager.Play("RockBreak", transform.position);

        // BUG: Right now the physics of the broken boulder freezes on the clients
        if (breakVfxPrefab != null)
        {
            var brokenBoulder = Instantiate(breakVfxPrefab, transform.position, transform.rotation);
            brokenBoulder.transform.localScale = boulderScale;
            
            // Get the velocity of the current networked boulder to pass it to the chunks
            Vector3 currentVelocity = Vector3.zero;
            if (TryGetComponent<Rigidbody>(out var rb))
            {
                currentVelocity = rb.linearVelocity;
            }

            // Loop through all chunks in the broken prefab
            Rigidbody[] chunks = brokenBoulder.GetComponentsInChildren<Rigidbody>();
            foreach (var chunkRb in chunks)
            {
                chunkRb.isKinematic = false; // Force physics back on
                chunkRb.WakeUp();            // Ensure the physics engine is looking at it
            
                // Give it the boulder's momentum + a little random 'pop'
                chunkRb.linearVelocity = currentVelocity + UnityEngine.Random.insideUnitSphere * 2f;
            }
            
            // Destroy the boulder
            Destroy(brokenBoulder, 5f);
        }
    }
    
    [Rpc(RpcSources.StateAuthority, RpcTargets.All, TickAligned = false)]
    private void RPC_Play(string audioName, Vector3 position)
    {
        if (audioManager != null) audioManager.Play(audioName, position);
    }
}