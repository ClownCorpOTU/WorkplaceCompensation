using System;
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
    private NetworkRunnerHandler handler;

    public void Initialize(NetworkPlayer player, NetworkRigidbody3D rb)
    {
        if (handler == null) 
            handler = FindFirstObjectByType<NetworkRunnerHandler>();
        
        networkPlayer = player;
        networkRB = rb;
        spawnPoint = handler.SpawnPoint;
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