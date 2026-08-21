using Fusion;
using UnityEngine;

public class NetworkPlayerPowerUpInventory : NetworkBehaviour
{
    [SerializeField] private PowerUpItemSO[] allPowerUpItems;
    [SerializeField] private Transform handHoldPoint;
    
    [Networked, Capacity(4), OnChangedRender(nameof(UpdateVisuals))] public NetworkArray<int> InventorySlots { get; }
    [Networked, OnChangedRender(nameof(UpdateVisuals))] public int NetworkSelectedSlot { get; set; }

    private GameObject currentlySpawnedVisual;


    public override void Spawned()
    {
        UpdateVisuals();
    }

    public override void FixedUpdateNetwork()
    {
        if (GetInput(out NetworkInputData data))
        {
            if (Object.HasStateAuthority)
                NetworkSelectedSlot = data.SelectedSlotIndex;

            if (Object.HasStateAuthority && data.IsUseItemPressed)
            {
                int itemIDInSlot =  InventorySlots[NetworkSelectedSlot];

                if (itemIDInSlot != 0)
                    UseItem(itemIDInSlot);
            }
        }
    }

    private void UseItem(int itemIDInSlot)
    {
        // Find the SO that matches this ID
        PowerUpItemSO itemToUse = null;

        foreach (var item in allPowerUpItems)
        {
            if (item.ItemID == itemIDInSlot)
                itemToUse = item;
        }
        
        if (itemToUse == null) return;
        
        // Do the effect
        if (itemToUse.PowerUpType == PowerUpType.Deployable && itemToUse.DeployablePrefab != null)
        {
            // Spawn the item directly in front of the player
            Vector3 spawnPos = transform.position + transform.forward * 1.5f;
            Runner.Spawn(itemToUse.DeployablePrefab, spawnPos, transform.rotation);
        }
        else if (itemToUse.PowerUpType == PowerUpType.Consumable)
        {
            Debug.Log("Consumed " + itemToUse.PowerUpName);
        }
        
        // Remove item from the inventory
        InventorySlots.Set(NetworkSelectedSlot, 0);
    }

    private void UpdateVisuals()
    {
        // Destroy whatever we were holding before
        if (currentlySpawnedVisual != null)
            Destroy(currentlySpawnedVisual);
        
        // Check if current slot has an item
        int currentItemID = InventorySlots[NetworkSelectedSlot];
        if (currentItemID == 0) return;
        
        // Find the item and spawn it's visual
        foreach (var item in allPowerUpItems)
        {
            if (item.ItemID == currentItemID && item.HeldPrefab != null)
            {
                currentlySpawnedVisual = Instantiate(item.HeldPrefab, handHoldPoint);
                currentlySpawnedVisual.transform.parent = handHoldPoint;
                
                currentlySpawnedVisual.transform.localScale = item.HeldScale;
                
                currentlySpawnedVisual.transform.localPosition = Vector3.zero;
                currentlySpawnedVisual.transform.localRotation = Quaternion.identity;
                break;
            }
        }
    }
}