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
    [SerializeField] private LobbyMenuManager lobbyMenuManager;
    [SerializeField] private int lobbyMenuBuildIndex = int.MinValue;
    [SerializeField] private int mainMenuBuildIndex = int.MinValue;

    void Awake()
    {
        if (_runnerInstance == null)
        {
            _runnerInstance = Instantiate(networkRunnerPrefab);
        }

        _runnerInstance.AddCallbacks(this);
        
        //_runnerInstance = gameObject.GetComponent<NetworkRunner>();

        if (lobbyMenuBuildIndex == int.MinValue)
        {
            lobbyMenuBuildIndex = SceneUtility.GetBuildIndexByScenePath("Assets/_Scenes/Menus/Lobby.unity");
            Debug.Log($"=> lobby build index = {lobbyMenuBuildIndex}.");
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
        throw new NotImplementedException();
    }

    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)
    {
        throw new NotImplementedException();
    }

    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data)
    {
        throw new NotImplementedException();
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
        throw new NotImplementedException();
    }

    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
        throw new NotImplementedException();
    }

    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
    {
        throw new NotImplementedException();
    }

    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {
        throw new NotImplementedException();
    }

    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {
        throw new NotImplementedException();
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        throw new NotImplementedException();
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        throw new NotImplementedException();
    }

    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress)
    {
        throw new NotImplementedException();
    }

    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data)
    {
        throw new NotImplementedException();
    }

    public void OnSceneLoadDone(NetworkRunner runner)
    {
        throw new NotImplementedException();
    }

    public void OnSceneLoadStart(NetworkRunner runner)
    {
        throw new NotImplementedException();
    }
    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
        if (SceneManager.GetActiveScene().name == "Lobby")
        {
            FindFirstObjectByType<NetworkRunnerHandler>().OnJoinLobby("MainLobbyList");
        }

        SceneManager.LoadScene(lobbyMenuBuildIndex);
    }

    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message)
    {
        throw new NotImplementedException();
    }

    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
    {
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
