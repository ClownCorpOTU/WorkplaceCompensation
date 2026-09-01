using UnityEngine;
using UnityEngine.Audio;

[System.Serializable]
public class Sound
{
    public string name;
    public AudioClip clip;
    public bool hasVariations;
    public AudioClip[] variations;
    public AudioMixerGroup audioMixerGroup;

    [Range(0f, 1f)] public float volume;
    [Range(0.1f, 3f)] public float pitch;

    [HideInInspector] public AudioSource source;

    public bool loop;
    public bool is3D;
    public bool usePooling;
    [Tooltip("Useful for sounds like walking that play more than once at a time")] public bool allowOverlap;
}