using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] string gameplayScene = "FallExpo_FinalReview";
    [SerializeField] string lobbyScene = "Lobby";
    [SerializeField] GameObject notesMenu;

    
    private void Start()
    {
        notesMenu.SetActive(false);
    }

    private void DisableUI()
    {
        GameObject.Find("MenuUI").SetActive(false);
        notesMenu.SetActive(false);
    }

    public void JustPlay()
    {
        DisableUI();
        SceneManager.LoadScene(gameplayScene);
    }

    public void GoToLobbyMenu()
    {
        DisableUI();
        SceneManager.LoadScene(lobbyScene);
    }

    public void OpenNotesMenu()
    {
        notesMenu.SetActive(true);
    }
    
    public void CloseNotesMenu()
    {
        // These could be the same function but I'm just being quick
        notesMenu.SetActive(false);
    }

    public void QuiteGame()
    {
        Application.Quit();
    }
}
