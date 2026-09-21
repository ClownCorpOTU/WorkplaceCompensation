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
    public TMP_Text lobbyNameText;
    public TMP_Text playerCountText;
    public Button joinButton;
    public int currentPlayerCount = 0, maxPlayerCount = 1;

    [Header("Session Info")]
    SessionInfo sessionInfo;
    public string joinCode { get; private set; }

    public event Action<SessionInfo> OnJoinSession;

    void Awake()
    {
        if (lobbyManager != null)
        {
            lobbyManager = FindFirstObjectByType<LobbyMenuManager>();
        }

        if (lobbyNameText == null)
        {
            foreach (Transform child in this.transform)
            {
                if (child.name == "LobbyNameDisplay")
                {
                    lobbyNameText = child.GetComponent<TMP_Text>();
                }
            }
        }

        if (playerCountText == null)
        {
            foreach (Transform child in this.transform)
            {
                if (child.name == "PlayerCountDisplay")
                {
                    playerCountText = child.GetComponent<TMP_Text>();
                }
            }
        }

        if (joinButton == null)
        {
            foreach (Transform child in this.transform)
            {
                if (child.name == "JoinButton")
                {
                    joinButton = child.GetComponent<Button>();
                }
            }
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

        playerCountText.text = $"{currentPlayerCount}/{maxPlayerCount}";
    }

    /// <summary>
    /// Gets the session information.
    /// </summary>
    /// <param name="info"></param>
    public void UpdateLobbyInformation(SessionInfo info)
    {
        this.sessionInfo = info;

        sessionInfo.Properties.TryGetValue("DisplayName", out var displayName);

        lobbyNameText.text = displayName;
        UpdatePlayerCount();

        sessionInfo.Properties.TryGetValue("JoinCode", out var code);
        joinCode = code;
        Debug.Log($"=> Join Code for Lobby {displayName.ToString()} is {joinCode}.");


        bool canPlayerJoin = true;
        if (currentPlayerCount >= maxPlayerCount)
        {
            canPlayerJoin = false;
        }

        joinButton.gameObject.SetActive(canPlayerJoin);
    }

    /// <summary>
    /// Handles players joining lobbies when button is clicked.
    /// </summary>
    public void OnJoinRoomClick()
    {
        joinButton.interactable = false;

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

        joinButton.interactable = true;
    }

    public void OnClick()
    {
        OnJoinSession?.Invoke(this.sessionInfo);
    }
}
