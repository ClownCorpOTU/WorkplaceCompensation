using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThemeSong : MonoBehaviour
{
    public static ThemeSong instance;

    private AudioSource source;
    private AudioLowPassFilter lowPassFilter;

    private void Awake() {
        if (instance == null) {
            instance = this;
        }
        else {
            Destroy(gameObject);
            return;
        }
    }

    private void Start() {
        source = GetComponent<AudioSource>();
        lowPassFilter = GetComponent<AudioLowPassFilter>();
        source.Play();
    }

    public void EnableLowPassFilter(bool enable) {
        if (enable) {
            lowPassFilter.cutoffFrequency = 500f;;
        }
        else {
            lowPassFilter.cutoffFrequency = 22000f;   
        }
    }
}
