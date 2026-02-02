using System;
using System.Linq;
using System.Threading.Tasks;
using Fusion;
using Fusion.Sockets;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// This script handles the creation of the NetworkRunner which is the main entry point for the game.
/// It also handles the scene loading.
/// </summary>
public class NetworkRunnerHandler : MonoBehaviour
{
    [SerializeField] private NetworkRunner networkRunnerPrefab;
    private NetworkRunner networkRunner;

    private void Awake()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 120;
        
        if (networkRunner != null)
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
        }
        
        if (SceneManager.GetActiveScene().name != "MainMenu")
        {
        var clientTask = InitializeNetworkRunner(networkRunner, GameMode.AutoHostOrClient, sessionName, 
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
            CustomLobbyName = runner.name,
            SceneManager = sceneManager
        });
    }


    public void OnJoinLobby()
    {
        var clientTask = JoinLobby();
    }

    private async Task JoinLobby()
    {
        string lobbyID = "DefaultLobbyID";

        var result = await networkRunner.JoinSessionLobby(SessionLobby.Custom, lobbyID);

        if (!result.Ok)
        {
            Debug.LogError($"Unable to join lobby {lobbyID}");
        }
    }

    public void CreateGame(string sessionName, string sceneName)
    {
        var clientTask = InitializeNetworkRunner(networkRunner, GameMode.Host, sessionName, 
            NetAddress.Any(), SceneRef.FromIndex(SceneManager.GetActiveScene().buildIndex), null);
    }

    public void JoinGame(SessionInfo sessionInfo)
    {
        var clientTask = InitializeNetworkRunner(networkRunner, GameMode.Client, sessionInfo.Name, 
            NetAddress.Any(), SceneRef.FromIndex(SceneManager.GetActiveScene().buildIndex), null);
    }
}