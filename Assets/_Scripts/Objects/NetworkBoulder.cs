using System;
using System.Threading.Tasks;
using Fusion;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class NetworkBoulder : NetworkBehaviour
{
    [SerializeField] private float selfDestructTime = 12f;
    [SerializeField] private GameObject breakVfxPrefab;
    [SerializeField] private float maxShakeDistance = 30f;
    [SerializeField] private float baseShakeForce = 1.5f;
    
    [Networked] private byte breakSignal { get; set; } // Networked byte to signal the "Break" event
    [Networked] private byte collisionSignal { get; set; } // Networked byte to signal the collision events
    [Networked] private byte flattenSignal { get; set; } // Networked byte to signal the collision events
    [Networked] private TickTimer selfDestructTimer { get; set; }
    [Networked] private NetworkPlayer lastHitPlayer { get; set; }

    private ChangeDetector changes;
    private AudioManager audioManager;
    private bool isDespawning = false;
    private Vector3 boulderScale;
    private CinemachineImpulseSource thisImpulseSource;
    private CameraShakeManager camShakeManager;

    public override void Spawned()
    {
        audioManager = FindFirstObjectByType<AudioManager>();
        changes = GetChangeDetector(ChangeDetector.Source.SimulationState);
        boulderScale = transform.localScale;
        thisImpulseSource = GetComponent<CinemachineImpulseSource>();
        camShakeManager = CameraShakeManager.Instance;
        
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
                PlayCollisionEffects();

            if (change == nameof(lastHitPlayer))
            {
                if (lastHitPlayer != null)
                    Debug.Log($"[Render] Flattening: {lastHitPlayer.Object.Id}");
            }

            if (lastHitPlayer != null && change == nameof(flattenSignal) && flattenSignal > 0)
            {
                lastHitPlayer.FlattenAndMakeRagdoll();
            }
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
                lastHitPlayer = player;
                flattenSignal++;
            }
            else if (other.transform.root.TryGetComponent<NetworkPlayer>(out var rootPlayer))
            {
                lastHitPlayer = rootPlayer;
                flattenSignal++;
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

    private void PlayCollisionEffects()
    {
        // Play sound
        if (audioManager != null) audioManager.Play("RockImpact", transform.position);
        
        // Shake camera
        if (camShakeManager != null && thisImpulseSource != null && NetworkPlayer.Local != null)
        {
            // Using sqrMagnitude to avoid the expensive square root calculation
            float sqrDistance = (transform.position - NetworkPlayer.Local.transform.position).sqrMagnitude;
            float sqrMaxDist = maxShakeDistance * maxShakeDistance;
            
            if (sqrDistance < sqrMaxDist)
            {
                // Calculate attenuation (0 at maxDistance, 1 when boulder is right next to the player)
                float linearAttenuation = Mathf.Clamp01(1f - (sqrDistance / sqrMaxDist));
        
                // Square it for a more "impactful" feel (Quadratic Falloff)
                float finalAttenuation = linearAttenuation * linearAttenuation;

                // Bias the shake slightly forward/down based on the boulder's drop
                Vector3 shakeDir = (Vector3.down + Random.insideUnitSphere * 0.3f).normalized;
        
                camShakeManager.ApplyCameraShake(thisImpulseSource, shakeDir, baseShakeForce * finalAttenuation);
            }
        }
    }
}