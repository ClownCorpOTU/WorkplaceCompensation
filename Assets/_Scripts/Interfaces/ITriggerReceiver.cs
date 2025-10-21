using UnityEngine;

public interface ITriggerReceiver
{
    void OnChildTriggerEnter(Collider other);
    void OnChildTriggerExit(Collider other);
}