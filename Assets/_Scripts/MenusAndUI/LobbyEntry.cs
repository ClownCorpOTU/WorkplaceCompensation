using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class LobbyEntry : MonoBehaviour
{
    public TextMeshProUGUI lobbyName, playerCount;
    public Button joinButton;
    [Range(0, 10)] public int currentPlayerCount = 0, maxPlayerCount = 0;

    /// <summary>
    /// Update the player count text to match the current player count.
    /// </summary>
    public void UpdatePlayerCount()
    {
        playerCount.text = $"{currentPlayerCount}/{maxPlayerCount}";
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
