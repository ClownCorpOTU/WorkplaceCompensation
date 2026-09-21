using System;
using Unity.Cinemachine;
using UnityEngine;

public class CameraShakeManager : MonoBehaviour
{
    public static CameraShakeManager Instance;

    private void Awake() {
        if (Instance == null) {
            Instance = this;
        }
        else {
            Destroy(gameObject);
            return;
        }
        //DontDestroyOnLoad(gameObject);
    }

    public void ApplyCameraShake(CinemachineImpulseSource impulseSource, Vector3 impulseVelocity, float impulseForce)
    {
        impulseSource.DefaultVelocity = impulseVelocity;
        impulseSource.GenerateImpulseWithForce(impulseForce);
    }
}