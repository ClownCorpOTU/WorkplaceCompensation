using UnityEngine;
using System.Collections.Generic;
using Fusion;
using System;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;
using Steamworks;
using System.Linq.Expressions;

/// <summary>
/// Handles the communication between the UI of menus and the creation of lobbies/rooms.
/// </summary>
public class LobbyRoomManager : MonoBehaviour 
{
    [Header("Managers")]
    [SerializeField] NetworkRunnerHandler networkRunnerHandler;
    //[SerializeField] SteamManager steamManager;

    [Header("Tutorial Lobbies")]
    [SerializeField] string tutorialServerName = "Server_Tutorial_Lobby_List";
    [SerializeField] string tutorialLobbyName;
    [SerializeField] string tutorialSceneName;
    [SerializeField] int tutorialMaxPlayerCap = 16;

    [Header("General Lobby")]
    [SerializeField] public string serverName {get; private set;} = "Server_Testing_Lobby_List";
    [SerializeField] int lobbyMaxPlayerCap = 16;

    [Header("Matchmaking Settings")]
    [Tooltip("If true, clicking a map will attempt to join an existing lobby for that map instead of creating a new one.")]
    [SerializeField] bool autoJoinExistingMapLobby = true;

    public void Awake()
    {
        if (networkRunnerHandler == null)
        {
            networkRunnerHandler = FindFirstObjectByType<NetworkRunnerHandler>();
        }

        // if (steamManager == null)
        // {
        //     steamManager = FindFirstObjectByType<SteamManager>();
        // }
    }

    /// <summary>
    /// Create a room for players as a tutorial/play stage.
    /// </summary>
    public void CreateTutorialRoom()
    {
        tutorialLobbyName = $"{SteamFriends.GetPersonaName()}'s Room";

        networkRunnerHandler.OnJoinLobbyList(tutorialServerName);

        if (tutorialSceneName == "")
        {
            tutorialSceneName = "Test_MarsCanyon";
        }
        //networkRunnerHandler.CreateGame(tutorialLobbyName, tutorialMaxPlayerCap, SceneManager.GetSceneByName(tutorialSceneName), true);
    }

    /// <summary>
    /// Create new game room.
    /// </summary>
    /// <param name="levelName">Name of the scene.</param>
    public void CreateNewRoom(string levelName)
    {
        if (autoJoinExistingMapLobby && networkRunnerHandler.sessionList != null)
        {
            foreach (var session in networkRunnerHandler.sessionList)
            {
                // 2. Ensure the room is open and not full
                if (session.IsOpen && session.PlayerCount < session.MaxPlayers)
                {
                    // 3. Check if the map name matches the button clicked
                    if (session.Properties.TryGetValue("MapName", out var sessionMapName) && (string)sessionMapName == levelName)
                    {
                        Debug.Log($"Found existing room for {levelName}. Joining as client.");
                        networkRunnerHandler.JoinGame(session);
                        return; // Exit the method early so we don't host a new room
                    }
                }
            }
        }

        networkRunnerHandler.OnJoinLobbyList(serverName);

        string lobbyName = $"Test_{levelName}";
        networkRunnerHandler.CreateGame(lobbyName, lobbyMaxPlayerCap, GetBuildIndexByName(levelName), levelName, serverName);
    }

    // HELPERS
    int GetBuildIndexByName(string levelName)
    {
        int buildIndex = -1;
        switch (levelName)
        {
            case "WinterWeek2_ShaderTests":
                {
                    buildIndex = 1;
                    break;
                }
            case "Test_MarsCanyon":
                {
                    buildIndex = 2;
                    break;
                }
            default:
                {
                    buildIndex = -1;
                    break;
                }
        }

        return buildIndex;
    }

}