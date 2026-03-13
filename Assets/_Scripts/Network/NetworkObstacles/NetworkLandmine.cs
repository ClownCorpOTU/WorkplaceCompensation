using System;
using System.Threading.Tasks;
using Fusion;
using Unity.Cinemachine;
using UnityEngine;
using Random = UnityEngine.Random;

public class NetworkLandmine : NetworkBehaviour
{
    [SerializeField] private float explosionWaitTime = 0.1f;
    [SerializeField] private float explosionRadius = 5f;
    [SerializeField] private float explosionForce = 1000f;
    [SerializeField] private float camShakeForce = 1.5f;
    
    [Networked] private NetworkBool hasActivated { get; set; } // The "fuse"
    [Networked] private NetworkBool physicsTriggered { get; set; } // The "boom"
    [Networked] private NetworkBool hasExploded { get; set; } // The after-effects
    [Networked] private TickTimer explosionTimer { get; set; }
    [Networked] private TickTimer flashbangSFxTimer { get; set; }
    [Networked] private TickTimer despawnTimer { get; set; }
    
    private ChangeDetector changes;
    private CinemachineImpulseSource thisImpulseSource;
    private FullScreenEffectsManager fullScreenEffectsManager;
    private AudioManager audioManager;
    

    public override void Spawned()
    {
        changes = GetChangeDetector(ChangeDetector.Source.SimulationState);
        thisImpulseSource = GetComponent<CinemachineImpulseSource>();
        fullScreenEffectsManager = FindFirstObjectByType<FullScreenEffectsManager>();
        audioManager = FindFirstObjectByType<AudioManager>();
    }
    
    private void OnCollisionEnter(Collision other)
    {
        if (!Object.HasStateAuthority) return;
        if (hasActivated || other.gameObject.CompareTag("Ground")) return;

        hasActivated = true;
        explosionTimer = TickTimer.CreateFromSeconds(Runner, explosionWaitTime);
    }

    public override void FixedUpdateNetwork()
    {
        if (explosionTimer.Expired(Runner))
        {
            explosionTimer = TickTimer.None;
            physicsTriggered = true;
            Explode();

            despawnTimer = TickTimer.CreateFromSeconds(Runner, 0.25f);
        }

        if (flashbangSFxTimer.Expired(Runner))
            hasExploded = true;
        
        if (despawnTimer.Expired(Runner))
            Runner.Despawn(Object);
    }

    private void Explode()
    {
        // Explode landmine
        Collider[] colliders = new Collider[10];
        int numFound = Runner.GetPhysicsScene().OverlapSphere(transform.position, explosionRadius, colliders, ~0, QueryTriggerInteraction.Collide);

        for (int i = 0; i < numFound; i++)
        {
            Rigidbody rb = colliders[i].GetComponent<Rigidbody>();
            
            if (rb != null)
                rb.AddExplosionForce(explosionForce, transform.position, explosionRadius, 3f);
            
            if (colliders[i].transform.root.TryGetComponent(out NetworkPlayer player))
                player.MakeRagdoll();
        }
        
        flashbangSFxTimer = TickTimer.CreateFromSeconds(Runner, 0.2f);
    }
    
    public override void Render()
    {
        // This detects the change on both Host and Client
        foreach (var change in changes.DetectChanges(this))
        {
            if (change == nameof(hasActivated) && hasActivated)
                audioManager.Play("BombCountdown", transform.position);
            if (change == nameof(physicsTriggered) && physicsTriggered)
                TriggerExplosionEffects();
            if (change == nameof(hasExploded) && hasExploded)
                audioManager.Play("Flashbanged");
        }
    }

    private void TriggerExplosionEffects()
    {
        // Audio
        audioManager.Play("LandmineExplosion", transform.position);
        
        // VFx
        fullScreenEffectsManager.TriggerTimeStop(0.1f);
        fullScreenEffectsManager.TriggerImpactFlash(10);
        // Also spawn explosion particles and debris later
        
        // Camera Shake
        float distance = Vector3.Distance(NetworkPlayer.Local.transform.position, transform.position);
        float shakeRadius = explosionRadius + 5f;
        
        if (distance < shakeRadius)
        {
            float proximityMultiplier = 1.0f - (distance / shakeRadius);
            float finalForce = camShakeForce * proximityMultiplier;
            
            Vector3 shakeDir = (new Vector3(0.2f, 0.8f, -0.3f) + Random.insideUnitSphere * 0.5f).normalized;
            CameraShakeManager.Instance.ApplyCameraShake(thisImpulseSource, shakeDir, finalForce);
        }
    }
}
