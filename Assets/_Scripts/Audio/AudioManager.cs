using System;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public Sound[] sounds;
    public static AudioManager instance;

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

            s.source.volume = s.volume;
            s.source.pitch = s.pitch;
            s.source.loop = s.loop;
            s.source.spatialBlend = s.is3D ? 1f : 0f;
        }
    }

    public void Play(string name, Vector3? position = null) {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if (s == null) {
            Debug.LogWarning("Sound: " + name + "not found!");
            return;
        }

        if (!s.is3D)
        {
            s.source.Play();
            return;
        }
        
        // Otherwise, create a temporary 3D AudioSource at the given position
        Vector3 soundPosition = position ?? Vector3.zero;

        GameObject tempGO = new GameObject("3D Sound: " + s.name);
        tempGO.transform.position = soundPosition;

        AudioSource tempSource = tempGO.AddComponent<AudioSource>();
        tempSource.clip = s.clip;
        tempSource.volume = s.volume;
        tempSource.pitch = s.pitch;
        tempSource.loop = false; // Usually 3D sounds aren't looping (unless you want ambience)
        tempSource.spatialBlend = 1f; // Fully 3D

        tempSource.Play();
        Destroy(tempGO, s.clip.length); // Destroy after playback
    }
}