using System.Collections.Generic;
using Fusion;
using Photon.Realtime;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class RoomListManager : MonoBehaviour
{
    [Header("Managers")]
    [SerializeField] LobbyRoomManager lobbyRoomManager;
    [SerializeField] SteamManager steamManager;
    [SerializeField] NetworkRunnerHandler networkRunnerHandler;

    [Header("Room List Content")]
    [SerializeField] GameObject roomDisplayPrefab;
    [SerializeField] List<GameObject> listOfRoomUIObjects = new List<GameObject>();

    [Header("UI Elements")]
    [SerializeField] Button refreshButton;
    [SerializeField] Button joinButton;
    [SerializeField] TMP_InputField joinCodeInput;

    void Awake()
    {
        if (roomDisplayPrefab == null)
        {
            Debug.Log("Missing prefab to display rooms");
            return;
        }

        if (lobbyRoomManager == null)
        {
            lobbyRoomManager = FindFirstObjectByType<LobbyRoomManager>();
        }

        if (steamManager == null)
        {
            steamManager = FindFirstObjectByType<SteamManager>();
        }

        if (networkRunnerHandler == null)
        {
            networkRunnerHandler = FindFirstObjectByType<NetworkRunnerHandler>();
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        listOfRoomUIObjects.Clear(); // Clear list

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

        if (joinButton.enabled == true)
        {
            joinButton.enabled = false;
        }
    }

    public void OnAddRoomToDisplay(SessionInfo newRoomInfo)
    {
        
    }

    public void OnUpdateRoomList()
    {
        
    }

    public void OnJoinSelectedRoomClicked(GameObject selectedRoom, string code = "")
    {
        
    }

    public void OnJoinCodeEntered(string code)
    {
        
    }

    public void OnRefreshListClicked()
    {
        
    }

}
