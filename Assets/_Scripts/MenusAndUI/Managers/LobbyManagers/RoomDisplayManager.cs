using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Fusion;
using Photon.Realtime;
using UnityEngine.SceneManagement;

/// <summary>
/// Handles the filling of information that will be displayed to players in the Lobby UI menu.
/// </summary>
public class RoomDisplayManager : MonoBehaviour
{
    [Header("Displayed Info")]
    [SerializeField] TMP_Text roomNameTMPText;
    [SerializeField] TMP_Text playerCountTMPText;
    [SerializeField] TMP_Text mapNameTMPText;

    private SessionInfo _roomInfo;
    private RoomListManager _roomListManager;

    void Awake()
    {
        if (roomNameTMPText == null)
        {
            roomNameTMPText = GameObject.Find("RoomNameText").GetComponent<TMP_Text>();
        }

        if (playerCountTMPText == null)
        {
            playerCountTMPText = GameObject.Find("PlayerCountText").GetComponent<TMP_Text>();
        }

        if (mapNameTMPText == null)
        {
            mapNameTMPText = GameObject.Find("LevelNameText").GetComponent<TMP_Text>();
        }
    }

    void Start()
    {
        roomNameTMPText.text = "";
        playerCountTMPText.text = "0/0";
        mapNameTMPText.text = "";
    }

    public void UpdateRoomInfo(SessionInfo roomInfo, RoomListManager lobbyUIManager)
    {
        _roomInfo = roomInfo;
        _roomListManager = lobbyUIManager;

        if (_roomInfo.Properties.TryGetValue("DisplayName", out var displayName))
        {
            roomNameTMPText.text = displayName;
        }
        else
        {
            roomNameTMPText.text = _roomInfo.Name;
        }

        playerCountTMPText.text = $"{roomInfo.PlayerCount}/{roomInfo.MaxPlayers}";

         if (_roomInfo.Properties.TryGetValue("MapName", out var mapName))
        {
            mapNameTMPText.text = mapName;
        }
        else
        {
            mapNameTMPText.text = "No Named Map";
        }
        
    }

    public void OnRoomDisplaySelected()
    {
        if (_roomListManager != null && _roomInfo != null)
        {
            _roomListManager.SetSelectedRoom(_roomInfo);
        }
    }
}
