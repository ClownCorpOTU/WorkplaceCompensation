using System;
using System.Threading.Tasks;
using Fusion;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

public class NetworkBoulder : NetworkBehaviour
{
    [SerializeField] private float selfDestructTime = 12f;
    [SerializeField] private GameObject breakVfxPrefab;
    
    [Networked] private byte breakSignal { get; set; } // Networked byte to signal the "Break" event
    [Networked] private byte collisionSignal { get; set; } // Networked byte to signal the collision events
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
            if (change == nameof(collisionSignal) && collisionSignal > 0)
                if (audioManager != null) audioManager.Play("RockImpact", transform.position);
        }
    }

    private void OnCollisionEnter(Collision other)
    {
        if (!Object.HasStateAuthority) return;
        
        //RPC_Play("RockImpact", transform.position);
        collisionSignal++;

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

            // Moves the local object into the Runner's physics scene
            if (Runner != null)
            {
                SceneManager.MoveGameObjectToScene(brokenBoulder, Runner.SimulationUnityScene);
            }
            
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
                // Ensure they aren't on a layer that ignores physics or collides with the old boulder
                chunkRb.isKinematic = false;
            
                // Inherit the boulder's momentum + a random burst
                chunkRb.AddForce(currentVelocity + UnityEngine.Random.insideUnitSphere * 5f, ForceMode.VelocityChange);
            
                // Add some random spin to make it look "alive"
                chunkRb.angularVelocity = UnityEngine.Random.insideUnitSphere * 10f;
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