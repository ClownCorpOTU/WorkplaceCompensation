using System;
using System.Collections;
using Fusion;
using UnityEngine;

public class NetworkDoor : NetworkBehaviour, ILever
{
    [SerializeField] private NetworkLever lever;
    [SerializeField] private float speed = 2f;
    [SerializeField] private float doorLockTime = 5f;
    [SerializeField] private float openPositionY, closedPositionY;

    private float targetY;
    private bool hasAutoUnlocked; // one time flag
    [Networked] private TickTimer doorLockTimer { get; set; }
    
    private void Start()
    {
        targetY = openPositionY;
        lever.ToggleLeverOff();
        hasAutoUnlocked = false;
    }

    public void OnLeverToggled(bool state)
    {
        if (!Object.HasStateAuthority) return;

        targetY = state ? closedPositionY : openPositionY;;
        
        if (state)
        {
            // Open door automatically after a bit

            doorLockTimer = TickTimer.CreateFromSeconds(Runner, doorLockTime);
            hasAutoUnlocked = false;
        }
        else
        {
            // Door was opened, make sure there's no timer running
            doorLockTimer = TickTimer.None;
            hasAutoUnlocked = false;
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (!Object.HasStateAuthority) return;

        if (Mathf.Abs(transform.position.y - targetY) > 0.01f)
        {
            Vector3 pos = transform.position;
            pos.y = Mathf.MoveTowards(pos.y, targetY, speed * Runner.DeltaTime);
            transform.position = pos;
        }

        if (doorLockTimer.ExpiredOrNotRunning(Runner) && !hasAutoUnlocked)
        {
            lever.ToggleLeverOff();
            hasAutoUnlocked = true;
        }
    }
}