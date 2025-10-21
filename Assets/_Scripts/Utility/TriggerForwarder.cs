using System;
using UnityEngine;

public class TriggerForwarder : MonoBehaviour
{
    [SerializeField] private MonoBehaviour receiver;
    private ITriggerReceiver triggerReceiver;

    private void Awake()
    {
        triggerReceiver = receiver as ITriggerReceiver;

        if (triggerReceiver == null)
            Utils.DebugLogError($"{receiver.name} does not implement ITriggerReceiver!");
    }

    private void OnTriggerEnter(Collider other)
    {
        triggerReceiver?.OnChildTriggerEnter(other);
    }

    private void OnTriggerExit(Collider other)
    {
        triggerReceiver?.OnChildTriggerExit(other);
    }
}