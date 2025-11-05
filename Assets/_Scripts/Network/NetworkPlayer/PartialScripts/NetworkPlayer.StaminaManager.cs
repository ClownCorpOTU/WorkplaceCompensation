using Fusion;
using UnityEngine;

public partial class NetworkPlayer
{
    
    [Header("Stamina Settings")]
    [SerializeField] private float maxStamina = 15f;
    [SerializeField] private float minStaminaDrainRate = 1f;
    [SerializeField] private float maxStaminaDrainRate = 5f;
    [SerializeField] private float regenDelay = 1f; // seconds after activity stops
    [SerializeField, Tooltip("Stamina per second")] private float regenRate = 3f;

    [Networked, HideInInspector] public float CurrentStamina { get; private set; } = 15f;
    
    private float staminaDrainRate;
    private float lastActivityTime = 0f;

    
    private void HandleStamina()
    {
        if (!Object.HasStateAuthority) return;

        bool isUsingStamina = false;

        // Reduce stamina while grabbing
        if (IsGrabbingActive || IsLeftHandGrabbingActive || IsRightHandGrabbingActive)
        {
            // Heavier objects drain more
            if (playerGrab.CurrentlyGrabbedRigidbody != null)
            {
                var otherObject = playerGrab.CurrentlyGrabbedRigidbody.transform.root;
                
                if (otherObject.TryGetComponent(out NetworkPlayer otherPlayer))
                {
                    staminaDrainRate = otherPlayer.IsActiveRagdoll ? maxStaminaDrainRate : 2.5f;
                }
                else
                {
                    staminaDrainRate = Mathf.Clamp(playerGrab.CurrentlyGrabbedRigidbody.mass * 0.5f, minStaminaDrainRate, maxStaminaDrainRate);
                }
            }

            CurrentStamina -= staminaDrainRate * Runner.DeltaTime;
            isUsingStamina = true;

            // Auto-release if exhausted
            if (CurrentStamina <= 0f)
            {
                playerGrab.ForceRelease();
                CurrentStamina = 0f;
            }
        }

        // Regeneration
        if (isUsingStamina)
        {
            lastActivityTime = Runner.SimulationTime;
        }
        else if (Runner.SimulationTime - lastActivityTime > regenDelay)
        {
            CurrentStamina = Mathf.Min(maxStamina, CurrentStamina + (regenRate * Runner.DeltaTime));
        }
    }

    public float NormalizeStamina()
    {
        return CurrentStamina / maxStamina;
    }
}