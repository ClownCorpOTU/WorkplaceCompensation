using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using Fusion;
using System;
using UnityEngine.SceneManagement;

public class LobbyMenuManager : MonoBehaviour
{
    [SerializeField] List<LobbyEntry> lobbyEntries = new List<LobbyEntry>();
    [SerializeField] GameObject lobbyEntryPrefab;
    [SerializeField] VerticalLayoutGroup lobbyDisplay;


    [SerializeField] GameObject lobbyUI;
    [SerializeField] GameObject newLobbyPopUp;

    [SerializeField] TMP_InputField lobbyNameInput;
    //[SerializeField] TMP_InputField lobbySizeInput;
    [SerializeField] TMP_Dropdown mapSelection;

    int maxLobbySize = 4;

    void Awake()
    {
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

    public void ClearLobbyList()
    {
        foreach (Transform child in lobbyDisplay.transform)
        {
            Destroy(child.gameObject);
        }
    }

    public void AddToList(SessionInfo sessionInfo)
    {
        LobbyEntry newLobbyEntry = Instantiate(lobbyEntryPrefab, lobbyDisplay.transform).GetComponent<LobbyEntry>();
        newLobbyEntry.SessionInformation(sessionInfo);
    }

    public void NewLobby()
    {
        if (newLobbyPopUp.activeSelf == false)
        {
            newLobbyPopUp.SetActive(true);
        }
    }

    public void CreateLobby()
    {
        //int.TryParse(lobbySizeInput.text, out int lobbySize);

        string lobbyName = lobbyNameInput.text;

        SceneManager.LoadScene(SelectMap());

        newLobbyPopUp.SetActive(false); // Hide Popup.
    }

    public string SelectMap()
    {
        string sceneName = "FallExpo_FinalReview";

        switch (mapSelection.value)
        {
            case 0:
                {
                    sceneName = "FallExpo_FinalReview";
                    return sceneName;
                }
            case 1:
            {
                sceneName = "";
                return sceneName;
            }
            default:
                {
                    return sceneName;
                }
        }
    }
}
