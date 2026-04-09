using System;
using Fusion;
using UnityEngine;

public class NetworkButton : NetworkBehaviour, ICollisionReceiver
{
    [SerializeField] private Transform buttonCap;
    [SerializeField] private Vector3 pressDepth = new Vector3(0f, 0f, 0.1f);
    [SerializeField] private float pressSpeed = 5f;
    [SerializeField] private float returnSpeed = 2f;
    
    [Networked] public NetworkBool IsButtonPressed { get; set; }

    private Vector3 initialLocalPos;
    private Vector3 targetLocalPos;

    private void Awake()
    {
        initialLocalPos = buttonCap.localPosition;
        targetLocalPos = initialLocalPos;
    }

    public override void FixedUpdateNetwork()
    {
        // Smoothly move button towards target
        buttonCap.localPosition = Vector3.Lerp(
            buttonCap.localPosition,
            targetLocalPos,
            (IsButtonPressed ? pressSpeed : returnSpeed) * Runner.DeltaTime
        );
    }

    public void OnChildCollisionEnter(Collision collision)
    {
        if (!Object.HasStateAuthority) return;

        IsButtonPressed = true;
        targetLocalPos = initialLocalPos - pressDepth;
        RPC_Play("ButtonPress", transform.position); // Can I just use the instance?
    }
    
    public void OnChildCollisionExit(Collision collision)
    {
        if (!Object.HasStateAuthority) return;
        
        IsButtonPressed = false;
        targetLocalPos = initialLocalPos;
    }
    
    [Rpc(RpcSources.StateAuthority, RpcTargets.All, TickAligned = false)]
    private void RPC_Play(string audioName, Vector3 position)
    {
        if (AudioManager.instance != null) AudioManager.instance.Play(audioName, position);
    }
}