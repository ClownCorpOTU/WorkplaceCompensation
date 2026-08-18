using UnityEngine;
using Fusion;

public class NetworkHazard : NetworkBehaviour
{
    [SerializeField] private bool destroyOnHit = false;

    private void OnTriggerEnter(Collider other)
    {
        // Only the Host/Server should handle the logic of "Who gets hurt"
        if (!Object.HasStateAuthority) return;

        // Look for the NetworkPlayer component
        if (other.transform.root.TryGetComponent(out NetworkPlayer player))
        {
            player.Burn();
            
            if (destroyOnHit) Runner.Despawn(Object);
        }
    }
}