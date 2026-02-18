using Fusion;
using UnityEngine;

public class NetworkFossilScanner : NetworkBehaviour
{
    [Header("Settings")]
    [SerializeField] private float maxBatteryLife = 60f;
    [Networked] public float CurrentBattery { get; set; }

    [Header("Detection")]
    [SerializeField] private float detectionRange = 20f;

    private NetworkFossilManager fossilManager;
    [HideInInspector] public bool IsActive = false;

    public override void Spawned()
    {
        fossilManager = FindFirstObjectByType<NetworkFossilManager>();
        if (Object.HasStateAuthority) CurrentBattery = maxBatteryLife;
    }

    public override void FixedUpdateNetwork()
    {
        if (CurrentBattery > 0)
        {
            if (Object.HasStateAuthority && IsActive)
            {
                CurrentBattery -= Runner.DeltaTime;
                UpdateScannerFeedback();
            }
        }
        else
        {
            // Battery can be recharged by taking the scanner back to it's original position
            OnBatteryDead();
        }
    }

    private void UpdateScannerFeedback()
    {
        int closestIndex = fossilManager.GetClosestFossilIndex(transform.position, out Vector3 fossilPos);

        if (closestIndex != -1)
        {
            float distance = Vector3.Distance(transform.position, fossilPos);

            if (distance <= detectionRange)
            {
                // TODO: Trigger beeping logic based on distance
                PlayBeep(distance);
            }
        }
    }

    private void PlayBeep(float distance)
    {
        print($"Closest fossil is {distance} units away.");
    }

    private void OnBatteryDead()
    {
        print("Battery is dead!");
    }
}