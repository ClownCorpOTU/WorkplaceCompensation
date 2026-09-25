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
        networkRunnerHandler.OnJoinLobbyList(serverName);

        string lobbyName = $"Test_{levelName}";

        networkRunnerHandler.CreateGame(lobbyName, lobbyMaxPlayerCap, GetBuildIndexByName(levelName), levelName);
    }

    public void JoinSelectedRoom()
    {
        
    }

    public void JoinRoomByCode(string code)
    {
        
    }

    public void DeleteSelectedRoom()
    {
        
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