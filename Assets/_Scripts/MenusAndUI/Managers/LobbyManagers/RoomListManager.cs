using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Fusion;
using Photon.Realtime;
using System.Xml.Serialization;
using Fusion.Sockets;
using WebSocketSharp;


/// <summary>
/// Handles the Lobby UI
/// </summary>
public class RoomListManager : MonoBehaviour, INetworkRunnerCallbacks
{
    [Header("Managers")]
    [SerializeField] LobbyRoomManager _lobbyRoomManager;
    [SerializeField] SteamManager _steamManager;
    [SerializeField] NetworkRunnerHandler _networkRunnerHandler;
    [SerializeField] NetworkRunner _networkRunner;

    [Header("Room List Content")]
    [SerializeField] GameObject roomDisplayPrefab;
    [SerializeField] List<SessionInfo> _listOfRoomInfo = new List<SessionInfo>();
    [SerializeField] Transform roomListingContainer;

    private SessionInfo _selectedRoomInfo;

    [Header("UI Elements")]
    [SerializeField] Button refreshButton;
    [SerializeField] Button joinButton;
    [SerializeField] TMP_InputField joinCodeInput;

    void Awake()
    {
        if (roomDisplayPrefab == null)
        {
            Debug.LogError("Missing prefab to display rooms");
            return;
        }

        if (roomListingContainer == null)
        {
            roomListingContainer = GameObject.Find("RoomListingContainer").transform;
        }

        if (_lobbyRoomManager == null)
        {
            _lobbyRoomManager = FindFirstObjectByType<LobbyRoomManager>();
        }

        if (_steamManager == null)
        {
            _steamManager = FindFirstObjectByType<SteamManager>();
        }

        if (_networkRunnerHandler == null)
        {
            _networkRunnerHandler = FindFirstObjectByType<NetworkRunnerHandler>();
        }

        if (_networkRunner == null)
        {
            _networkRunner = FindFirstObjectByType<NetworkRunner>();
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _listOfRoomInfo.Clear(); // Clear list

        if (joinCodeInput == null)
        {
            joinCodeInput = GameObject.Find("JoinCodeInputField").GetComponent<TMP_InputField>();
        }

        joinCodeInput.text = ""; // Clear input field

        if (refreshButton == null)
        {
            refreshButton = GameObject.Find("RefreshListButton").GetComponent<Button>();
        }

        if (joinButton == null)
        {
            joinButton = GameObject.Find("JoinRoomButton").GetComponent<Button>();
        }

        //joinButton.interactable = false;

        refreshButton.onClick.AddListener(RefreshLobbyListing);
        joinButton.onClick.AddListener(JoinSelectedRoom);

        joinCodeInput.onSubmit.AddListener(AsyncJoinRoomByCode);

        ConnectToLobbyList();
    }

    private async void ConnectToLobbyList()
    {
        if (_networkRunnerHandler != null)
        {
            _networkRunnerHandler.OnJoinLobbyList(_lobbyRoomManager.serverName);

            if (_networkRunner == null)
            {
                _networkRunner = FindFirstObjectByType<NetworkRunner>();
            }

            _networkRunner.AddCallbacks(this);

            var result = await _networkRunner.JoinSessionLobby(SessionLobby.ClientServer, "Server_Testing_Lobby_List");

            if (!result.Ok)
            {
                Debug.LogError($"Failed to join lobby listing: {result.ErrorMessage}");
            }
        }
    }

    /// <summary>
    /// INTERFACE IMPLEMENTATION.
    /// </summary>
    /// <param name="runner"></param>
    /// <param name="roomInfoList"></param>
    public void OnSessionListUpdated (NetworkRunner runner, List<SessionInfo> roomInfoList)
    {
        _listOfRoomInfo = roomInfoList;

        if (_networkRunnerHandler != null)
        {
            _networkRunnerHandler.sessionList = roomInfoList;
        }

        RefreshLobbyListing();
    }

    public void RefreshLobbyListing()
    {
        Debug.Log("Refreshing List...");

        _selectedRoomInfo = null;
        //joinButton.interactable = false;

        foreach (Transform roomListing in roomListingContainer)
        {
            Destroy(roomListing.gameObject);
        }

        foreach (var roomInfo in _listOfRoomInfo)
        {
            if (roomInfo.IsVisible && roomInfo.IsOpen)
            {
                GameObject newRoomListing = Instantiate(roomDisplayPrefab, roomListingContainer);
                RoomDisplayManager roomListingScript = newRoomListing.GetComponent<RoomDisplayManager>();

                if (roomListingScript != null)
                {
                    roomListingScript.UpdateRoomInfo(roomInfo, this);
                }
            }
        }
    }

    public void SetSelectedRoom(SessionInfo roomInfo)
    {
        _selectedRoomInfo = roomInfo;
        //joinButton.interactable = true;
    }

    private void JoinSelectedRoom()
    {
        // Does nothing if there is no selected room and the input text is empty or if the NetworkRunnerHandler is missing
        if (_selectedRoomInfo == null && joinCodeInput.text.IsNullOrEmpty() || _networkRunnerHandler != null)
        {
            return;
        }

        // Prioritize join codes even if the player selected a room.
        if (!joinCodeInput.text.IsNullOrEmpty())
        {
            JoinRoomByCode();
        }
        else if (_selectedRoomInfo != null)
        {
            _networkRunnerHandler.JoinGame(_selectedRoomInfo);
        }
    }

    private void JoinRoomByCode()
    {
        string inputtedCode = joinCodeInput.text;

        joinCodeInput.text = string.Empty; // Clear the input field

        if (!string.IsNullOrEmpty(inputtedCode))
        {
            AsyncJoinRoomByCode(inputtedCode);
        }
    }

    private async void AsyncJoinRoomByCode(string code)
    {
        if (_networkRunnerHandler != null)
        {
            _networkRunnerHandler.JoinGameByCode(code);
        }
    }

    #region Unused INetworkRunnerCallbacks
    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player) { }
    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) { }
    public void OnInput(NetworkRunner runner, NetworkInput input) { }
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }
    public void OnConnectedToServer(NetworkRunner runner) { }
    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, System.ArraySegment<byte> data) { }
    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }
    public void OnSceneLoadDone(NetworkRunner runner) { }
    public void OnSceneLoadStart(NetworkRunner runner) { }
    #endregion

}
