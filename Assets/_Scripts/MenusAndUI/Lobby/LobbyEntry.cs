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
    public int currentPlayerCount = 0, maxPlayerCount = 0;

    [Header("Session")]
    SessionInfo sessionInfo;
    public event Action<SessionInfo> OnJoinSession;

    public LobbyEntry(LobbyMenuManager lobbyMenuManager)
    {
        if (lobbyMenuManager != null)
        {
            lobbyManager = lobbyMenuManager;
        }    
    }

    /// <summary>
    /// Update the entry that is displayed.
    /// </summary>
    /// <param name="name">String of the lobby name.</param>
    /// <param name="playerInLobby">Number of players in the lobby, currently.</param>
    /// <param name="playerCap">Maximum players allowed in the lobby.</param>
    /// <param name="map">String of the scene's name that the player will see.</param>
    public void UpdateEntryInfo(string name, int playerInLobby, int playerCap, string map = null)
    {
        lobbyName.text = name;
        currentPlayerCount = playerInLobby;
        maxPlayerCount = playerCap;

        UpdatePlayerCount();
    }
    /// <summary>
    /// Update the player count text to match the current player count.
    /// </summary>
    public void UpdatePlayerCount()
    {
        playerCount.text = $"{currentPlayerCount}/{maxPlayerCount}";
    }

    public void SessionInformation(SessionInfo info)
    {
        this.sessionInfo = info;

        UpdateEntryInfo(info.Name, info.PlayerCount, info.MaxPlayers);

        bool canPlayerJoin = true;
        if (currentPlayerCount >= maxPlayerCount)
        {
            canPlayerJoin = false;
        }
        joinButton.gameObject.SetActive(canPlayerJoin);
    }

    public void OnJoinRoomClick()
    {
        if (currentPlayerCount >= maxPlayerCount)
        {
            Debug.LogError("Lobby is full.");
            return;
        }

        currentPlayerCount++;

        UpdatePlayerCount();

        NetworkRunnerHandler networkRunnerHandler = FindFirstObjectByType<NetworkRunnerHandler>();

        networkRunnerHandler.OnJoinLobby(lobbyName.text);
    }

    public void OnClick()
    {
        OnJoinSession?.Invoke(this.sessionInfo);
    }

    public void ExitRoom()
    {
        currentPlayerCount--;
        UpdatePlayerCount();
    }
}
