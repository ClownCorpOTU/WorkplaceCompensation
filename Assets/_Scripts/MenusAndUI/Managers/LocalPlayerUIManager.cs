using System;
using System.Collections;
using Photon.Voice.Unity;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LocalPlayerUIManager : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private GameObject settingsMenu;
    [SerializeField] private GameObject hostLeftPanel;

    [Header("Gameplay UI")]
    [SerializeField] private Image staminaBarImage;
    [SerializeField] private GameObject fakeLoadingScreen;
    [SerializeField] private Material radiationScreenMat;

    public bool IsLocalGamePaused { get; private set; }
    private Image staminaBarImage2;
    private Recorder recorder;
    
    private void OnDisable()
    {
        var input = FindFirstObjectByType<InputReader>();
        if (input != null) input.OnPausePressed -= TogglePause;
    }

    private void Start()
    {
        fakeLoadingScreen.SetActive(false);
        staminaBarImage2 = NetworkPlayer.Local.StaminaFillImage;
        CloseAllMenus();
    }
    
    public void SetInputSource(InputReader reader)
    {
        reader.OnPausePressed -= TogglePause; // Prevent double-subscription
        reader.OnPausePressed += TogglePause;
    }

    public void TogglePause()
    {
        if (IsLocalGamePaused)
            ResumeGame();
        else
            OpenPauseMenu();
    }
    
    public void OpenPauseMenu()
    {
        IsLocalGamePaused = true;
        pausePanel.SetActive(true);
        
        // FIX: Always reset to main menu view
        pauseMenu.SetActive(true);
        settingsMenu.SetActive(false);

        UpdateCursorState();
    }

    public void ResumeGame()
    {
        IsLocalGamePaused = false;
        pausePanel.SetActive(false);
        
        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }
        
        UpdateCursorState();
    }
    
    private void UpdateCursorState()
    {
        if (IsLocalGamePaused)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
    
    private void OnApplicationFocus(bool hasFocus)
    {
        // If the player clicks back into the game window, re-apply the correct lock state
        if (hasFocus) UpdateCursorState();
    }
    
    private void CloseAllMenus()
    {
        IsLocalGamePaused = false;
        pausePanel.SetActive(false);
        UpdateCursorState();
    }

    public void BackToMenu()
    {
        // Shut down runner before leaving
        var runner = FindFirstObjectByType<Fusion.NetworkRunner>();
        if (runner != null) runner.Shutdown();
        
        SceneManager.LoadScene(0);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void ShowHostLeftScreen()
    {
        hostLeftPanel.SetActive(true);
        pausePanel.SetActive(false);

        IsLocalGamePaused = true;
        UpdateCursorState();
    }

    private void Update()
    {
        if (NetworkPlayer.Local != null) 
            UpdateStaminaVisuals();
    }

    private void UpdateStaminaVisuals()
    {
        // Stamina (old)
        var normalizeStamina = NetworkPlayer.Local.NormalizeStamina();
        staminaBarImage.fillAmount = normalizeStamina;
        
        if (staminaBarImage2 == null)
        {
            staminaBarImage2 = NetworkPlayer.Local.StaminaFillImage;
        }
        else
        {
            staminaBarImage2.fillAmount = normalizeStamina;

            float targetAlpha = NetworkPlayer.Local.IsUsingStamina ? 1f : 0.3f;
            Color targetColor = new Color(1f, 1f, 1f, targetAlpha);
    
            // Time.deltaTime * 5f controls the speed. Higher = Faster fade.
            staminaBarImage2.color = Color.Lerp(staminaBarImage2.color, targetColor, Time.deltaTime * 5f);
        }
    }

    public void MuteLocalPlayer(bool mute)
    {
        if (recorder == null)
        {
            recorder = FindFirstObjectByType<Recorder>();
        }
        
        recorder.RecordingEnabled = !mute;
    }

    public void UpdateRadiationFullScreenEffect(float intensity)
    {
        radiationScreenMat.SetFloat("_RadiationIntensity", intensity);
    }
}