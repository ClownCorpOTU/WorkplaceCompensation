using System;
using Fusion;
using UnityEngine;

public class NetworkFlattenBlobby : NetworkBehaviour
{
    private void OnCollisionEnter(Collision other)
    {
        if (!Object.HasStateAuthority) return;

        if (other.gameObject.CompareTag("Player"))
        {
            // Look in the object OR its parents for the NetworkPlayer script
            if (other.gameObject.TryGetComponent<NetworkPlayer>(out var player))
            {
                player.FlattenAndMakeRagdoll();
            }
            else if (other.transform.root.TryGetComponent<NetworkPlayer>(out var rootPlayer))
            {
                rootPlayer.FlattenAndMakeRagdoll();
            }
            else
            {
                Utils.DebugLogError("Cannot find player network component on " + other.gameObject.name + " or " + other.transform.root.name);
            }
        }
    }
}