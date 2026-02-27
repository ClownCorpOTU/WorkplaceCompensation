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


    private void Start()
    {
        fakeLoadingScreen.SetActive(false);
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
        var normalizeStamina = NetworkPlayer.Local.NormalizeStamina();
        staminaBarImage.fillAmount = normalizeStamina;
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