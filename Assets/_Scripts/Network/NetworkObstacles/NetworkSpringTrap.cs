using System;
using Fusion;
using UnityEngine;

public class NetworkSpringTrap : NetworkBehaviour
{
    [SerializeField] private float launchForce = 20f;
    [SerializeField] private Vector3 launchDirection = new Vector3(0, 1, 0);
    [SerializeField] private float resetDelaySeconds = 0.2f;

    private Rigidbody rb;
    private Vector3 ogPos;
    private Vector3 ogRot;
    private TickTimer resetDelay;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        ogPos = transform.position;
        ogRot = transform.eulerAngles;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!Object.HasStateAuthority) return;
        if (!other.CompareTag("Player")) return;

        // Launch the player
        if (other.TryGetComponent(out Rigidbody otherRB))
            otherRB.AddForce(launchDirection * launchForce, ForceMode.Impulse);

        // Small bounce for trap
        rb.AddForce(launchDirection * 25f, ForceMode.Impulse);

        // Begin reset countdown
        resetDelay = TickTimer.CreateFromSeconds(Runner, resetDelaySeconds);
    }

    public override void FixedUpdateNetwork()
    {
        if (resetDelay.IsRunning && resetDelay.Expired(Runner))
        {
            ResetPosition();
            resetDelay = TickTimer.None;
        }
    }

    private void ResetPosition()
    {
        print("Resetting spring trap");
        
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.MovePosition(ogPos);
        rb.MoveRotation(Quaternion.Euler(ogRot));
    }
}