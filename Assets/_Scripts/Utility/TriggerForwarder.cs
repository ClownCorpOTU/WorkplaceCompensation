using System;
using UnityEngine;

public enum TriggerType
{
    Left,
    Right
}

public class TriggerForwarder : MonoBehaviour
{
    [SerializeField] private MonoBehaviour receiver;
    [SerializeField] private TriggerType triggerType = TriggerType.Left;
    
    private ITriggerReceiver triggerReceiver;

    private void Awake()
    {
        triggerReceiver = receiver as ITriggerReceiver;

        if (triggerReceiver == null)
            Utils.DebugLogError($"{receiver.name} does not implement ITriggerReceiver!");
    }

    private void OnTriggerEnter(Collider other)
    {
        triggerReceiver?.OnChildTriggerEnter(other, triggerType);
    }

    private void OnTriggerExit(Collider other)
    {
        triggerReceiver?.OnChildTriggerExit(other, triggerType);
    }
}