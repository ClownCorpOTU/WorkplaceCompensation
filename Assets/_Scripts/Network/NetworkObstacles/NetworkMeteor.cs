using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Fusion;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using Random = UnityEngine.Random;

public class NetworkMeteor : NetworkBehaviour
{
    [Header("Meteor Settings")]
    [SerializeField] private float selfDestructTime = 12f;
    [SerializeField] private GameObject breakVfxPrefab;
    [SerializeField] private GameObject landingWarningPrefab;
    [SerializeField] private GameObject fireTrailObj;

    [Header("Impact Settings")] 
    [SerializeField] private float explosionForce = 500f;
    [SerializeField] private float explosionRadius = 5f;
    [SerializeField] private float baseShakeForce = 2.2f;
    [SerializeField] private float maxShakeDistance = 50f;
    [SerializeField] private GameObject explosionVFX;
    
    [Networked] private Vector3 networkedVelocity { get; set; }
    [Networked] private Vector3 targetPos { get; set; }
    [Networked] private byte impactSignal { get; set; }
    [Networked] private TickTimer selfDestructTimer { get; set; }
    [Networked] private TickTimer despawnTimer { get; set; }

    private ChangeDetector changes;
    private AudioManager audioManager;
    private CameraShakeManager camShakeManager;
    private GameObject localWarningCircle;
    private CinemachineImpulseSource thisImpulseSource;
    private FullScreenEffectsManager fullScreenEffectsManager;
    private bool isDespawning = false;
    private DecalProjector warningDecal;
    
    public void InitializeMeteor(Vector3 finalSpeed, Vector3 targetLandingPos)
    {
        networkedVelocity = finalSpeed;
        targetPos = targetLandingPos;
        print(targetPos);
    }

    public override void Spawned()
    {
        audioManager = FindFirstObjectByType<AudioManager>();
        changes = GetChangeDetector(ChangeDetector.Source.SimulationState);
        thisImpulseSource = GetComponent<CinemachineImpulseSource>();
        camShakeManager = CameraShakeManager.Instance;
        fullScreenEffectsManager = FindFirstObjectByType<FullScreenEffectsManager>();
        
        if (MeteorWarningUI.Instance != null) MeteorWarningUI.Instance.SetWarning(true);

        if (Object.HasStateAuthority)
        {
            selfDestructTimer = TickTimer.CreateFromSeconds(Runner, selfDestructTime);
        }
    }
    
    public override void FixedUpdateNetwork()
    {
        // Despawn
        if (despawnTimer.Expired(Runner) && (Object != null && Runner != null && Runner.IsRunning))
            Runner.Despawn(Object);
        
        if (isDespawning) return;

        Vector3 displacement = networkedVelocity * Runner.DeltaTime;
        
        // Check if we are about to hit something
        if (Physics.Raycast(transform.position, networkedVelocity.normalized, out RaycastHit hit,
                displacement.magnitude + 0.1f))
        {
            transform.position = hit.point; // Snap to hit point for cleaner impact
            TriggerImpact();
            return;
        }
        
        // Make sure fire trail is rotated correctly so it actually looks like a tail
        // Make sure fire trail is rotated correctly so it actually looks like a tail
        if (fireTrailObj != null && networkedVelocity.sqrMagnitude > 0.1f)
        {
            // We set the WORLD rotation so it ignores the parent meteor's tumbling.
            // Use -networkedVelocity because the 'forward' axis of a trail 
            // usually needs to point AWAY from the direction of travel to look right.
            fireTrailObj.transform.rotation = Quaternion.LookRotation(-networkedVelocity);
        }
        
        // If not hit, move normally
        transform.position += displacement;
        transform.Rotate(Vector3.up, 90f * Runner.DeltaTime);

        // Safety despawn if it misses the world or lingers too long
        if (Object.HasStateAuthority && selfDestructTimer.Expired(Runner))
        {
            TriggerImpact();
        }
    }

    public override void Render()
    {
        foreach (var change in changes.DetectChanges(this))
        {
            if (change == nameof(impactSignal) && impactSignal > 0)
                PlayImpactEffects();
        }
        
        if (!isDespawning)
            UpdateLandingWarning();
    }

    private void OnCollisionEnter(Collision other)
    {
        if (!Object.HasStateAuthority || isDespawning) return;

        print("Collided");
        TriggerImpact();
    }

    private void TriggerImpact()
    {
        isDespawning = true;
        impactSignal++;

        ApplyRadialImpact();
    }

    private void ApplyRadialImpact()
    {
        // 1. Increase buffer size. 64 is usually safe for an explosion.
        Collider[] colliders = new Collider[64]; 
    
        // 2. Use a LayerMask to ignore the ground/environment if they don't have RBs
        // This saves slots in the 'colliders' array.
        int layerMask = ~LayerMask.GetMask("Ground"); 

        int numFound = Runner.GetPhysicsScene().OverlapSphere(
            transform.position, 
            explosionRadius, 
            colliders, 
            layerMask, 
            QueryTriggerInteraction.Collide
        );

        // 3. Track which players we've already hit to avoid redundant logic
        HashSet<NetworkPlayer> uniquePlayersHit = new HashSet<NetworkPlayer>();

        for (int i = 0; i < numFound; i++)
        {
            // Handle Physics (Boxes/Debris)
            // We apply force to every Rigidbody we find (this is good for ragdolls)
            if (colliders[i].TryGetComponent(out Rigidbody rb))
            {
                rb.AddExplosionForce(explosionForce, transform.position, explosionRadius, 3f);
            }
        
            // Handle Player Logic (Flattening/Health)
            // We use the root to find the NetworkPlayer script
            if (colliders[i].transform.root.TryGetComponent(out NetworkPlayer player))
            {
                // .Add() returns true only if the player wasn't already in the set
                if (uniquePlayersHit.Add(player))
                {
                    player.FlattenAndMakeRagdoll();
                }
            }
        }
        
        // 4. Create a despawn timer to ensure everything happens first
        despawnTimer = TickTimer.CreateFromSeconds(Runner, 0.2f);
    }

    private void PlayImpactEffects()
    {
        // Play SFx
        audioManager.Play("MeteorImpact", transform.position);
        
        // Play VFx
        if (explosionVFX != null) Instantiate(explosionVFX, transform.position, Quaternion.identity);
        fullScreenEffectsManager.TriggerTimeStop(0.2f);
        fullScreenEffectsManager.TriggerImpactFlash(16);
        
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
        
        // Spawn local broken chunks (BUG)
        if (breakVfxPrefab != null)
        {
            var vfx = Instantiate(breakVfxPrefab, transform.position, transform.rotation);
            vfx.transform.localScale = transform.localScale;
            Destroy(vfx, 3f);
        }
        
        // Remove warning UI
        if (MeteorWarningUI.Instance != null) MeteorWarningUI.Instance.SetWarning(false);
        
        // Clean up warning prefab
        if (localWarningCircle != null) Destroy(localWarningCircle);
    }
    
    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        // Extra safety to ensure the circle is destroyed if the meteor is despawned abruptly
        if (localWarningCircle != null) Destroy(localWarningCircle);
    }
    
    private void UpdateLandingWarning()
    {
        // Raycast from high above the target position down to the ground
        if (Physics.Raycast(targetPos + Vector3.up * 50f, Vector3.down, out RaycastHit hit, 100f))
        {
            if (localWarningCircle == null && landingWarningPrefab != null)
            {
                localWarningCircle = Instantiate(landingWarningPrefab, hit.point, Quaternion.identity);
                warningDecal = localWarningCircle.GetComponent<DecalProjector>();
            }

            if (warningDecal != null)
            {
                // 1. DEPTH & OFFSET FIX: 
                // We set a large Projection Depth (20 units) so it can handle hills.
                // We move the center of the box 10 units ABOVE the ground so the "beam" hits.
                float projectionDepth = 20f;
                warningDecal.transform.position = hit.point + (hit.normal * (projectionDepth * 0.5f));
                warningDecal.transform.rotation = Quaternion.LookRotation(-hit.normal);

                // 2. GROWTH MATH FIX:
                // Calculate distance from the meteor's current height to the impact height
                float currentDist = transform.position.y - hit.point.y;
            
                // Start growing when the meteor is 150m away (increase this if it's still "too small")
                float growthThreshold = 150f; 
                float progress = Mathf.Clamp01(1.0f - (currentDist / growthThreshold));

                // 3. SIZE RANGE: 
                // Let's go bigger. From 4m wide to 20m wide at impact.
                float currentSize = Mathf.Lerp(4f, 20f, progress);
            
                // Apply size (X, Y are width/height, Z is that depth we defined earlier)
                warningDecal.size = new Vector3(currentSize, currentSize, projectionDepth);
            }
        }
    }
    
    /*
    private void UpdateLandingWarning()
    {
        // Go 50 units above the targetPos, and raycast 100 units down to find any points on the terrain
        if (Physics.Raycast(targetPos + Vector3.up * 50f, Vector3.down, out RaycastHit hit, 100f))
        {
            if (localWarningCircle == null && landingWarningPrefab != null)
            {
                //localWarningCircle = Instantiate(landingWarningPrefab, hit.point + (hit.normal * 0.05f), 
                    //Quaternion.LookRotation(hit.normal));

                localWarningCircle = Instantiate(landingWarningPrefab, hit.point + (hit.normal * 0.1f), 
                    Quaternion.FromToRotation(Vector3.forward, -hit.normal));
            }

            if (localWarningCircle != null)
            {
                // Growing shadow
                float distanceToGround = Vector3.Distance(transform.position, hit.point);
                float scaleFactor = Mathf.Clamp01(1.0f - (distanceToGround / 100f));
                localWarningCircle.transform.localScale = Vector3.one * (scaleFactor * 10f);
            }
        }
    }
    */
}