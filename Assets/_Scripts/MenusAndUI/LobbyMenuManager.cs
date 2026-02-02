using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using Fusion;
using System;

public class LobbyMenuManager : MonoBehaviour
{
    [SerializeField] List<LobbyEntry> lobbyEntries = new List<LobbyEntry>();
    [SerializeField] GameObject lobbyEntryPrefab;

    [SerializeField] GameObject lobbyUI;
    [SerializeField] GameObject newLobbyPopUp;

    [SerializeField] TMP_InputField lobbyNameInput;
    //[SerializeField] TMP_InputField lobbySizeInput;

    int maxLobbySize = 4;

    void Awake()
    {
        if (lobbyUI == null)
        {
            lobbyUI = GameObject.Find("LobbyUI");
        }

        if (newLobbyPopUp == null)
        {
            newLobbyPopUp = GameObject.Find("NewLobbyPopUp");
        }
        if (lobbyNameInput == false)
        {
            lobbyNameInput = GameObject.Find("LobbyNameInput").GetComponent<TMP_InputField>();
        }
        /*if (lobbySizeInput == false)
        {
            lobbySizeInput = GameObject.Find("LobbySizeInput").GetComponent<TMP_InputField>();
        }*/
        
        newLobbyPopUp.SetActive(false);
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



        newLobbyPopUp.SetActive(false); // Hide Popup.
    }
}
