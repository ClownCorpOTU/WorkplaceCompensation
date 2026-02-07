using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Fusion;
using System;

public class LobbyEntry : MonoBehaviour
{
    public TextMeshProUGUI lobbyName, playerCount;
    public TMP_Dropdown mapSelection;
    public Button joinButton;
    public int currentPlayerCount = 0, maxPlayerCount = 0;

    SessionInfo sessionInfo;
    public event Action<SessionInfo> OnJoinSession;

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
            Debug.Log("Lobby is full.");
            return;
        }

        NetworkManager._runnerInstance.StartGame(new Fusion.StartGameArgs()
        {
            SessionName = lobbyName.text,
        });
    }
}
