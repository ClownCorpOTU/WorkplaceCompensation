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
    int currentPlayerCount = 0, maxPlayerCount = 1;

    [Header("Session Info")]
    SessionInfo sessionInfo;
    public string joinCode {get; private set; }

    public event Action<SessionInfo> OnJoinSession;

    void Awake()
    {
        if (lobbyManager != null)
        {
            lobbyManager = GameObject.FindFirstObjectByType<LobbyMenuManager>();
        }

        joinCode = string.Empty;
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
    public void UpdateLobbyInformation(SessionInfo info)
    {
        this.sessionInfo = info;

        sessionInfo.Properties.TryGetValue("DisplayName", out var displayName);

        lobbyName.text = displayName;
        UpdatePlayerCount();

        sessionInfo.Properties.TryGetValue("JoinCode", out var code);
        joinCode = code;
        Debug.Log($"=> Join Code for Lobby {displayName.ToString()} is {joinCode}.");


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

        if (networkRunnerHandler != null)
        {
            networkRunnerHandler.JoinGame(sessionInfo);
        }
    }

    public void OnClick()
    {
        OnJoinSession?.Invoke(this.sessionInfo);
    }
}
