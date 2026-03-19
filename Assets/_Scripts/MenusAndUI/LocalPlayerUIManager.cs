using System;
using Photon.Voice.Unity;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LocalPlayerUIManager : MonoBehaviour
{
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private Image staminaBarImage;
    [SerializeField] private GameObject fakeLoadingScreen;
    [SerializeField] private Material radiationScreenMat;

    private Recorder recorder;
    public bool IsLocalGamePaused { get; private set; }

    private Image staminaBarImage2;

    private void Start()
    {
        fakeLoadingScreen.SetActive(false);
        staminaBarImage2 = NetworkPlayer.Local.StaminaFillImage;
    }

    public void TogglePause()
    {
        IsLocalGamePaused = !IsLocalGamePaused;
        pausePanel.SetActive(IsLocalGamePaused);
        Cursor.lockState = IsLocalGamePaused ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = IsLocalGamePaused;
    }

    public void ResumeGame()
    {
        IsLocalGamePaused = false;
        pausePanel.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void BackToMenu()
    {
        SceneManager.LoadScene(0);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    private void Update()
    {
        if (NetworkPlayer.Local == null) return;
        
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