using System;
using Fusion;
using UnityEngine;

public class NetworkLever : NetworkBehaviour
{
    [SerializeField] private GameObject lever;
    [SerializeField] private MonoBehaviour receiver;
    
    private NetworkBool IsLeverOn { get; set; }
    private ILever iLever;
    
    
    private void Awake()
    {
        lever.transform.localRotation = Quaternion.Euler(-45f, 0f, 0f);
        IsLeverOn = false;
        
        iLever = receiver as ILever;

        if (iLever == null)
            Utils.DebugLogError($"{receiver.name} does not implement ILever!");
    }

    public override void FixedUpdateNetwork()
    {
        float xRot = lever.transform.localEulerAngles.x;
        if (xRot > 180f) xRot -= 360f; // Normalize

        bool state = xRot > 0f;

        if (state != IsLeverOn)
        {
            AudioManager.instance.Play("FlickLever", transform.position);
            IsLeverOn = state;
            iLever?.OnLeverToggled(IsLeverOn);
        }
    }

    public void ToggleLeverOff()
    {
        lever.transform.localRotation = Quaternion.Euler(-45f, 0f, 0f);
        IsLeverOn = false;
        iLever?.OnLeverToggled(false);
        AudioManager.instance.Play("FlickLever", transform.position);
    }
}