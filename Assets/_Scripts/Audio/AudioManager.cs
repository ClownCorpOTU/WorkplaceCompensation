using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

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

        foreach (Sound s in sounds) {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;
            s.source.outputAudioMixerGroup = s.audioMixerGroup;

            s.source.volume = s.volume;
            s.source.pitch = s.pitch;
            s.source.loop = s.loop;
            s.source.spatialBlend = s.is3D ? 1f : 0f;
        }
    }

    public void Play(string name, Vector3? position = null) {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if (s == null) {
            Debug.LogWarning("Sound: " + name + " not found!");
            return;
        }

        // ================================
        // 2D SOUND — avoid double play
        // ================================
        if (!s.is3D)
        {
            if (s.source.isPlaying)
                return; // prevent double-trigger

            s.source.Play();
            return;
        }

        // ================================
        // 3D SOUND — avoid double spawning
        // ================================
        if (active3DSounds.Contains(s.name))
            return; // already playing in 3D

        active3DSounds.Add(s.name);

        Vector3 soundPosition = position ?? Vector3.zero;

        GameObject tempGO = new GameObject("3D Sound: " + s.name);
        tempGO.transform.position = soundPosition;

        AudioSource tempSource = tempGO.AddComponent<AudioSource>();
        tempSource.clip = s.clip;
        tempSource.outputAudioMixerGroup = s.audioMixerGroup;
        tempSource.volume = s.volume;
        tempSource.pitch = s.pitch;
        tempSource.loop = false;
        tempSource.spatialBlend = 1f;

        tempSource.Play();

        // Remove from active set when done
        Destroy(tempGO, s.clip.length);
        StartCoroutine(RemoveAfterDelay(s.name, s.clip.length));
    }

    private System.Collections.IEnumerator RemoveAfterDelay(string name, float delay)
    {
        yield return new WaitForSeconds(delay);
        active3DSounds.Remove(name);
    }
}