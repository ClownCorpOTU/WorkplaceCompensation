using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] string gameplayScene = "FallExpo_FinalReview";
    [SerializeField] string lobbyScene = "Lobby";

    [SerializeField] private GameObject mainMenu;
    [SerializeField] GameObject notesMenu;
    [SerializeField] GameObject fakeLoadingScreen;
    [SerializeField] private GameObject tutorialMenu; 
    
    private void Start()
    {
        notesMenu.SetActive(false);
        fakeLoadingScreen.SetActive(false);
    }

    private void DisableUI()
    {
        notesMenu.SetActive(false);
        fakeLoadingScreen.SetActive(true);
    }

    public void JustPlay()
    {
        DisableUI();
        SceneManager.LoadScene(gameplayScene);
    }

    public void PlayLevel(string levelName)
    {
        SceneManager.LoadScene(levelName);
    }

    public void ShowTutorialMenu(string levelName)
    {
        tutorialMenu.SetActive(true);
        mainMenu.SetActive(false);
        TutorialManager.Instance.SetLevel(levelName);
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
