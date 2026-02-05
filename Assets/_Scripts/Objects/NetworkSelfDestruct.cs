using Fusion;
using UnityEngine;

public class NetworkSelfDestruct : NetworkBehaviour
{
    [SerializeField] private float selfDestructTime = 5f;
    
    [Networked] private TickTimer selfDestructTimer { get; set; }
    
    public override void Spawned()
    {
        selfDestructTimer = TickTimer.CreateFromSeconds(Runner, selfDestructTime);
    }
    
    public override void FixedUpdateNetwork()
    {
        if (selfDestructTimer.Expired(Runner))
        {
            Runner.Despawn(Object);
        }
    }
}