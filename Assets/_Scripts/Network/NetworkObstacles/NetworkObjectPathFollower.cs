using System;
using System.Collections.Generic;
using Fusion;
using UnityEngine;

public class NetworkObjectPathFollower : NetworkBehaviour, ITriggerReceiver
{
    [SerializeField] private Rigidbody movingRB;

    [Header("Waypoints")]
    [SerializeField] private Transform[] waypoints;
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float arriveThreshold = 0.1f;

    private int currentWaypointIndex = 0;
    private Vector3 lastPosition;
    private Vector3 frameDelta;

    private readonly HashSet<NetworkPlayer> playersOnPlatform = new();

    public override void Spawned()
    {
        lastPosition = movingRB.position;
    }

    public override void FixedUpdateNetwork()
    {
        if (!Object.HasStateAuthority || waypoints.Length == 0) return;

        // --- Move the platform ---
        Vector3 oldPos = movingRB.position;
        Transform target = waypoints[currentWaypointIndex];

        Vector3 newPos = Vector3.MoveTowards(
            oldPos,
            target.position,
            moveSpeed * Runner.DeltaTime
        );

        movingRB.MovePosition(newPos);

        // --- Calculate how far the platform moved this tick ---
        frameDelta = newPos - oldPos;

        // --- Update last position for next frame ---
        lastPosition = newPos;

        // --- Check if waypoint reached ---
        if (Vector3.Distance(newPos, target.position) < arriveThreshold)
            currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;

        // --- Apply platform motion to players standing on it ---
        foreach (var player in playersOnPlatform)
        {
            if (player == null) continue;

            Rigidbody rb = player.GetComponent<Rigidbody>();
            if (rb != null)
            {
                // Move the player by the same delta as the platform
                rb.position += frameDelta;
            }
        }
    }

    public void OnChildTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        var player = other.GetComponentInParent<NetworkPlayer>();
        if (player != null)
            playersOnPlatform.Add(player);
    }

    public void OnChildTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        var player = other.GetComponentInParent<NetworkPlayer>();
        if (player != null)
            playersOnPlatform.Remove(player);
    }
}
