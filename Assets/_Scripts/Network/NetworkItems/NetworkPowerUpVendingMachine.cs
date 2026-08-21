using Fusion;
using UnityEngine;

public class NetworkPowerUpVendingMachine : NetworkBehaviour
{
    [SerializeField] private GameObject[] pickupPrefabs;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private float spawnCooldown = 60f;
    
    [Networked] private TickTimer spawnTimer { get; set; }

    private AudioManager audioManager;
    
    
    public override void Spawned()
    {
        audioManager = FindFirstObjectByType<AudioManager>();
        
        if (Object.HasStateAuthority)
            spawnTimer = TickTimer.CreateFromSeconds(Runner, spawnCooldown);
    }

    public override void FixedUpdateNetwork()
    {
        if (!Object.HasStateAuthority) return;
        
        if (spawnTimer.Expired(Runner))
        {
            int randomIndex = Random.Range(0, pickupPrefabs.Length);
            GameObject prefabToSpawn = pickupPrefabs[randomIndex];
            
            NetworkObject spawnedObj = Runner.Spawn(prefabToSpawn, spawnPoint.position, spawnPoint.rotation);

            if (spawnedObj.TryGetComponent(out Rigidbody rb))
            {
                Vector3 spitDirection = (spawnPoint.forward + Vector3.up).normalized;
                rb.AddForce(spitDirection * 5f, ForceMode.Impulse);
                
                rb.AddTorque(Random.insideUnitSphere * 5f, ForceMode.Impulse);
            }
            
            RPC_Play("VendingMachinePowerUpSpawn",  spawnPoint.position);
            
            spawnTimer = TickTimer.CreateFromSeconds(Runner, spawnCooldown);
        }
    }
    
    [Rpc(RpcSources.StateAuthority, RpcTargets.All, TickAligned = false)]
    private void RPC_Play(string audioName, Vector3 position)
    {
        if (audioManager != null) audioManager.Play(audioName, position);
    }
}