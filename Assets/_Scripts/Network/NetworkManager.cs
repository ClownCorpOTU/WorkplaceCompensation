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
    public static NetworkRunner _runnerInstance;

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

        if (lobbyMenuManager == null && SceneManager.GetActiveScene().buildIndex == lobbyMenuBuildIndex)
        {
            lobbyMenuManager = FindAnyObjectByType<LobbyMenuManager>();
        }
    }


    public void OnConnectedToServer(NetworkRunner runner)
    {
        
    }

    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
    {
        // Empty callback
    }

    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)
    {
        // Empty callback
    }

    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data)
    {
        // Empty callback
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
        // Empty callback
    }

    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
        // Empty callback
    }

    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
    {
        // Empty callback
    }

    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {
        // Empty callback
    }

    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {
        // Empty callback
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        // Empty callback
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        // Empty callback
    }

    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress)
    {
        // Empty callback
    }

    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data)
    {
        // Empty callback
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
        // Now that you've added .AddCallbacks(this), you will see this print!
        Debug.Log($"<color=orange>NetworkManager:</color> Game Shutdown! Reason: {shutdownReason}");
    
        // 0 is 'Ok' (the user left normally). 
        // Anything else means a disconnect, host-quit, or error.
        if (shutdownReason != ShutdownReason.Ok)
        {
            var uiManager = FindFirstObjectByType<LocalPlayerUIManager>();
            if (uiManager != null)
            {
                uiManager.ShowHostLeftScreen();
            }
        }

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        if (shutdownReason == ShutdownReason.Ok)
        {
            return;
        }

        Debug.Log($"Network Shutdown Reason: {shutdownReason}");
        

        if (SceneManager.GetActiveScene().name != "Lobby")
        {
            SceneManager.LoadScene(lobbyMenuBuildIndex);
        }
        else
        {
            if (networkRunnerHandler != null)
            {
                networkRunnerHandler.OnJoinLobby("MainLobbyList");
            }
        }
    }

    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message)
    {
        // Empty callback
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
