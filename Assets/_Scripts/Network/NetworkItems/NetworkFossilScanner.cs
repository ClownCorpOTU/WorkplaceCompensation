using System.Collections;
using Fusion;
using UnityEngine;

public class NetworkFossilScanner : NetworkBehaviour
{
    [Header("Settings")]
    [SerializeField] private float maxBatteryLife = 60f;
    [Networked] public float CurrentBattery { get; set; }
    [Networked, HideInInspector] public NetworkBool IsActive { get; set; }
    [SerializeField] private float heightOffset = 0.1f;

    [Header("Detection")]
    [SerializeField] private float detectionRange = 20f;

    [Header("Audio Settings")] 
    [SerializeField] private Vector2 tickDelayRange = new Vector2(0.1f, 1.5f);
    [SerializeField] private Vector2 tickPitchRange = new Vector2(0.9f, 1.1f);
    [SerializeField] private Vector2 tickVolumeRange = new Vector2(0.5f, 1.0f);

    [Header("Visuals")]
    [SerializeField] private MeshRenderer[] ledArray;
    [SerializeField] private Gradient signalGradient;

    private NetworkFossilManager fossilManager;
    private AudioSource tickSource;
    private float nextBeepTime;
    private float currentVolume;
    
    public override void Spawned()
    {
        fossilManager = FindFirstObjectByType<NetworkFossilManager>();
        tickSource = GetComponent<AudioSource>();
        
        if (Object.HasStateAuthority) CurrentBattery = maxBatteryLife;
    }

    public override void FixedUpdateNetwork()
    {
        if (CurrentBattery > 0)
        {
            if (Object.HasStateAuthority && IsActive)
            {
                CurrentBattery -= Runner.DeltaTime;
                
                // Keep model grounded
                if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 2f))
                {
                    float targetY = hit.point.y + heightOffset;
                    transform.position = new Vector3(transform.position.x, targetY, transform.position.z);
                    
                    // Align rotation to slope
                    transform.rotation = Quaternion.FromToRotation(transform.up, hit.normal) * transform.rotation;
                }
            }
        }
        else
        {
            // Battery can be recharged by taking the scanner back to it's original position
            OnBatteryDead();
        }
    }

    public override void Render()
    {
        if (IsActive && CurrentBattery > 0) UpdateScannerFeedback();
    }

    private void UpdateScannerFeedback()
    {
        int closestIndex = fossilManager.GetClosestFossilIndex(transform.position, out Vector3 fossilPos);

        if (closestIndex != -1)
        {
            float distance = Vector3.Distance(transform.position, fossilPos);

            if (distance <= detectionRange)
            {
                print($"Closest fossil is {distance} units away.");
                
                // Normalize distance (0 = at fossil; 1 = max range)
                float t = Mathf.Clamp01(distance / detectionRange);
                
                // Beep faster when t is closer to 0
                float currentDelay = Mathf.Lerp(tickDelayRange.x, tickDelayRange.y, t);
                
                // Volume increases with proximity
                currentVolume = Mathf.Lerp(tickVolumeRange.y, tickVolumeRange.x, t);

                if (Time.time > nextBeepTime)
                {
                    PlayBeep();
                    nextBeepTime = Time.time + currentDelay;
                }
                
                // Lights
                UpdateDirectionalVisuals(fossilPos);
            }
        }
    }

    private void PlayBeep()
    {
        if (tickSource == null) return;

        tickSource.volume = currentVolume;
        tickSource.pitch = Random.Range(tickPitchRange.x, tickPitchRange.y);
        tickSource.PlayOneShot(tickSource.clip);
    }

    private void UpdateDirectionalVisuals(Vector3 fossilPos)
    {
        // Get direction to fossil
        Vector3 dirToFossil = (fossilPos - transform.position).normalized;
        
        // Calculate forward and right
        float forwardDot = Vector3.Dot(transform.forward, dirToFossil);
        float rightDot = Vector3.Dot(transform.right, dirToFossil);
        
        // Update each LED
        for (int i = 0; i < ledArray.Length; i++)
        {
            // Calculate target dot (Left LED is -1.0; center is 0.0; Right is 1.0)
            float ledTargetWeight = Mathf.Lerp(-1f, 1f, (float)i / (ledArray.Length - 1));
            
            // How much does this LED match the current direction?
            float intensity = Mathf.Max(0, 1f - Mathf.Abs(rightDot - ledTargetWeight));
            
            // Only light up bright if we are also facing generally towards it
            if (forwardDot < 0) intensity *= 0.2f; // Dim if it's behind us

            Color finalColor = signalGradient.Evaluate(intensity);
            ledArray[i].material.SetColor("_BaseColor", finalColor * intensity * 5f);
        }
    }

    private void OnBatteryDead()
    {
        print("Battery is dead!");
    }
}