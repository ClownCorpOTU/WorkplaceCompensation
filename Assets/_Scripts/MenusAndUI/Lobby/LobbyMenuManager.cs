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
    public Dictionary<string, GameObject> lobbyEntriesDictionary = new Dictionary<string, GameObject>();
    public SessionInfo sessionInfo;

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
        sessionInfo = NetworkManager._runnerInstance.SessionInfo;
    }

    public void ClearLobbyDisplay()
    {
        foreach (Transform child in lobbyDisplay.transform)
        {
            Destroy(child.gameObject);
        }
    }

    public void AddToList(SessionInfo info)
    {
        LobbyEntry newLobbyEntry = Instantiate(lobbyEntryPrefab, lobbyDisplay.transform).GetComponent<LobbyEntry>();
        newLobbyEntry.SessionInformation(info);

        newLobbyEntry.maxPlayerCount = maxLobbySize;
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

        AddToList(NetworkManager._runnerInstance.SessionInfo);

        networkRunnerHandler.CreateGame(lobbyName, SelectMap());

        CreateEntry(lobbyName, sessionInfo);

        newLobbyPopUp.SetActive(false); // Hide Popup.
    }


    /// <summary>
    /// Gets the scene for the level chosen in the lobby creation menu.
    /// </summary>
    /// <returns>The scene of the level.</returns>
    public string SelectMap()
    {
        string sceneName = "";

        switch (mapSelection.value)
        {
            case 0:
                {
                    sceneName = "_Scene/Fall/FallExpo_FinalReview";
                    //sceneName = "FallExpo_FinalReview";
                    return sceneName;
                }
            case 1:
            {
                sceneName = "_Scene/Winter/Test_MarsCanyon";
                //sceneName = "Test_MarsCanyon";
                return sceneName;
            }
            default:
                {
                    return "FallExpo_FinalReview";
                }
        }
    }

    /// <summary>
    /// Checks if the lobby is in session. If not, the lobby will be removed from the UI and Dictionary.
    /// </summary>
    /// <param name="sessionList"></param>
    void DeleteEntry(List<SessionInfo> sessionList)
    {
        bool isInSession = false;
        GameObject entryToDelete = null;

        // Check LobbyEntries for lobbies that are no longer in session list
        foreach (KeyValuePair<string, GameObject> kvp in lobbyEntriesDictionary)
        {
            string lobbyName = kvp.Key;
            foreach (SessionInfo sessionInfo in sessionList)
            {
                if (sessionInfo.Name == lobbyName)
                {
                    isInSession = true;
                    break;
                }
            }

            if (!isInSession)
            {
                entryToDelete = kvp.Value;
                lobbyEntriesDictionary.Remove(lobbyName); // Remove Lobby from entry
                Destroy(entryToDelete); // Delete lobby
            }
        }
    }

    void CompareEntries(List<SessionInfo> sessionList)
    {
        foreach (SessionInfo session in sessionList)
        {
            if (lobbyEntriesDictionary.ContainsKey(session.Name))
            {
                UpdateEntry(session);
            }
            else
            {
                CreateEntry(session.Name, session);
            }
        }
    }

    /// <summary>
    /// Create new lobby entry and display it on the lobby UI.
    /// </summary>
    /// <param name="session"></param>
    public void CreateEntry(string name, SessionInfo session)
    {
        GameObject newEntry = GameObject.Instantiate(lobbyEntryPrefab, lobbyDisplay.transform);
        lobbyEntriesDictionary.Add(name, newEntry);

        UpdateEntry(session, newEntry);
    }

    /// <summary>
    /// Updates lobby entry.
    /// </summary>
    /// <param name="session"></param>
    /// <param name="entry">The new entry UI prefab that will be updated. If it is null, get the existing entry from the dictionary and update it.</param>
    void UpdateEntry(SessionInfo session, GameObject entry = null)
    {
        if (entry == null)
        {
            lobbyEntriesDictionary.TryGetValue(session.Name, out entry);
        }
        
        LobbyEntry entryScript = entry.GetComponent<LobbyEntry>();

        entryScript.UpdatedEntryInfo(session.Name, session.PlayerCount, session.MaxPlayers);

        entryScript.joinButton.interactable = session.IsOpen;

        entry.SetActive(session.IsValid);
    }
}
