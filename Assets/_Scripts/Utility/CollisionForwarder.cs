using System;
using System.Linq;
using UnityEngine;

public class CollisionForwarder : MonoBehaviour
{
    [SerializeField] private MonoBehaviour receiver;
    [SerializeField] private MonoBehaviour[] multipleReceivers; // I don't want to update the above because it'll break reference for a lot of places
    private ICollisionReceiver collisionReceiver;
    private ICollisionReceiver[] collisionReceivers;

    private void Awake()
    {
        var receivers = new System.Collections.Generic.List<ICollisionReceiver>();

        // Keep original single receiver
        if (receiver != null)
        {
            var r = receiver as ICollisionReceiver;
            if (r != null)
                receivers.Add(r);
            else
                Utils.DebugLogError($"{receiver.name} does not implement ICollisionReceiver!");
        }

        // Add additional receivers
        foreach (var mb in multipleReceivers)
        {
            if (mb == null) continue;

            var r = mb as ICollisionReceiver;
            if (r != null)
                receivers.Add(r);
            else
                Utils.DebugLogError($"{mb.name} does not implement ICollisionReceiver!");
        }

        collisionReceivers = receivers.ToArray();
    }

    private void OnCollisionEnter(Collision other)
    {
        foreach (var r in collisionReceivers)
            r.OnChildCollisionEnter(other);
    }
    
    private void OnCollisionExit(Collision other)
    {
        foreach (var r in collisionReceivers)
            r.OnChildCollisionExit(other);
    }
}