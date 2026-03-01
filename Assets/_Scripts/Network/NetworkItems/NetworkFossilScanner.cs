using System;
using System.Collections;
using Fusion;
using UnityEngine;
using Random = UnityEngine.Random;

public enum Axis { X, Y, Z }

public class NetworkFossilScanner : NetworkBehaviour
{
    [Header("Scanner Settings")]
    [SerializeField] private float detectionRange = 20f;
    [SerializeField] private float maxBatteryLife = 60f;
    [Networked] public float CurrentBattery { get; set; }
    [SerializeField] private float drainRate = 1f;
    [SerializeField] private float rechargeRate = 1.2f;
    [SerializeField] private float heightOffset = 0.1f;
    
    [Header("Visual Settings")] 
    [SerializeField] private Transform arrowPivot;
    [SerializeField] private MeshRenderer arrowRend;
    [SerializeField] private Gradient signalGradient;
    [SerializeField] private Vector2 signalEmissionRange = new Vector2(0f, 5f);
    [SerializeField] private Transform batteryBarPivot;
    [SerializeField] private MeshRenderer batteryBarRend;
    [SerializeField] private Gradient batteryGradient;
    [SerializeField] private Axis batteryScalingAxis = Axis.Z;
    [SerializeField] private Transform rechargeStation; 
    
    [Header("Audio Settings")]
    [SerializeField] private Vector2 tickDelayRange = new Vector2(0.1f, 1.5f);
    [SerializeField] private Vector2 tickPitchRange = new Vector2(0.9f, 1.1f);
    [SerializeField] private Vector2 tickVolumeRange = new Vector2(0.5f, 1.0f);
    
    [Networked, HideInInspector] public NetworkBool IsActive { get; set; }
    [Networked, HideInInspector] public NetworkBool IsInRechargeZone { get; set; }

    private NetworkFossilManager fossilManager;
    private AudioSource tickSource;
    private float nextBeepTime;
    private float currentVolume;
    private Vector3 batteryOriginalScale;
    private Rigidbody rb;
    
    public override void Spawned()
    {
        fossilManager = FindFirstObjectByType<NetworkFossilManager>();
        tickSource = GetComponent<AudioSource>();
        if (batteryBarPivot != null) batteryOriginalScale = batteryBarPivot.localScale;
        rb = GetComponent<Rigidbody>();
        
        if (Object.HasStateAuthority) CurrentBattery = maxBatteryLife;
    }

    public override void FixedUpdateNetwork()
    {
        if (!Object.HasStateAuthority) return;
        
        if (IsInRechargeZone && !IsActive && CurrentBattery < maxBatteryLife)
        {
            print("Charging!");
            CurrentBattery += Runner.DeltaTime * rechargeRate;
        }
        
        if (CurrentBattery > 0)
        {
            if (IsActive)
            {
                CurrentBattery -= Runner.DeltaTime * drainRate;
                
                /*
                // Make scanner stick to ground (using) Vector3.back (0, 0, -1) because Z is the vertical axis on this model)
                if (Physics.Raycast(transform.position, Vector3.back, out RaycastHit hit, 2f))
                {
                    // Apply the offset to the Z axis instead of Y
                    float targetZ = hit.point.z + heightOffset;
                    transform.position = new Vector3(transform.position.x, transform.position.y, targetZ);
    
                    // Align the model's 'forward' (local Z) to the surface normal
                    // We use transform.forward because that is the 'Up' axis for a Z-up model
                    transform.rotation = Quaternion.FromToRotation(transform.forward, hit.normal) * transform.rotation;
                }
                */
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
        if (IsActive && CurrentBattery > 0)
        {
            UpdateScannerFeedback();
        }
        UpdateBatteryVisuals();
    }

    private void UpdateScannerFeedback()
    {
        int closestIndex = fossilManager.GetClosestFossilIndex(transform.position, out Vector3 fossilPos);

        if (closestIndex != -1)
        {
            float distance = Vector3.Distance(transform.position, fossilPos);

            if (distance <= detectionRange)
            {
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
                UpdateCompass(fossilPos, t);
            }
            else
            {
                SpinArrowAimlessly();
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

    private void UpdateCompass(Vector3 fossilPos, float t)
    {
        if (arrowPivot == null) return;

        Vector3 direction = (fossilPos - transform.position);
        direction.y = 0;

        if (direction != Vector3.zero)
        {
            // Create target rotation
            Quaternion targetRot = Quaternion.LookRotation(direction);
            
            // Smoothly rotate the arrow towards the fossil
            arrowPivot.rotation = Quaternion.Slerp(arrowPivot.rotation, targetRot, Time.deltaTime * 5f);
            
            // Change color
            if (arrowRend == null) return;
            
            float intensity = 1f - t;
            Color finalColor = signalGradient.Evaluate(intensity);
            
            arrowRend.material.SetColor("_BaseColor", finalColor);
            arrowRend.material.SetColor("_EmissionColor",
                finalColor * Mathf.LinearToGammaSpace(intensity * signalEmissionRange.y));
        }
        else
        {
            SpinArrowAimlessly();
        }
    }

    private void SpinArrowAimlessly()
    {
        arrowPivot.Rotate(Vector3.up, 100f * Time.deltaTime);
        arrowRend.material.SetColor("_BaseColor", signalGradient.Evaluate(0));
        arrowRend.material.SetColor("_EmissionColor",
            signalGradient.Evaluate(0) * Mathf.LinearToGammaSpace(signalEmissionRange.x));
    }

    private void UpdateBatteryVisuals()
    {
        if (batteryBarPivot == null) return;

        // Calculate scale (Min 0.01 so lighting doesn't mess up)
        float batteryPercent = Mathf.Clamp01(CurrentBattery / maxBatteryLife);
        batteryPercent = Mathf.Max(0.01f, batteryPercent);

        // Scale bar on the needed axis
        Vector3 scale = batteryOriginalScale;

        switch (batteryScalingAxis)
        {
            case Axis.X:
                scale.x *= batteryPercent;
                break;
            case Axis.Y:
                scale.y *= batteryPercent;
                break;
            case Axis.Z:
                scale.z *= batteryPercent;
                break;
        }

        batteryBarPivot.localScale = scale;
        
        // Change color
        if (batteryBarRend != null)
        {
            Color barColor = batteryGradient.Evaluate(batteryPercent);
            batteryBarRend.material.SetColor("_BaseColor", barColor);
            batteryBarRend.material.SetColor("_EmissionColor", barColor * (2f * batteryPercent));
        }
    }

    private void OnBatteryDead()
    {
        // Create target rotation
        rechargeStation.position = new Vector3(rechargeStation.position.x, 0f, rechargeStation.position.z);
        Quaternion targetRot = Quaternion.LookRotation(rechargeStation.position);
        
        // Smoothly rotate the arrow towards the recharge station
        arrowPivot.rotation = Quaternion.Slerp(arrowPivot.rotation, targetRot, Time.deltaTime * 5f);
        
        // Set colors
        arrowRend.material.SetColor("_BaseColor", signalGradient.Evaluate(0));
        arrowRend.material.SetColor("_EmissionColor",
            signalGradient.Evaluate(0) * Mathf.LinearToGammaSpace(signalEmissionRange.x));
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("RechargeStation"))
            IsInRechargeZone = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("RechargeStation"))
            IsInRechargeZone = false;
    }
}