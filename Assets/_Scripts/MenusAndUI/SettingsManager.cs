using System;
using Photon.Voice.Unity;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class SettingsManager : MonoBehaviour
{
    [Header("Audio Setup")] 
    [SerializeField] private AudioMixer mainMixer;
    [SerializeField] private Slider masterSlider;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private Slider voiceSlider;
    [SerializeField] private Slider musicSlider;
    
    [Header("Other Settings")]
    [SerializeField] private Toggle micToggle;
    [SerializeField] private Slider sensitivitySlider;
    
    private bool isMicActive = false;
    private Recorder recorder;
    private CinemachineCamera vCam;

    private void Start()
    {
        LoadVolumeSettings();
        
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
        
        // === SENSITIVITY LOGIC === ///
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

    private void LoadVolumeSettings()
    {
        // Load and Apply Master
        float masterVal = PlayerPrefs.GetFloat("MasterVol", 0.75f); // Default to 100%
        masterSlider.value = masterVal;
        SetVolume("MasterVol", masterVal);
        masterSlider.onValueChanged.AddListener((val) => { 
            SetVolume("MasterVol", val); 
            PlayerPrefs.SetFloat("MasterVol", val);
        });
        
        // Repeat for SFX
        float sfxVal = PlayerPrefs.GetFloat("SFXVol", 0.8f);
        sfxSlider.value = sfxVal;
        SetVolume("SFXVol", sfxVal);
        sfxSlider.onValueChanged.AddListener((val) => { 
            SetVolume("SFXVol", val); 
            PlayerPrefs.SetFloat("SFXVol", val);
        });
        
        // Repeat for voice
        float voiceVal = PlayerPrefs.GetFloat("VoiceVol", 1.0f);
        voiceSlider.value = voiceVal;
        SetVolume("VoiceVol", voiceVal);
        voiceSlider.onValueChanged.AddListener((val) => { 
            SetVolume("VoiceVol", val); 
            PlayerPrefs.SetFloat("VoiceVol", val);
        });
        
        // Repeat for music
        float musicVal = PlayerPrefs.GetFloat("MusicVol", 0.7f);
        musicSlider.value = musicVal;
        SetVolume("MusicVol", musicVal);
        musicSlider.onValueChanged.AddListener((val) => { 
            SetVolume("MusicVol", val); 
            PlayerPrefs.SetFloat("MusicVol", val);
        });
    }
    
    public void SetVolume(string parameterName, float sliderValue)
    {
        // We clamp to 0.0001 because Log10 of 0 is undefined (and causes errors)
        float volumeInDb = Mathf.Log10(Mathf.Max(0.0001f, sliderValue)) * 20;
        mainMixer.SetFloat(parameterName, volumeInDb);
        print($"Setting {parameterName} to {volumeInDb}");
    }
}
