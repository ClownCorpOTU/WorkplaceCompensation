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
    public string lobbyName = "Default";

    [SerializeField] private NetworkRunner networkRunnerPrefab;

    public Transform lobbyEntryContentParent;
    public GameObject lobbyEntryPrefab;
    public string gameplayScene = "FallExpo_FinalReview";
    
    public Dictionary<string, GameObject> lobbyEntriesDictionary = new Dictionary<string, GameObject>(); 

    void Awake()
    {
        _runnerInstance = gameObject.GetComponent<NetworkRunner>();

        if (lobbyEntryContentParent == null)
        {
            lobbyEntryContentParent = GameObject.Find("LobbyListContent").transform;
        }
    }

    void Start()
    {
        if (_runnerInstance == null)
        {
            _runnerInstance = Instantiate(networkRunnerPrefab);
        }

        _runnerInstance.AddCallbacks(this);
        _runnerInstance.JoinSessionLobby(SessionLobby.Shared, lobbyName);
    }

    
    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
    {

        DeleteEntry(sessionList);

        CompareEntries(sessionList);
        

    }

    /// <summary>
    /// Checks if the lobby is in session. If not, the lobby will be removed from the UI and Dictionary.
    /// </summary>
    /// <param name="sessionList"></param>
    void DeleteEntry(List<SessionInfo> sessionList)
    {
        bool isInSession = false;
        GameObject entryToDelete = null;

        // Check LobbyEntries for lobbies that are no longer in session list
        foreach (KeyValuePair<string, GameObject> kvp in lobbyEntriesDictionary)
        {
            string lobbyName = kvp.Key;
            foreach (SessionInfo sessionInfo in sessionList)
            {
                if (sessionInfo.Name == lobbyName)
                {
                    isInSession = true;
                    break;
                }
            }

            if (!isInSession)
            {
                entryToDelete = kvp.Value;
                lobbyEntriesDictionary.Remove(lobbyName); // Remove Lobby from entry
                Destroy(entryToDelete); // Delete lobby
            }
        }
    }

    void CompareEntries(List<SessionInfo> sessionList)
    {
        foreach (SessionInfo session in sessionList)
        {
            if (lobbyEntriesDictionary.ContainsKey(session.Name))
            {
                UpdateEntry(session);
            }
            else
            {
                CreateEntry(session);
            }
        }
    }

    /// <summary>
    /// Create new lobby entry and display it on the lobby UI.
    /// </summary>
    /// <param name="session"></param>
    void CreateEntry(SessionInfo session)
    {
        GameObject newEntry = GameObject.Instantiate(lobbyEntryPrefab, lobbyEntryContentParent);
        lobbyEntriesDictionary.Add(session.Name, newEntry);

        UpdateEntry(session, newEntry);
    }

    /// <summary>
    /// Updates lobby entry.
    /// </summary>
    /// <param name="session"></param>
    /// <param name="entry">The new entry UI prefab that will be updated. If it is null, get the existing entry from the dictionary and update it.</param>
    void UpdateEntry(SessionInfo session, GameObject entry = null)
    {
        if (entry == null)
        {
            lobbyEntriesDictionary.TryGetValue(session.Name, out entry);
        }
        
        LobbyEntry entryScript = entry.GetComponent<LobbyEntry>();

        entryScript.lobbyName.text = session.Name;
        entryScript.currentPlayerCount = session.PlayerCount;
        entryScript.maxPlayerCount = session.MaxPlayers;

        entryScript.UpdatePlayerCount();

        entryScript.joinButton.interactable = session.IsOpen;

        entry.SetActive(session.IsValid);
    }

    public void CreateNewSession()
    {
        int sessionInt = UnityEngine.Random.Range(1000,9999);

        string sessionName = $"Room-{sessionInt}";

        GameObject.Find("LobbyUI").SetActive(false);
        DontDestroyOnLoad(_runnerInstance.gameObject);
        SceneManager.LoadScene(gameplayScene);
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
        // Empty callback
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
        // Empty callback
    }

    public void OnSceneLoadStart(NetworkRunner runner)
    {
        // Empty callback
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
    }

    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message)
    {
        // Empty callback
    }
}
