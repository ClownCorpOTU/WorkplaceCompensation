using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using Fusion;
using System;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;

public class LobbyMenuManager : MonoBehaviour
{
    [Header("Managers")]
    [SerializeField] NetworkRunnerHandler networkRunnerHandler;
    [SerializeField] NetworkManager networkManager;

    [Header("Lobby UI")]
    //[SerializeField] List<LobbyEntry> lobbyEntries = new List<LobbyEntry>();
    [SerializeField] GameObject lobbyEntryPrefab;
    [SerializeField] VerticalLayoutGroup lobbyDisplay;

    [Header("Game Objects")]
    [SerializeField] GameObject lobbyUI;
    [SerializeField] GameObject newLobbyPopUp;
    [SerializeField] TMP_Text errorMessenger;

    [Header("User Inputs")]
    [SerializeField] TMP_InputField lobbyNameInput;
    //[SerializeField] TMP_InputField lobbySizeInput;
    [SerializeField] TMP_Dropdown mapSelection;

    [Header ("Lobby")]
    int maxLobbySize = 16;
    Dictionary<SessionInfo, GameObject> lobbyEntries = new Dictionary<SessionInfo, GameObject>();

    void Awake()
    {
        if (networkRunnerHandler == null)
        {
            networkRunnerHandler = FindFirstObjectByType<NetworkRunnerHandler>();
        }

        if (networkManager == null)
        {
            networkManager = FindFirstObjectByType<NetworkManager>();
        }

        if (lobbyUI == null)
        {
            lobbyUI = GameObject.Find("LobbyUI");
        }

        if (lobbyDisplay == null)
        {
            lobbyDisplay = GameObject.Find("LobbyListContent").GetComponent<VerticalLayoutGroup>();
        }

        if (newLobbyPopUp == null)
        {
            newLobbyPopUp = GameObject.Find("NewLobbyPopUp");
        }
        
        if (errorMessenger == null)
        {
            errorMessenger = GameObject.Find("ErrorMessage").GetComponent<TMP_Text>();
        }

        if (lobbyNameInput == null)
        {
            lobbyNameInput = GameObject.Find("LobbyNameInput").GetComponent<TMP_InputField>();
        }

        if (mapSelection == null)
        {
            mapSelection = GameObject.Find("MapOptions").GetComponent<TMP_Dropdown>();
        }
        
        /*if (lobbySizeInput == null)
        {
            lobbySizeInput = GameObject.Find("LobbySizeInput").GetComponent<TMP_InputField>();
        }*/
        
        errorMessenger.text = ""; // Clear error message.

        errorMessenger.gameObject.SetActive(false);
        newLobbyPopUp.SetActive(false);
    }

    void Start()
    {
        lobbyEntries.Clear();
        networkRunnerHandler.OnJoinLobby("MainLobbyList");
    }

    /// <summary>
    /// Clears the Parent transform holding all lobby entries.
    /// </summary>
    public void ClearLobbyDisplay()
    {
        foreach (Transform child in lobbyDisplay.transform)
        {
            Destroy(child.gameObject);
        }
    }

    /// <summary>
    /// OnClick function. Enables the popup menu to create a lobby.
    /// </summary>
    public void OnCreateNewLobby()
    {
        if (newLobbyPopUp.activeSelf == false)
        {
            newLobbyPopUp.SetActive(true);
        }
    }

    /// <summary>
    /// Handles getting the data from the popup menu passes it to NetworkRunnerHandler to create the room.
    /// </summary>
    public void CreateLobbySession()
    {
        if (errorMessenger.gameObject.activeSelf)
        {
            errorMessenger.gameObject.SetActive(false);
        }

        //int.TryParse(lobbySizeInput.text, out int lobbySize);

        string lobbyName = lobbyNameInput.text;
        
        networkRunnerHandler.CreateGame(lobbyName, maxLobbySize, SelectMap());

        newLobbyPopUp.SetActive(false); // Hide Popup.
    }

    /// <summary>
    /// Gets the scene for the level chosen in the lobby creation menu.
    /// </summary>
    /// <returns>The scene of the level.</returns>
    public string SelectMap()
    {
        switch (mapSelection.value)
        {
            case 0:
                {
                    return "Assets/_Scenes/Fall/FallExpo_FinalReview.unity";
                }
            case 1:
            {
                return "Assets/_Scenes/Winter/Test_MarsCanyon.unity";
            }
            default:
                {
                    return "Assets/_Scenes/MainMenu.unity";
                }
        }
    }

    /// <summary>
    /// Create new lobby entry and display it on the lobby UI.
    /// </summary>
    /// <param name="session"></param>
    public void CreateEntry(SessionInfo session)
    {
        GameObject newEntry = GameObject.Instantiate(lobbyEntryPrefab, lobbyDisplay.transform);

        //lobbyEntries.Add(session, newEntry);

        UpdateEntry(session, newEntry);
    }

    /// <summary>
    /// Updates lobby entry in the UI using the SessionInfo.
    /// </summary>
    /// <param name="session">Information about the current session/lobby.</param>
    /// <param name="entry">The new entry UI prefab that will be updated. If it is null, get the existing entry from the dictionary and update it.</param>
    void UpdateEntry(SessionInfo session, GameObject entry = null)
    {        
        LobbyEntry entryScript = entry.GetComponent<LobbyEntry>();

        entryScript.UpdateLobbyInformation(session);

        entryScript.joinButton.interactable = session.IsOpen;

        entry.SetActive(session.IsValid);
    }


    /// <summary>
    /// Removes entry from UI.
    /// </summary>
    /// <param name="delete"></param>
    public void DeleteEntry(List<SessionInfo> sessionList)
    {
        
    }


    /// <summary>
    /// Checks if the session already exists in the UI.
    /// </summary>
    /// <param name="compareTo"></param>
    public void CompareEntries(List<SessionInfo> sessionList)
    {
        
    }

    /// <summary>
    /// Displays error message on popup.
    /// </summary>
    /// <param name="errMsg">The message to be displayed</param>
    public void DisplaySessionCreateError(string errMsg)
    {
        errorMessenger.gameObject.SetActive(true);

        errorMessenger.text = errMsg;
    }

    /// <summary>
    /// DELETE. Testing Join function. Overrides the entry UI's join button.
    /// </summary>
    /// <param name="roomCode"></param>
    public void JoinLobbyByCode(string roomCode)
    {
        roomCode = roomCode.ToUpper(); // Ensures room code is in all uppercase.

        LobbyEntry currentLobbyEntry;

        foreach (Transform lobbyEntryUI in lobbyDisplay.transform)
        {
             currentLobbyEntry = lobbyEntryUI.GetComponent<LobbyEntry>();

             if (roomCode == currentLobbyEntry.joinCode)
            {
                currentLobbyEntry.OnJoinRoomClick();
                return;
            }
        }
    }
}
