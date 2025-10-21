using System;
using UnityEngine;

public class CollisionForwarder : MonoBehaviour
{
    [SerializeField] private MonoBehaviour receiver;
    private ICollisionReceiver collisionReceiver;

    private void Awake()
    {
        collisionReceiver = receiver as ICollisionReceiver;

        if (collisionReceiver == null)
            Utils.DebugLogError($"{receiver.name} does not implement ICollisionReceiver!");
    }

    private void OnCollisionEnter(Collision other)
    {
        collisionReceiver?.OnChildCollisionEnter(other);
    }
    
    private void OnCollisionExit(Collision other)
    {
        collisionReceiver?.OnChildCollisionExit(other);
    }
}