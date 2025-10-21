using System;
using Fusion;
using UnityEngine;

public class NetworkObjectRotator : NetworkBehaviour
{
    [SerializeField] private Transform rotatorTransform;
    [SerializeField] private Rigidbody rotatorRB;
    [SerializeField] private Vector3 rotationAmount;
    
    public override void FixedUpdateNetwork()
    {
        if (Object.HasStateAuthority)
        {
            Vector3 rotateBy = transform.rotation.eulerAngles + rotationAmount * Runner.DeltaTime;
            
            if (rotatorRB == null)
                rotatorTransform.rotation = Quaternion.Euler(rotateBy);
            //else
                //rotatorRB.MoveRotation(Quaternion.Euler(rotateBy));
        }
    }
}