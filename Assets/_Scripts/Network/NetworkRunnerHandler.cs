using System;
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
    [SerializeField] private bool shouldStartInSinglePlayer = false;
    private NetworkRunner networkRunner;
    
    public Vector3 SpawnPoint => spawnPoint;

    [SerializeField] int defaultSessionPlayerCap = 4;

    private void Awake()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 120;
        
        if (networkRunner == null)
        {
            networkRunner = FindFirstObjectByType<NetworkRunner>();
        }
    }

    private void Start()
    {
        string sessionName = "";
        if (networkRunner == null)
        {
            networkRunner = Instantiate(networkRunnerPrefab);
            networkRunner.name = "NetworkRunner";
            networkRunner.gameObject.GetComponent<Spawner>().Initialize(spawnPoint);
            sessionName = "TestSession";
        }
        
        if (SceneManager.GetActiveScene().name == "Lobby")
        {
            return;
        }

        GameMode mode = shouldStartInSinglePlayer ? GameMode.Single : GameMode.AutoHostOrClient;
        if (SceneManager.GetActiveScene().name != "MainMenu")
        {
            var clientTask = InitializeNetworkRunner(networkRunner, mode, sessionName, defaultSessionPlayerCap, 
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

    protected virtual Task InitializeNetworkRunner(NetworkRunner runner, GameMode gameMode, string sessionName, int lobbyCap,
        NetAddress address, SceneRef scene, Action<NetworkRunner> initialized)
    {
        INetworkSceneManager sceneManager = GetSceneManager(runner);
        runner.ProvideInput = true;

        return runner.StartGame(new StartGameArgs()
        {
            GameMode = gameMode,
            Address = address,
            Scene = scene,
            SessionName = sessionName,
            CustomLobbyName = "MainLobbyList",
            SceneManager = sceneManager,
            PlayerCount = lobbyCap,
            IsOpen = true,
            IsVisible = true
        });
    }

    public void OnJoinLobby(string lobbyListName)
    {
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

    public void CreateGame (string sessionName, int lobbyCap, string scenePath)
    {
        int buildIndex = SceneUtility.GetBuildIndexByScenePath(scenePath);

        var clientTask = InitializeNetworkRunner(networkRunner, GameMode.Host, sessionName, lobbyCap,
            NetAddress.Any(), SceneRef.FromIndex(buildIndex), null);
    }

    public void JoinGame (SessionInfo sessionInfo)
    {
        var clientTask = InitializeNetworkRunner(networkRunner, GameMode.Client, sessionInfo.Name, sessionInfo.MaxPlayers,
            NetAddress.Any(), SceneRef.FromIndex(SceneManager.GetActiveScene().buildIndex), null);
    }
}