using System;
using Photon.Voice.Unity;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.UI;

public class SettingsManager : MonoBehaviour
{
    [SerializeField] private Toggle micToggle;
    [SerializeField] private Slider sensitivitySlider;
    
    private bool isMicActive = false;
    private Recorder recorder;
    private CinemachineCamera vCam;

    private void Start()
    {
        // === MIC LOGIC === ///
        recorder = FindFirstObjectByType<Recorder>();

        int savedMicState = PlayerPrefs.GetInt("IsMicActive?", 1); 
        isMicActive = (savedMicState == 1);

        if (micToggle != null) 
        {
            micToggle.SetIsOnWithoutNotify(isMicActive);
            micToggle.onValueChanged.AddListener(OnToggleMic);
        }
    
        if (recorder != null) recorder.TransmitEnabled = isMicActive;
        
        // === SENSITIVTY LOGIC === ///
        float savedSensitivity = PlayerPrefs.GetFloat("MouseSensitivity", 1.0f);
        if (NetworkPlayer.Local.PlayerCamera != null)
            NetworkPlayer.Local.PlayerCamera.UpdateSensitivity();
        
        if (sensitivitySlider != null)
        {
            sensitivitySlider.value = savedSensitivity;
            sensitivitySlider.onValueChanged.AddListener(OnSensitivityChanged);
        }
    }

    public void OnToggleMic(bool micActive)
    {
        if (recorder != null) recorder.TransmitEnabled = micActive;

        var newValue = micActive ? 1 : 0;
        PlayerPrefs.SetInt("IsMicActive?", newValue);
        PlayerPrefs.Save();
    }

    public void OnSensitivityChanged(float value)
    {
        PlayerPrefs.SetFloat("MouseSensitivity", value);
        PlayerPrefs.Save();
        
        if (NetworkPlayer.Local.PlayerCamera != null)
            NetworkPlayer.Local.PlayerCamera.UpdateSensitivity();
    }
}
