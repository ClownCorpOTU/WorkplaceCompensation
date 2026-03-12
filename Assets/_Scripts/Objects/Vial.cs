using System;
using Fusion;
using UnityEngine;

public class Vial : NetworkBehaviour
{
    [Networked] public VialType Type { get; set; }
    [HideInInspector] public PlayerRef LastHeldBy { get; private set; }
    
    public void Initialize(VialType type)
    {
        if (Object.HasStateAuthority) Type = type;
    }

    public void OnGrabbedBy(NetworkPlayer player)
    {
        LastHeldBy = player.Object.InputAuthority;
    }

    private void OnCollisionEnter(Collision other)
    {
        AudioManager.instance.Play("BoxDrop");
    }
}