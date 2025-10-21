using System;
using Fusion;
using UnityEngine;

public class NetworkObjectPathFollower : NetworkBehaviour
{
    [SerializeField] private Rigidbody movingRB;
    
    [Header("Waypoints")] 
    [SerializeField] private Transform[] waypoints;
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float arriveThreshold = 0.1f;

    private int currentWaypointIndex = 0;

    public override void FixedUpdateNetwork()
    {
        if (!Object.HasStateAuthority || waypoints.Length == 0) return;

        Transform targetWaypoint = waypoints[currentWaypointIndex];
        
        // Calculate step
        Vector3 newPos = Vector3.MoveTowards(
            movingRB.position,
            targetWaypoint.position,
            moveSpeed * Runner.DeltaTime
        );
        movingRB.MovePosition(newPos);
        
        // Check if we reached the waypoint
        if (Vector3.Distance(movingRB.position, targetWaypoint.position) < arriveThreshold)
        {
            currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
        }
    }
}