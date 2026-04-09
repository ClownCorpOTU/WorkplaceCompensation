using System;
using Fusion;
using UnityEngine;

public class NetworkOutputReceiver : NetworkBehaviour
{
    [Header("Parameters")]
    [SerializeField] private float flyDelay = 0.5f;
    [SerializeField] private float flySpeed = 5f;
    [SerializeField] private float despawnDelay = 3f;
    
    [Header("Juice")]
    [SerializeField] private GameObject windPrefab;
    [SerializeField] private Transform windSpawnPoint;
    [SerializeField] private float fxDespawnDelay = 15;
    [SerializeField] private string suctionAudioName = "Suction"; // Temporary since I'm using this on Mars for the UFO

    [Networked] private TickTimer flyDelayTimer { get; set; }
    [Networked] private TickTimer despawnTimer { get; set; }
    [Networked] private NetworkObject vialToDespawn { get; set; }

    private NetworkGameManager networkGameManager;
    private bool hasFlown;
    
    
    public override void Spawned()
    {
        networkGameManager = FindFirstObjectByType<NetworkGameManager>();
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (!Object.HasStateAuthority) return;

        if (other.TryGetComponent(out Vial vial) && vial.Type == VialType.OutputBox)
        {
            // Reset all states before starting new sequence
            flyDelayTimer = TickTimer.None;
            despawnTimer = TickTimer.None;
            hasFlown = false;
            
            // Play juice
            RPC_PlayWind();
            
            
            // Record the vial object and start the first timer
            vialToDespawn = vial.Object;
            flyDelayTimer = TickTimer.CreateFromSeconds(Runner, flyDelay);
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (!Object.HasStateAuthority) return;

        // Handle flying after short delay
        if (!hasFlown && flyDelayTimer.Expired(Runner) && vialToDespawn != null)
        {
            if (vialToDespawn.TryGetComponent(out Rigidbody rb))
            {
                rb.AddForce(Vector3.up * flySpeed, ForceMode.Impulse);
            }

            hasFlown = true;
            despawnTimer = TickTimer.CreateFromSeconds(Runner, despawnDelay);
        }

        // Handle despawn after the second delay
        if (despawnTimer.Expired(Runner) && vialToDespawn != null)
        {
            Vial v = vialToDespawn.gameObject.GetComponent<Vial>();
            networkGameManager.AddScore(v.LastHeldBy, 2);
            Runner.Despawn(vialToDespawn);
            vialToDespawn = null;
            v = null;
        }
    }
    
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_PlayWind()
    {
        AudioManager.instance.Play(suctionAudioName, transform.position);

        if (windPrefab == null || windSpawnPoint == null) return;

        GameObject fx = Instantiate(windPrefab, windSpawnPoint.position, Quaternion.Euler(-90f,0f,0f));
        
        // Auto-destroy if vfx didn't destory itself
        if (fx != null) Destroy(fx, fxDespawnDelay);
    }
}