using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Fusion;
using System;

public class LobbyEntry : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI lobbyName, playerCount;
    public Button joinButton;
    public int currentPlayerCount = 0, maxPlayerCount = 0;

    SessionInfo sessionInfo;
    public event Action<SessionInfo> OnJoinSession;

    /// <summary>
    /// Update the entry that is displayed.
    /// </summary>
    /// <param name="name">String of the lobby name.</param>
    /// <param name="playerInLobby">Number of players in the lobby, currently.</param>
    /// <param name="playerCap">Maximum players allowed in the lobby.</param>
    /// <param name="map">String of the scene's name that the player will see.</param>
    public void UpdatedEntryInfo(string name, int playerInLobby, int playerCap, string map = null)
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
        sessionInfo = info;
    }

    public void JoinRoom()
    {
        if (currentPlayerCount >= maxPlayerCount)
        {
            Debug.LogError("Lobby is full.");
            return;
        }

        NetworkManager._runnerInstance.StartGame(new Fusion.StartGameArgs()
        {
            SessionName = lobbyName.text,
        });

        UpdatePlayerCount();
    }
}
