using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using Fusion.Sockets;
using System;
using UnityEngine.SceneManagement;
using UnityEditor;

public class NetworkManager : MonoBehaviour, INetworkRunnerCallbacks
{
    public NetworkRunner _runnerInstance;

    [SerializeField] private NetworkRunner networkRunnerPrefab;

    [Header("Lobbies")]
    [SerializeField] private NetworkRunnerHandler networkRunnerHandler;
    [SerializeField] private LobbyMenuManager lobbyMenuManager;
    [SerializeField] private int lobbyMenuBuildIndex = int.MinValue;
    [SerializeField] private int mainMenuBuildIndex = int.MinValue;

    void Awake()
    {
        if (networkRunnerHandler == null)
        {
            networkRunnerHandler = FindFirstObjectByType<NetworkRunnerHandler>();
        }

        if (lobbyMenuBuildIndex == int.MinValue)
        {
            lobbyMenuBuildIndex = SceneUtility.GetBuildIndexByScenePath("Assets/_Scenes/Menus/Lobby.unity");
        }

        if (mainMenuBuildIndex == int.MinValue)
        {
            mainMenuBuildIndex = SceneUtility.GetBuildIndexByScenePath("Assets/_Scenes/Menus/MainMenu.unity");
        }
    }

    public void OnConnectedToServer(NetworkRunner runner)
    {
        
    }

    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
    {
        
    }

    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)
    {
        
    }

    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data)
    {
        
    }

    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
    {
        Debug.Log($"Disconnected from sever: {reason}");
        
        if (SceneManager.GetActiveScene().buildIndex != lobbyMenuBuildIndex)
        {
            SceneManager.LoadScene(lobbyMenuBuildIndex);
        }
    }

    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
    {
        
    }

    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
        
    }

    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
    {
        
    }

    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {
        
    }

    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {
        
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        
    }

    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress)
    {
        
    }

    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data)
    {
        
    }

    public void OnSceneLoadDone(NetworkRunner runner)
    {
        Spawner spawner = FindFirstObjectByType<Spawner>();

        if (spawner != null)
        {
            runner.AddCallbacks(spawner);
        }
    }

    public void OnSceneLoadStart(NetworkRunner runner)
    {
        if (lobbyMenuManager != null)
        {
            lobbyMenuManager.gameObject.SetActive(false);
        }
    }
    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {

        if (shutdownReason == ShutdownReason.Ok)
        {
            return;
        }

        Debug.Log($"Network Shutdown Reason: {shutdownReason}");
    
        if (SceneManager.GetActiveScene().name == "Lobby")
        {
            FindFirstObjectByType<NetworkRunnerHandler>().OnJoinLobby("MainLobbyList");
        }

        if (_runnerInstance == runner)
        {
            _runnerInstance = null;
        }

        SceneManager.LoadScene(lobbyMenuBuildIndex);
    }

    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message)
    {
        
    }

    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
    {
        networkRunnerHandler.sessionList = sessionList;

        Debug.Log("Session list (NetworkManager) updated: " + sessionList.Count);

        lobbyMenuManager.ClearLobbyDisplay();

        if (sessionList.Count != 0)
        {
            //lobbyMenuManager.CompareEntries(sessionList);

            foreach (SessionInfo session in sessionList)
            {
                lobbyMenuManager.CreateEntry(session);
            }
        }


    }
}
