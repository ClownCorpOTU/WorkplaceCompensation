using System;
using Fusion;
using UnityEngine;

public class NetworkAcid : NetworkBehaviour
{
    [SerializeField] private float acidKillDelay = 5f;
    [SerializeField] private int scoreToAdd = 1;
    
    private NetworkGameManager networkGameManager;
    private Vial vialToDespawn;
    private bool hasThrownTrashBefore;
    
    [Networked] private TickTimer acidKillTimer { get; set; }
    
    
    public override void Spawned()
    {
        networkGameManager = FindFirstObjectByType<NetworkGameManager>();
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (!Object.HasStateAuthority) return;

        // Get a realistic position for the sound (approximate contact point)
        Vector3 contactPoint = other.ClosestPoint(transform.position);
        AudioManager.instance.Play("AcidMelt", contactPoint);

        if (other.TryGetComponent(out Vial vial) && vial.Type == VialType.TrashBag)
        {
            networkGameManager.AddScore(vial.LastHeldBy, scoreToAdd);
            RPC_TriggerTutorialEvent(vial.LastHeldBy, (int)GameEvent.VialsMixed);

            // Start timer
            vialToDespawn = vial;
            acidKillTimer = TickTimer.CreateFromSeconds(Runner, acidKillDelay);
        }
    }

    
    public override void FixedUpdateNetwork()
    {
        if (!Object.HasStateAuthority) return;

        if (acidKillTimer.Expired(Runner) && vialToDespawn != null)
        {
            Runner.Despawn(vialToDespawn.Object);
            vialToDespawn = null;
            acidKillTimer = TickTimer.None;
        }
    }
    
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_TriggerTutorialEvent([RpcTarget] PlayerRef player, int eventEnumInt)
    {
        if (!hasThrownTrashBefore)
        {
            GameEventManager.TriggerEvent(GameEvent.TrashDeposited);
            hasThrownTrashBefore = true;
        }
    }
}
