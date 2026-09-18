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
        if (tutorialMenu != null) tutorialMenu.SetActive(true);
        if (mainMenu != null) mainMenu.SetActive(false);
        
        if (TutorialManager.Instance != null) TutorialManager.Instance.SetLevel(levelName);
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
