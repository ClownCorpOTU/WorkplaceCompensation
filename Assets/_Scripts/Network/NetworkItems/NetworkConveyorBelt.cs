using System;
using System.Collections.Generic;
using Fusion;
using UnityEngine;

public class NetworkConveyorBelt : NetworkBehaviour
{
    [SerializeField] private float beltSpeed = 5f;
    [SerializeField] private Vector3 direction = new Vector3(1,0,0);
    [SerializeField] private float maxActiveDistance = 7f;

    private List<Rigidbody> onBelt = new List<Rigidbody>();

    
    public override void FixedUpdateNetwork()
    {
        if (!Object.HasStateAuthority) return;
        if (onBelt == null || onBelt.Count < 1) return;
        
        for (int i = onBelt.Count - 1; i >= 0; i--)
        {
            Rigidbody rb = onBelt[i];

            // If object isn't active or is too far away, remove it and continue
            // Prevents edge cases (Eg - Player dies on the belt. When they repsawn, they still have the belt velocity applied)
            if (rb == null || !rb.gameObject.activeInHierarchy || 
                Vector3.Distance(rb.position, transform.position) > maxActiveDistance)
            {
                onBelt.RemoveAt(i);
                continue;
            }
            
            rb.linearVelocity = direction.normalized * beltSpeed * Runner.DeltaTime;
        }
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.TryGetComponent(out Rigidbody otherRb))
        {
            if (!onBelt.Contains(otherRb)) onBelt.Add(otherRb);
        }
    }

    private void OnCollisionExit(Collision other)
    {
        if (other.gameObject.TryGetComponent(out Rigidbody otherRb))
        {
            if (onBelt.Contains(otherRb)) onBelt.Remove(otherRb);
        }
    }
}