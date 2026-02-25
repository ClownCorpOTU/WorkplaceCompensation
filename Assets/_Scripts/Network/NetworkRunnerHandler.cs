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
            var clientTask = InitializeNetworkRunner(networkRunner, mode, sessionName, 
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

    protected virtual Task InitializeNetworkRunner(NetworkRunner runner, GameMode gameMode, string sessionName,
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
            CustomLobbyName = sessionName,
            SceneManager = sceneManager
        });
    }

    public void OnJoinLobby(string lobbyName)
    {
        var clientTask = JoinLobby(lobbyName);
    }

    private async Task JoinLobby(string lobbyID)
    {
        var result = await networkRunner.JoinSessionLobby(SessionLobby.Custom, lobbyID);

        if (!result.Ok)
        {
            UnityEngine.Debug.LogError($"Unable to join lobby {lobbyID}.");
        }
    }

    public void CreateGame (string sessionName, string scenePath)
    {
        int buildIndex = SceneUtility.GetBuildIndexByScenePath(scenePath);

        var clientTask = InitializeNetworkRunner(networkRunner, GameMode.Host, sessionName, 
            NetAddress.Any(), SceneRef.FromIndex(buildIndex), null);
    }

    public void JoinGame (SessionInfo sessionInfo)
    {
        var clientTask = InitializeNetworkRunner(networkRunner, GameMode.Client, sessionInfo.Name, 
            NetAddress.Any(), SceneRef.FromIndex(SceneManager.GetActiveScene().buildIndex), null);
    }
}