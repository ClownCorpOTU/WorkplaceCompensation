using Fusion.Addons.Physics;
using UnityEngine;

/// <summary>
/// Self-explanatory. Maybe this shouldn't be a component?
/// </summary>
public class NetworkPlayerRespawn : MonoBehaviour
{
    // Injected
    private NetworkPlayer networkPlayer;
    private NetworkRigidbody3D networkRB;
    private Vector3 spawnPoint;
    
    
    public void Initialize(NetworkPlayer player, NetworkRigidbody3D rb, Vector3 spawn)
    {
        networkPlayer = player;
        networkRB = rb;
        spawnPoint = spawn;
    }

    public void Respawn(bool inPlace = false)
    {
        networkRB.Rigidbody.linearVelocity = Vector3.zero;
        networkRB.Rigidbody.angularVelocity = Vector3.zero;

        if (!inPlace)
        {
            networkRB.Teleport(spawnPoint, Quaternion.Euler(0f, 0f, 0f));
            networkPlayer.MakeActiveRagdoll();
        }
        else
        {
            networkPlayer.MakeActiveRagdoll();
        }
    }
}