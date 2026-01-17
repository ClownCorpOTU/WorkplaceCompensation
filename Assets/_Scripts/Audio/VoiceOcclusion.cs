using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions.Must;

public class VoiceOcclusion : MonoBehaviour
{
    [SerializeField] private float muffledFrequency = 500f;
    [SerializeField] private float clearFrequency = 22000f;
    [SerializeField] private float fadeSpeed = 10f;
    [SerializeField] private LayerMask occlusionMask;
    [SerializeField] private float occlusionCheckInterval = 0.1f;
    
    private Transform listener;
    private float targetFrequency;
    private float occlusionTimer;

    private static VoiceOcclusion instance;

    private readonly List<SpeakerEntry> speakers = new();

    private struct SpeakerEntry
    {
        public Transform transform;
        public AudioLowPassFilter lowPass;
        public bool occluded;
    }
    
    
    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        
        instance = this;
        listener = transform; // This should sit on the camera
        
        
        // Register speakers that existed before you joined
        var registers = FindObjectsByType<VoiceSpeakerRegister>(FindObjectsSortMode.None);

        foreach (var reg in registers)
        {
            // Skip self
            if (reg.transform.root == NetworkPlayer.Local.transform) continue;
            
            // Skip already registerd
            if (speakers.Exists(x => x.transform == reg.transform)) continue;

            // Register speaker
            RegisterSpeaker(reg);
        }
    }

    private void Update()
    {
        if (listener == null) return;

        occlusionTimer -= Time.deltaTime;
        
        bool shouldSample = occlusionTimer <= 0f;
        if (shouldSample)
            occlusionTimer = occlusionCheckInterval;
        
        for (int i = speakers.Count - 1; i >= 0; i--)
        {
            SpeakerEntry entry = speakers[i];
            
            if (entry.transform == null || entry.lowPass == null)
            {
                speakers.RemoveAt(i);
                continue;
            }

            if (shouldSample)
                SampleOcclusion(ref entry);

            ApplyOcclusion(ref entry);

            speakers[i] = entry; // Update entry struct
        }
    }

    private void SampleOcclusion(ref SpeakerEntry entry)
    {
        Vector3 origin = listener.position;
        Vector3 target = entry.transform.position;

        Vector3 direction = target - origin;
        float distance = direction.magnitude;

        bool blocked = false;

        if (Physics.Raycast(
                origin,
                direction.normalized,
                out RaycastHit hit,
                distance,
                occlusionMask,
                QueryTriggerInteraction.Ignore
            ))
        {
            // Only occluded if something blocks BEFORE the speaker
            if (hit.transform != entry.transform && !hit.transform.IsChildOf(entry.transform))
                blocked = true;
        }
        
        entry.occluded = blocked;
        
        DebugRaycast(entry, origin, target, hit);
    }

    private void ApplyOcclusion(ref SpeakerEntry entry)
    {
        float targetFrequency = entry.occluded ? muffledFrequency : clearFrequency;
        entry.lowPass.cutoffFrequency = Mathf.Lerp(entry.lowPass.cutoffFrequency, targetFrequency, fadeSpeed * Time.deltaTime);
    }

    private void DebugRaycast(SpeakerEntry entry, Vector3 origin, Vector3 target, RaycastHit hit)
    {
        if (entry.occluded)
        {
            Debug.DrawLine(origin, hit.point, Color.red, occlusionCheckInterval);
        }
        else
        {
            Debug.DrawLine(origin, target, Color.green, occlusionCheckInterval);
        }
    }
    
    
    // --- REGISTRATION --- //
    public static void RegisterSpeaker(VoiceSpeakerRegister register)
    {
        if (instance == null) return;
        
        AudioLowPassFilter lowPass = register.GetComponent<AudioLowPassFilter>();
        if (lowPass == null) return;
        
        instance.speakers.Add(new SpeakerEntry
        {
            transform = register.transform,
            lowPass = lowPass
        });
        
        print("Registered Speaker: " + register.transform.name);
    }
    
    public static void UnregisterSpeaker(VoiceSpeakerRegister register)
    {
        if (instance == null) return;
        
        instance.speakers.RemoveAll(e => e.transform == register.transform);
    }
}