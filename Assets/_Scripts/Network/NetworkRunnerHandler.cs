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

    [SerializeField] int defaultSessionPlayerCap = 4;
    public List<SessionInfo> sessionList = new List<SessionInfo>();

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
        if (SceneManager.GetActiveScene().name == "Lobby")
        {
            OnJoinLobby("MainLobbyList");
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
            CustomLobbyName = "MainLobbyList",
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

    public void OnJoinLobby(string lobbyListName)
    {
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

    private async Task JoinLobby(string lobbyListID = "MainLobbyList")
    {
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

        public async void JoinGame (SessionInfo sessionInfo)
    {
        await InitializeNetworkRunner(GameMode.Client, sessionInfo.Name, sessionInfo.MaxPlayers,
            NetAddress.Any(), SceneRef.FromIndex(SceneManager.GetActiveScene().buildIndex), null);
    }

    public async void CreateGame(string sessionName, int lobbyCap, string scenePath)
    {
        int buildIndex = SceneUtility.GetBuildIndexByScenePath(scenePath);

        await InitializeNetworkRunner(GameMode.Host, sessionName, lobbyCap,
            NetAddress.Any(), SceneRef.FromIndex(buildIndex), null);
    }

    public string GenerateLobbyCode(int length = 6)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        System.Random randomize = new System.Random();

        bool isNotUnique = true;
        string newCode;

        do
        {
            newCode = new string(Enumerable.Repeat(chars, length).Select(s => s[randomize.Next(s.Length)]).ToArray());

            if (sessionList.Count == 0)
            {
                return newCode;
            }

            foreach (SessionInfo session in sessionList)
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