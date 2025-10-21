using UnityEngine;

public interface ICollisionReceiver
{
    void OnChildCollisionEnter(Collision collision);
    void OnChildCollisionExit(Collision collision);
}