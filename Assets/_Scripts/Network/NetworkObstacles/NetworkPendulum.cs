using System;
using Fusion;
using UnityEngine;

public class NetworkPendulum : NetworkBehaviour
{
    [SerializeField] private Transform pivotTransform;
    
    [Header("Pendulum Settings")] 
    [SerializeField] private Vector3 swingAngle = new Vector3(0,45f,0f);
    [SerializeField] private float swingSpeed = 2f;
    [SerializeField] private float phaseOffset = 0f;

    private Quaternion startRot;

    private void Start()
    {
        if (pivotTransform == null) pivotTransform = transform;

        startRot = pivotTransform.localRotation;
    }

    public override void FixedUpdateNetwork()
    {
        if (!Object.HasStateAuthority) return;
        
        // Oscillate using sine wave
        float oscillation = Mathf.Sin((float)Runner.SimulationTime * swingSpeed + phaseOffset);

        Vector3 angles = new Vector3(
            swingAngle.x * oscillation,
            swingAngle.y * oscillation,
            swingAngle.z * oscillation
        );
        
        // Apply rotation
        Quaternion swingRot = startRot * Quaternion.Euler(angles);
        pivotTransform.localRotation = swingRot;
    }
}