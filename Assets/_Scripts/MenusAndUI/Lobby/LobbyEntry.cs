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
    /// Update the player count text to match the current player count.
    /// </summary>
    public void UpdatePlayerCount()
    {
        currentPlayerCount = sessionInfo.PlayerCount;

        if (currentPlayerCount <= 0)
        {
            Destroy(this.gameObject);
        }

        maxPlayerCount = sessionInfo.MaxPlayers;
        
        if (maxPlayerCount <= 0)
        {
            maxPlayerCount = 1;
        }

        playerCount.text = $"{currentPlayerCount}/{maxPlayerCount}";
    }

    /// <summary>
    /// Gets the session information.
    /// </summary>
    /// <param name="info"></param>
    public void SetSessionInformation(SessionInfo info)
    {
        this.sessionInfo = info;

        sessionInfo.Properties.TryGetValue("displayName", out var displayName);

        lobbyName.text = displayName;
        UpdatePlayerCount();

        bool canPlayerJoin = true;
        if (currentPlayerCount >= maxPlayerCount)
        {
            canPlayerJoin = false;
        }
        //joinButton.gameObject.SetActive(canPlayerJoin);
    }

    /// <summary>
    /// Handles players joining lobbies.
    /// </summary>
    public void OnJoinRoomClick()
    {
        if (currentPlayerCount >= maxPlayerCount)
        {
            Debug.LogError("Lobby is full.");
            return;
        }

        UpdatePlayerCount();

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
        UpdatePlayerCount();
    }
}
