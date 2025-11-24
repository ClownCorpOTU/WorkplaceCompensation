using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] SceneAsset gameplayScene;
    [SerializeField] SceneAsset lobbyScene;
    
    private void DisableUI()
    {
        GameObject.Find("MenuUI").SetActive(false);
    }

    public void JustPlay()
    {
        DisableUI();
        SceneManager.LoadScene(gameplayScene.name);
    }

    public void GoToLobbyMenu()
    {
        DisableUI();
        SceneManager.LoadScene(lobbyScene.name);
    }
}
