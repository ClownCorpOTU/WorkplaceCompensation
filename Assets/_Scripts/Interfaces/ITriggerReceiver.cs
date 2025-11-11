using UnityEngine;

public interface ITriggerReceiver
{
    void OnChildTriggerEnter(Collider other, TriggerType tType = TriggerType.Left);
    void OnChildTriggerExit(Collider other, TriggerType tType = TriggerType.Left);
}