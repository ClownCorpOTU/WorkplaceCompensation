using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using Fusion;
using Fusion.Sockets;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// This script handles the creation of the NetworkRunner which is the main entry point for the game.
/// It also handles the scene loading.
/// </summary>
public class NetworkRunnerHandler : MonoBehaviour
{
    [SerializeField] private NetworkRunner networkRunnerPrefab;
    [SerializeField] private Vector3 spawnPoint;
    [SerializeField] private NetworkPlayer networkPlayerPrefabOverride = null;
    [SerializeField] private bool shouldStartInSinglePlayer = false;
    private NetworkRunner networkRunner;
    
    public Vector3 SpawnPoint => spawnPoint;

    [Header ("Lobbies")]
    [SerializeField] int defaultSessionPlayerCap = 4;
    public List<SessionInfo> sessionList = new List<SessionInfo>();
    public string MainLobbyListName = "MainLobbyList";
    [SerializeField] bool doStartSessionOnScenePlay = false;

    private void Awake()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 120;
        
        if (networkRunner == null)
        {
            networkRunner = FindFirstObjectByType<NetworkRunner>();
        }

        sessionList.Clear();
    }

    private void Start()
    {
        if (!doStartSessionOnScenePlay)
        {
            OnJoinLobbyList(MainLobbyListName);
            return;
        }

        if (networkRunner != null && networkRunner.IsRunning || networkRunner.IsCloudReady)
        {
            return;
        }

        string sessionName = $"DirectLoad{SceneManager.GetActiveScene().name}";

        if (SceneManager.GetActiveScene().name != "MainMenu")
        {
            GameMode mode = shouldStartInSinglePlayer ? GameMode.Single : GameMode.AutoHostOrClient;

            var clientTask = InitializeNetworkRunner(mode, sessionName, defaultSessionPlayerCap, 
                NetAddress.Any(), SceneRef.FromIndex(SceneManager.GetActiveScene().buildIndex), null);
        }
    }

    private INetworkSceneManager GetSceneManager(NetworkRunner runner)
    {
        // Get scene manager
        INetworkSceneManager sceneManager =
            runner.GetComponents(typeof(MonoBehaviour)).OfType<INetworkSceneManager>().FirstOrDefault();
        
        // Add one if not found
        return sceneManager ?? runner.gameObject.AddComponent<NetworkSceneManagerDefault>();
    }

    protected virtual async Task InitializeNetworkRunner(GameMode gameMode, string sessionName, int lobbyCap,
        NetAddress address, SceneRef scene, Action<NetworkRunner> initialized, int codeLen = 6)
    {
        NetworkManager networkManager = FindFirstObjectByType<NetworkManager>();

        if (networkRunner != null)
        {
            if (networkManager != null) networkRunner.RemoveCallbacks(networkManager);

            await networkRunner.Shutdown(destroyGameObject: true);
            networkRunner = null;
        }

        networkRunner = Instantiate(networkRunnerPrefab);
        networkRunner.name = "NetworkRunner";   

        Spawner spawner = networkRunner.GetComponent<Spawner>();
        networkRunner.AddCallbacks(spawner);

        if (networkManager != null) networkRunner.AddCallbacks(networkManager);

        spawner.Initialize(spawnPoint, networkPlayerPrefabOverride);
        networkRunner.ProvideInput = true;

        INetworkSceneManager sceneManager = GetSceneManager(networkRunner);
        networkRunner.ProvideInput = true;

        string joinCode = "";
        string uniqueName = "";
        if (gameMode == GameMode.Host)
        {
            uniqueName = $"Room_{System.Guid.NewGuid()}_{sessionName}";
            joinCode = GenerateLobbyCode(codeLen);
        }
        else
        {
            uniqueName = sessionName;
        }

        var result = await networkRunner.StartGame(new StartGameArgs()
        {
            GameMode = gameMode,
            Address = address,
            Scene = scene,
            SessionName = uniqueName,
            CustomLobbyName = MainLobbyListName,
            SceneManager = sceneManager,
            PlayerCount = lobbyCap,
            IsOpen = true,
            IsVisible = true,
            SessionProperties = new Dictionary<string, SessionProperty>()
            {
                {"DisplayName", sessionName},
                {"JoinCode", joinCode}
            }
        });
    }

    /// <summary>
    /// Joins the lobby list.
    /// </summary>
    /// <param name="lobbyListName">The name of the lobby list you want to join (EU, NA, SEA, etc.)</param>
    public void OnJoinLobbyList(string lobbyListName)
    {
        // Check if a runner already exists and is busy
        if (networkRunner != null && (networkRunner.IsRunning || networkRunner.IsCloudReady))
        {
            UnityEngine.Debug.LogWarning("Runner is already busy. Ignoring JoinLobby request.");
            return;
        }

        if (networkRunner == null)
        {
            networkRunner = Instantiate(networkRunnerPrefab);
            networkRunner.name = "LobbyRunner";

            NetworkManager networkManager = FindFirstObjectByType<NetworkManager>();
            if (networkManager != null)
            {
                networkRunner.AddCallbacks(networkManager);
            }
        }

        var clientTask = JoinLobby(lobbyListName);
    }

    /// <summary>
    /// Task to create the lobby list and/or join it.
    /// </summary>
    /// <param name="lobbyListID">TThe name of the lobby list you want to join (EU, NA, SEA, etc.)</param>
    /// <returns></returns>
    private async Task JoinLobby(string lobbyListID)
    {
        if (lobbyListID == "")
        {
            lobbyListID = MainLobbyListName;
        }

        var result = await networkRunner.JoinSessionLobby(SessionLobby.Custom, lobbyListID);

        if (!result.Ok)
        {
            UnityEngine.Debug.LogError($"Unable to join lobby {lobbyListID}.");
        }
        else
        {
            UnityEngine.Debug.Log($"Joined lobby list of {lobbyListID}.");
        }
    }

    /// <summary>
    /// Joins a lobby.;
    /// </summary>
    /// <param name="sessionInfo"></param>
    public async void JoinGame (SessionInfo sessionInfo)
    {
        await InitializeNetworkRunner(GameMode.Client, sessionInfo.Name, sessionInfo.MaxPlayers,
            NetAddress.Any(), SceneRef.FromIndex(SceneManager.GetActiveScene().buildIndex), null);
    }

    /// <summary>
    /// Join a lobby using a join code.
    /// </summary>
    /// <param name="joinCode">The join code.</param>
    public async void JoinGameByCode(string joinCode)
    {
        foreach(SessionInfo session in sessionList)
        {
            session.Properties.TryGetValue("JoinCode", out var code);
            if (joinCode == code)
            {
                JoinGame(session);

                return;
            }
        }

        UnityEngine.Debug.LogError($"No such lobby with join code [{joinCode}] exists.");
    }

    /// <summary>
    /// Create the lobby.
    /// </summary>
    /// <param name="sessionName">Name of the lobby.</param>
    /// <param name="lobbyCap">Max number of players allowed in.</param>
    /// <param name="scenePath">Path to the map scene.</param>
    public async void CreateGame(string sessionName, int lobbyCap, string scenePath)
    {
        int buildIndex = SceneUtility.GetBuildIndexByScenePath(scenePath);

        if (buildIndex == -1) 
        {
            UnityEngine.Debug.LogError("Scene not found in Build Settings! Check the path string.");
            return;
        }

        await InitializeNetworkRunner(GameMode.Host, sessionName, lobbyCap,
            NetAddress.Any(), SceneRef.FromIndex(buildIndex), null);
    }

    /// <summary>
    /// Create a lobby join code.
    /// </summary>
    /// <param name="length">How long the join code should be. The length is 6 by default.</param>
    /// <returns></returns>
    public string GenerateLobbyCode(int length = 6)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        System.Random randomize = new System.Random();

        bool isNotUnique = true;
        string newCode = "";

        List<SessionInfo> sessionListSnapshot = sessionList.ToList();

        do
        {
            newCode = new string(Enumerable.Repeat(chars, length).Select(s => s[randomize.Next(s.Length)]).ToArray());

            if (sessionListSnapshot.Count == 0)
            {
                return newCode;
            }

            isNotUnique = false;

            foreach (SessionInfo session in sessionListSnapshot)
            {
                session.Properties.TryGetValue("JoinCode", out var sessionCode);

                if (sessionCode.ToString() == newCode)
                {
                    isNotUnique = true;
                    break;
                }
            }
        } while(isNotUnique);

        return newCode;
    }
}