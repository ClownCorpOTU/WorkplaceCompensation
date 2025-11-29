using System;
using UnityEngine;
using UnityEngine.UI;

public class LocalPlayerUIManager : MonoBehaviour
{
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private Image staminaBarImage;
    [SerializeField] private GameObject fakeLoadingScreen;
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

    private void Update()
    {
        var normalizeStamina = NetworkPlayer.Local.NormalizeStamina();
        staminaBarImage.fillAmount = normalizeStamina;
    }
}