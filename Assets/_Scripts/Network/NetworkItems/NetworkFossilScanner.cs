using System.Collections;
using Fusion;
using UnityEngine;

public class NetworkFossilScanner : NetworkBehaviour
{
    [Header("Settings")]
    [SerializeField] private float maxBatteryLife = 60f;
    [Networked] public float CurrentBattery { get; set; }
    [Networked] public NetworkBool IsActive { get; set; }

    [Header("Detection")]
    [SerializeField] private float detectionRange = 20f;

    [Header("Audio Settings")] 
    [SerializeField] private Vector2 tickDelayRange = new Vector2(0.1f, 1.5f);
    [SerializeField] private Vector2 tickPitchRange = new Vector2(0.9f, 1.1f);
    [SerializeField] private Vector2 tickVolumeRange = new Vector2(0.5f, 1.0f);

    [Header("Visuals")]
    [SerializeField] private MeshRenderer signalLight;
    [SerializeField] private Gradient signalGradient;
    [SerializeField] private float minEmission = 0f;
    [SerializeField] private float maxEmission = 5f;

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
                if (signalLight == null) return;

                float intensity = 1f - t;
                Color finalColor = signalGradient.Evaluate(intensity);
                
                signalLight.material.SetColor("_BaseColor", finalColor);
                signalLight.material.SetColor("_EmissionColor", 
                    finalColor * Mathf.LinearToGammaSpace(intensity * maxEmission));
            }
        }
    }

    private void PlayBeep()
    {
        if (tickSource == null) return;

        tickSource.volume = currentVolume;
        tickSource.pitch = Random.Range(tickPitchRange.x, tickPitchRange.y);
        tickSource.PlayOneShot(tickSource.clip);

        StartCoroutine(FlashLight());
    }

    private IEnumerator FlashLight()
    {
        signalLight.material.EnableKeyword("_EMISSION");
        yield return new WaitForSeconds(0.05f);
    }

    private void OnBatteryDead()
    {
        print("Battery is dead!");
    }
}