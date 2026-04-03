using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using Random = UnityEngine.Random;

public class AudioManager : MonoBehaviour
{
    [Header("Object Pooling Settings")] 
    [SerializeField] private int poolSize = 12;
    private Queue<AudioSource> pooled3DSources = new Queue<AudioSource>();
    
    [Header("Audio Clips")]
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

        // Initialize 2D sounds
        foreach (Sound s in sounds)
        {
            if (!s.is3D) Populate2DSounds(s);
        }
        
        // Pre-warm 3D pool
        for (int i = 0; i < poolSize; i++)
        {
            CreateNewPoolObject();
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

    private void CreateNewPoolObject()
    {
        GameObject go = new GameObject("Pooled_3D_Source");
        go.transform.SetParent(transform);
        AudioSource source = go.AddComponent<AudioSource>();
        go.SetActive(false);
        pooled3DSources.Enqueue(source);
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
        // 3D SOUND — avoid double spawning; includes object pooling
        // ================================
        if (s.usePooling)
        {
            PlayPooled3DSound(soundToPlay, position ?? Vector3.zero, s.name);
        }
        else
        {
            PlayStandard3DSound(soundToPlay, position ?? Vector3.zero, s.name);
        }
    }

    private void PlayPooled3DSound(Sound s, Vector3 position, string soundName)
    {
        if (!s.allowOverlap && active3DSounds.Contains(soundName)) return;
        
        if (pooled3DSources.Count == 0) CreateNewPoolObject();

        AudioSource source = pooled3DSources.Dequeue();
        source.gameObject.SetActive(true);
        source.transform.position = position;
        
        // Apply settings
        source.clip = s.clip;
        source.outputAudioMixerGroup = s.audioMixerGroup;
        source.volume = s.volume;
        source.pitch = s.pitch;
        source.spatialBlend = 1f;
        source.loop = s.loop;
        
        source.Play();
        active3DSounds.Add(soundName);

        StartCoroutine(ReturnToPool(source, s.clip.length, soundName));
    }

    private IEnumerator ReturnToPool(AudioSource source, float clipLength, string soundName)
    {
        yield return new WaitForSeconds(clipLength);
        source.gameObject.SetActive(false);
        pooled3DSources.Enqueue(source);
        active3DSounds.Remove(soundName);
    }

    private void PlayStandard3DSound(Sound s, Vector3 position, string soundName)
    {
        if (!s.allowOverlap && active3DSounds.Contains(soundName)) return;
        active3DSounds.Add(soundName);

        GameObject tempGO = new GameObject("3D_Sound_" + s.name);
        tempGO.transform.position = position;
        AudioSource tempSource = tempGO.AddComponent<AudioSource>();
        
        // Copy settings from the chosen soundToPlay (variation or base)
        tempSource.clip = s.clip;
        tempSource.outputAudioMixerGroup = s.audioMixerGroup;
        tempSource.volume = s.volume;
        tempSource.pitch = s.pitch;
        tempSource.spatialBlend = 1f; // Force 3D
        tempSource.loop = s.loop;

        tempSource.Play();

        // Cleanup
        float clipLength = s.clip.length;
        Destroy(tempGO, clipLength);
        StartCoroutine(RemoveAfterDelay(s.name, clipLength));
    }

    private System.Collections.IEnumerator RemoveAfterDelay(string name, float delay)
    {
        yield return new WaitForSeconds(delay);
        active3DSounds.Remove(name);
    }
}