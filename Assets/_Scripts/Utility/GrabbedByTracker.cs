using Fusion;
using UnityEngine;

public class GrabbedByTracker : NetworkBehaviour
{
    [HideInInspector, Networked] public PlayerRef LastHeldBy { get; private set; }
    
    public void OnGrabbedBy(NetworkPlayer player)
    {
        LastHeldBy = player.Object.InputAuthority;
    }
}
