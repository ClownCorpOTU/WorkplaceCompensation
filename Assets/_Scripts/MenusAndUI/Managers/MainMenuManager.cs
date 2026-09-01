using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] string gameplayScene = "FallExpo_FinalReview";
    [SerializeField] string lobbyScene = "Lobby";

    [SerializeField] private GameObject mainMenu;
    [SerializeField] private GameObject tutorialMenu; 
    

    public void JustPlay()
    {
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
        SceneManager.LoadScene(lobbyScene);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
