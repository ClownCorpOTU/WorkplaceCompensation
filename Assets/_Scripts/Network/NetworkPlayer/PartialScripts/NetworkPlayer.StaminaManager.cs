using Fusion;
using UnityEngine;

public partial class NetworkPlayer
{
    [Header("Stamina Settings")]
    [Networked] public float Stamina { get; private set; } = 15f;
    [SerializeField] private float maxStamina = 15f;
    [SerializeField] private float regenDelay = 1f; // seconds after activity stops
    [SerializeField, Tooltip("Stamina per second")] private float regenRate = 3f;
    
    private float lastActivityTime = 0f;

    private void HandleStamina()
    {
        if (!Object.HasStateAuthority) return;

        bool isUsingStamina = false;

        // --- Grabbing Cost ---
        if (IsGrabbingActive || IsLeftHandGrabbingActive || IsRightHandGrabbingActive)
        {
            // Example: heavier objects drain more
            float grabDrainRate = 1f;
            if (playerGrab.CurrentlyGrabbedRigidbody != null)
                grabDrainRate = Mathf.Clamp(playerGrab.CurrentlyGrabbedRigidbody.mass * 0.5f, 1f, 5f);

            Stamina -= grabDrainRate * Runner.DeltaTime;
            isUsingStamina = true;

            // Auto-release if exhausted
            if (Stamina <= 0f)
            {
                playerGrab.ForceRelease();
                Stamina = 0f;
            }
        }

        // --- Regeneration ---
        if (isUsingStamina)
        {
            lastActivityTime = Runner.SimulationTime;
        }
        else if (Runner.SimulationTime - lastActivityTime > regenDelay)
        {
            Stamina = Mathf.Min(maxStamina, Stamina + regenRate * Runner.DeltaTime);
        }
    }
}