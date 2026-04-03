using System;
using UnityEngine;
using Fusion;
using System.Collections.Generic;
using UnityEngine.Serialization;

public class WorkplaceWindFan : NetworkBehaviour
{
    [SerializeField] private float windPower = 12f;
    [SerializeField] private LayerMask affectedLayers;
    [SerializeField] private float resetDistance = 50f;
    [SerializeField] private Transform[] resetPoints;

    private int nextResetIndex = 0;

    // Items currently in the wind
    private Dictionary<Rigidbody, Rigidbody[]> activeEntities = new Dictionary<Rigidbody, Rigidbody[]>();
    // Items outside the wind, but being watched for a distance reset
    private List<Rigidbody> resetWatchlist = new List<Rigidbody>();
    // Dictionary to remember how each object should be rotated when it spawns
    private Dictionary<Rigidbody, Quaternion> originalRotations = new Dictionary<Rigidbody, Quaternion>();
    
    
    public override void FixedUpdateNetwork()
    {
        if (!Object.HasStateAuthority) return;

        // Apply wind force
        foreach (var entity in activeEntities)
        {
            foreach (Rigidbody rb in entity.Value)
            {
                if (rb == null) continue;
                rb.AddForce(Vector3.up * windPower, ForceMode.Acceleration);
            }
        }
        
        // Check distance for reset
        HandleResets();
    }

    private void HandleResets()
    {
        for (int i = resetWatchlist.Count - 1; i >= 0; i--)
        {
            Rigidbody rb = resetWatchlist[i];

            if (rb == null)
            {
                resetWatchlist.RemoveAt(i);
                continue;
            }
            
            // Check distance from fan
            if (Vector3.Distance(transform.position, rb.position) > resetDistance)
            {
                RespawnObject(rb);
                resetWatchlist.RemoveAt(i);
            }
        }
    }

    private void RespawnObject(Rigidbody rb)
    {
        if (resetPoints.Length == 0) return;

        Transform targetPoint = resetPoints[nextResetIndex];
        
        // Teleport
        rb.position = targetPoint.position;

        if (originalRotations.ContainsKey(rb))
            rb.rotation = originalRotations[rb];
        
        // Stop physics
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        
        nextResetIndex = (nextResetIndex + 1) % resetPoints.Length;
    }

    private void OnTriggerEnter(Collider other)
    {
        // Check if the layer matches (Players, Props, etc.)
        if (((1 << other.gameObject.layer) & affectedLayers) != 0)
        {
            // Find the NetworkObject this collider belongs to
            Rigidbody mainRB = GetMainRigidbody(other);

            if (mainRB != null)
            {
                if (!originalRotations.ContainsKey(mainRB)) originalRotations.Add(mainRB, mainRB.rotation);
                if (resetWatchlist.Contains(mainRB)) resetWatchlist.Remove(mainRB);

                if (!activeEntities.ContainsKey(mainRB))
                {
                    Rigidbody[] allParts = mainRB.transform.GetComponentsInChildren<Rigidbody>();
                
                    activeEntities.Add(mainRB, allParts);
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Rigidbody mainRB = GetMainRigidbody(other);
        
        if (mainRB != null && activeEntities.ContainsKey(mainRB))
        {
            activeEntities.Remove(mainRB);
            if (!resetWatchlist.Contains(mainRB)) resetWatchlist.Add(mainRB);
        } 
    }
    
    private Rigidbody GetMainRigidbody(Collider other)
    {
        // Try to find the NetworkObject first (Best for Players/Networked Items)
        NetworkObject netObj = other.GetComponentInParent<NetworkObject>();
        if (netObj != null) return netObj.GetComponent<Rigidbody>();

        // Fallback: Just get the Rigidbody attached to this collider (Best for simple props)
        return other.attachedRigidbody;
    }
}