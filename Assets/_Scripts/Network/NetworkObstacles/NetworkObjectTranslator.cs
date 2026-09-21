using System;
using Fusion;
using UnityEngine;

public class NetworkObjectTranslator : NetworkBehaviour
{
    [SerializeField] private Transform translatorTransform;
    [SerializeField] private Rigidbody translatorRB;
    [SerializeField] private Vector3 translationAmount; // Direction and amplitude
    [SerializeField] private float frequency = 1f; // Oscillations per second

    private Vector3 startPos;

    private void Start()
    {
        if (translatorTransform == null) translatorTransform = transform;
        
        startPos = translatorTransform.position;
    }

    public override void FixedUpdateNetwork()
    {
        if (Object.HasStateAuthority)
        {
            float time = Runner.SimulationTime * frequency * Mathf.PI * 2f;
            Vector3 offset = translationAmount * Mathf.Sin(time);

            Vector3 newPos = startPos + offset;
            
            if (translatorRB != null)
                translatorRB.MovePosition(newPos);
            else
            {
                translatorTransform.position = newPos;
            }
        }
    }
}
