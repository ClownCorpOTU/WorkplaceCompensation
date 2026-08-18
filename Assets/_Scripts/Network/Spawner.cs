using System;
using System.Collections.Generic;
using System.Linq;
using Fusion;
using Fusion.Sockets;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Spawns the player and collects local input to send to the host.
/// </summary>
public class Spawner : SimulationBehaviour, INetworkRunnerCallbacks
{
    [SerializeField] private NetworkPlayer regularBlobbyPrefab;
    [SerializeField] private NetworkPlayer martianBlobbyPrefab;
    [SerializeField] private string level1Name, level2Name;
    
    private NetworkGameManager networkGameManager;
    private NetworkPlayer playerToSpawn;
    private Vector3 spawnPoint;

    public void Initialize(Vector3 pos, NetworkPlayer playerPrefabOverride = null)
    {
        networkGameManager = FindFirstObjectByType<NetworkGameManager>();
        spawnPoint = pos;
        playerToSpawn = playerPrefabOverride ? playerPrefabOverride : regularBlobbyPrefab;
    }
    
    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {
        
    }

    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {
        
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        if (runner.IsServer)
        {
            var sceneName = SceneManager.GetActiveScene().name;
            // Only spawn Martian Blobby on the second level. For everything else, use regular
            playerToSpawn = (sceneName == level2Name) ? martianBlobbyPrefab : regularBlobbyPrefab; 
            
            var spawnedPlayer = runner.Spawn(playerToSpawn.gameObject, spawnPoint, Quaternion.identity, player);
            var spawnedNetworkPlayer = spawnedPlayer.GetComponent<NetworkPlayer>();
            spawnedNetworkPlayer.AssignPlayerIdentity(player);
            
            if (networkGameManager != null)
                networkGameManager.NetworkPlayers.Add(player,  spawnedNetworkPlayer);
        }

        var activePlayers = Runner.ActivePlayers.Count();
        if (DiscordManager.Instance != null) DiscordManager.Instance.UpdatePlayerCount(activePlayers);
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        if (runner.IsServer && networkGameManager != null)
            networkGameManager.NetworkPlayers.Remove(player);
            
        var activePlayers = Runner.ActivePlayers.Count();
        if (DiscordManager.Instance != null) DiscordManager.Instance.UpdatePlayerCount(activePlayers);
    }

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
        
    }

    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
    {
        
    }

    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)
    {
        
    }

    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
    {
        
    }

    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message)
    {
        
    }

    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data)
    {
        
    }

    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress)
    {
        
    }

    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
        if (NetworkPlayer.Local != null) 
            input.Set(NetworkPlayer.Local.GetNetworkInput());
    }

    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
    {
        
    }

    public void OnConnectedToServer(NetworkRunner runner)
    {
        
    }

    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
    {
        
    }

    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data)
    {
        
    }

    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
    {
        
    }

    public void OnSceneLoadDone(NetworkRunner runner)
    {
        
    }

    public void OnSceneLoadStart(NetworkRunner runner)
    {
        
    }
}
