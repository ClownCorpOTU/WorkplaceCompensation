using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Fusion;
using System;

public class LobbyEntry : MonoBehaviour
{
    [Header("Managers")]
    [SerializeField] LobbyMenuManager lobbyManager;

    [Header("Lobby Entry UI Info")]
    [SerializeField] TextMeshProUGUI lobbyName, playerCount;
    public Button joinButton;
    public int currentPlayerCount = 0, maxPlayerCount = 1;

    [Header("Session")]
    SessionInfo sessionInfo;
    public event Action<SessionInfo> OnJoinSession;

    void Awake()
    {
        if (lobbyManager != null)
        {
            lobbyManager = GameObject.FindFirstObjectByType<LobbyMenuManager>();
        }    
    }

    /// <summary>
    /// Update the entry that is displayed.
    /// </summary>
    /// <param name="name">String of the lobby name.</param>
    /// <param name="playerInLobby">Number of players in the lobby, currently.</param>
    /// <param name="playerCap">Maximum players allowed in the lobby.</param>
    /// <param name="map">String of the scene's name that the player will see.</param>
    public void UpdateEntryInfo(string name, SessionInfo info, string map = null)
    {
        lobbyName.text = name;
        sessionInfo = info;

        UpdatePlayerCount(sessionInfo);
    }
    /// <summary>
    /// Update the player count text to match the current player count.
    /// </summary>
    public void UpdatePlayerCount(SessionInfo info)
    {
        currentPlayerCount = info.PlayerCount;
        maxPlayerCount = info.MaxPlayers;
        
        if (maxPlayerCount <= 0)
        {
            maxPlayerCount = 1;
        }

        playerCount.text = $"{currentPlayerCount}/{maxPlayerCount}";
    }

    public void SessionInformation(SessionInfo info)
    {
        this.sessionInfo = info;

        UpdateEntryInfo(info.Name, info);

        bool canPlayerJoin = true;
        if (currentPlayerCount >= maxPlayerCount)
        {
            canPlayerJoin = false;
        }
        //joinButton.gameObject.SetActive(canPlayerJoin);
    }

    public void OnJoinRoomClick()
    {
        if (currentPlayerCount >= maxPlayerCount)
        {
            Debug.LogError("Lobby is full.");
            return;
        }

        UpdatePlayerCount(sessionInfo);

        NetworkRunnerHandler networkRunnerHandler = FindFirstObjectByType<NetworkRunnerHandler>();

        //networkRunnerHandler.OnJoinLobby("MainLobbyList");
        networkRunnerHandler.JoinGame(sessionInfo);
    }

    public void OnClick()
    {
        OnJoinSession?.Invoke(this.sessionInfo);
    }

    public void ExitRoom()
    {
        UpdatePlayerCount(sessionInfo);
    }
}
