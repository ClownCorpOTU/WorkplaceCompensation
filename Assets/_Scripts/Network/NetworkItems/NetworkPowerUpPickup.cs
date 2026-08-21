using Fusion;
using UnityEngine;


public class NetworkPowerUpPickup : NetworkBehaviour
{
    [SerializeField] private PowerUpItemSO itemData;

    private void OnTriggerEnter(Collider other)
    {
        if (!Object.IsValid) return;
        if (!Object.HasStateAuthority) return;
        
        NetworkPlayer player = other.GetComponentInParent<NetworkPlayer>();
        if (player == null) return;
        
        NetworkPlayerPowerUpInventory inventory = player.GetComponent<NetworkPlayerPowerUpInventory>();
        if (inventory == null) return;
        
        // Find an empty slot and give the player the item
        bool pickedUp = false;

        for (int i = 0; i < 4; i++)
        {
            if (inventory.InventorySlots[i] == 0)
            {
                inventory.InventorySlots.Set(i, itemData.ItemID);
                pickedUp = true;
                break;
            }
        }
        
        if (pickedUp)
            Runner.Despawn(Object);
    }
}