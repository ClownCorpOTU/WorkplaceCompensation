using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using Fusion;
using System;
using UnityEngine.SceneManagement;

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

    [Header("User Inputs")]
    [SerializeField] TMP_InputField lobbyNameInput;
    //[SerializeField] TMP_InputField lobbySizeInput;
    [SerializeField] TMP_Dropdown mapSelection;

    [Header ("Lobby")]
    SessionInfo testingInfo;
    int maxLobbySize = 4;

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
        
        newLobbyPopUp.SetActive(false);
    }

    void Start()
    {
        networkRunnerHandler.OnJoinLobby("MainLobbyList");
    }

    public void ClearLobbyDisplay()
    {
        foreach (Transform child in lobbyDisplay.transform)
        {
            Destroy(child.gameObject);
        }
    }

    public void OnCreateNewLobby()
    {
        if (newLobbyPopUp.activeSelf == false)
        {
            newLobbyPopUp.SetActive(true);
        }
    }

    public void CreateLobbySession()
    {
        //int.TryParse(lobbySizeInput.text, out int lobbySize);

        string lobbyName = lobbyNameInput.text;

        networkRunnerHandler.CreateGame(lobbyName, maxLobbySize, SelectMap());

        //CreateEntry(NetworkManager._runnerInstance.SessionInfo);

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
                    return "Assets/_Scenes/Fall/FallExpo_FinalReview.unity";
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

        UpdateEntry(session, newEntry);

        testingInfo = session;
    }

    /// <summary>
    /// Updates lobby entry.
    /// </summary>
    /// <param name="session"></param>
    /// <param name="entry">The new entry UI prefab that will be updated. If it is null, get the existing entry from the dictionary and update it.</param>
    void UpdateEntry(SessionInfo session, GameObject entry = null)
    {        
        LobbyEntry entryScript = entry.GetComponent<LobbyEntry>();

        entryScript.SessionInformation(session);

        entryScript.joinButton.interactable = session.IsOpen;

        entry.SetActive(session.IsValid);
    }

    public void JoinLobbyByName(string roomName)
    {
        if (roomName == testingInfo.Name)
        {
            networkRunnerHandler.JoinGame(testingInfo);
        }
        else
        {
            Debug.LogError($"{roomName} is not a session.");
        }
    }
}
