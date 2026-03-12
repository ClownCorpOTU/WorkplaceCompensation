using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using Random = UnityEngine.Random;

public class AudioManager : MonoBehaviour
{
    public Sound[] sounds;
    public static AudioManager instance;
    
    private HashSet<string> active3DSounds = new HashSet<string>(); // Ensures we don't play the same sound at the same time in 3D

    
    private void Awake() {
        if (instance == null) {
            instance = this;
        }
        else {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);

        foreach (Sound s in sounds)
        {
            if (!s.is3D) Populate2DSounds(s);
        }
    }

    private void Populate2DSounds(Sound s)
    {
        s.source = gameObject.AddComponent<AudioSource>();
        s.source.clip = s.clip;
        s.source.outputAudioMixerGroup = s.audioMixerGroup;
        s.source.volume = s.volume;
        s.source.pitch = s.pitch;
        s.source.loop = s.loop;
        s.source.spatialBlend = 0f; // Force 2D

        if (s.hasVariations && s.variations != null)
        {
            foreach (var v in s.variations)
            {
                Populate2DSounds(v);
            }
        }
    }

    public void Play(string name, Vector3? position = null) {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if (s == null) {
            Debug.LogWarning("Sound: " + name + " not found!");
            return;
        }
        
        // Determine which sound object to actually play (the base or a variation)
        Sound soundToPlay = s;
        if (s.hasVariations && s.variations.Length > 0)
        {
            int rand = Random.Range(0, s.variations.Length);
            soundToPlay = s.variations[rand];
        }

        // ================================
        // 2D SOUND — avoid double play
        // ================================
        if (!s.is3D)
        {
            // Check if this specific variation is already playing
            if (soundToPlay.source.isPlaying) return;

            soundToPlay.source.Play();
            return;
        }

        // ================================
        // 3D SOUND — avoid double spawning
        // ================================
        if (active3DSounds.Contains(s.name)) return;

        active3DSounds.Add(s.name);

        Vector3 soundPosition = position ?? Vector3.zero;
        GameObject tempGO = new GameObject("3D_Sound_" + soundToPlay.name);
        tempGO.transform.position = soundPosition;

        AudioSource tempSource = tempGO.AddComponent<AudioSource>();
        
        // Copy settings from the chosen soundToPlay (variation or base)
        tempSource.clip = soundToPlay.clip;
        tempSource.outputAudioMixerGroup = soundToPlay.audioMixerGroup;
        tempSource.volume = soundToPlay.volume;
        tempSource.pitch = soundToPlay.pitch;
        tempSource.spatialBlend = 1f; // Force 3D
        tempSource.loop = soundToPlay.loop;

        tempSource.Play();

        // Cleanup
        float clipLength = soundToPlay.clip.length;
        Destroy(tempGO, clipLength);
        StartCoroutine(RemoveAfterDelay(s.name, clipLength));
    }

    private System.Collections.IEnumerator RemoveAfterDelay(string name, float delay)
    {
        yield return new WaitForSeconds(delay);
        active3DSounds.Remove(name);
    }
}