using System;
using System.Threading.Tasks;
using Fusion;
using Unity.Cinemachine;
using UnityEngine;
using Random = UnityEngine.Random;

public class NetworkMeteor : NetworkBehaviour
{
    [Header("Meteor Settings")]
    [SerializeField] private float selfDestructTime = 12f;
    [SerializeField] private GameObject breakVfxPrefab;
    [SerializeField] private GameObject landingWarningPrefab;

    [Header("Impact Settings")] 
    [SerializeField] private float explosionRadius = 5f;
    [SerializeField] private float baseShakeForce = 2.2f;
    [SerializeField] private float maxShakeDistance = 50f;
    
    [Networked] private Vector3 networkedVelocity { get; set; }
    [Networked] private Vector3 targetPos { get; set; }
    [Networked] private byte impactSignal { get; set; }
    [Networked] private TickTimer selfDestructTimer { get; set; }

    private ChangeDetector changes;
    private AudioManager audioManager;
    private CameraShakeManager camShakeManager;
    private GameObject localWarningCircle;
    private CinemachineImpulseSource thisImpulseSource;
    private bool isDespawning = false;
    
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

        if (Object.HasStateAuthority)
        {
            selfDestructTimer = TickTimer.CreateFromSeconds(Runner, selfDestructTime);
        }
    }
    
    public override void FixedUpdateNetwork()
    {
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

    private async void TriggerImpact()
    {
        isDespawning = true;
        impactSignal++;

        ApplyRadialImpact();

        await Task.Delay(150);
        
        if (Object != null && Runner != null && Runner.IsRunning)
            Runner.Despawn(Object);
    }

    private void ApplyRadialImpact()
    {
        Collider[] hits = new Collider[10];

        var hitCount = Runner.GetPhysicsScene().OverlapSphere(
            transform.position,
            explosionRadius,
            hits,
            -1,
            QueryTriggerInteraction.UseGlobal
        );

        for (int i = 0; i < hitCount; i++)
        {
            var player = hits[i].GetComponent<NetworkPlayer>();
            if (player != null) player.FlattenAndMakeRagdoll();
        }
    }

    private void PlayImpactEffects()
    {
        // Play sound
        if (audioManager != null) audioManager.Play("RockBreak", transform.position);
        
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
            Destroy(vfx, 5f);
        }
        
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
        // Go 50 units above the targetPos, and raycast 100 units down to find any points on the terrain
        if (Physics.Raycast(targetPos + Vector3.up * 50f, Vector3.down, out RaycastHit hit, 100f))
        {
            if (localWarningCircle == null && landingWarningPrefab != null)
            {
                localWarningCircle = Instantiate(landingWarningPrefab, hit.point + (hit.normal * 0.05f), 
                    Quaternion.LookRotation(hit.normal));
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
}