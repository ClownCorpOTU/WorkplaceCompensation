using Fusion;
using UnityEngine;
using UnityEngine.UI;

public class NetworkPlayerPowerUpInventory : NetworkBehaviour
{
    [SerializeField] private PowerUpItemSO[] allPowerUpItems;
    [SerializeField] private Transform handHoldPoint;
    
    [Networked, Capacity(4), OnChangedRender(nameof(UpdateVisuals))] public NetworkArray<int> InventorySlots { get; }
    [Networked, OnChangedRender(nameof(UpdateVisuals))] private int NetworkSelectedSlot { get; set; }

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

        switch (itemToUse.PowerUpType)
        {
            // Do the effect
            case PowerUpType.Deployable when itemToUse.DeployablePrefab != null:
            {
                // Spawn the item directly in front of the player
                Vector3 spawnPos = transform.position + transform.forward * 1.5f;
                Runner.Spawn(itemToUse.DeployablePrefab, spawnPos, transform.rotation);
            InventorySlots.Set(NetworkSelectedSlot, 0);
                break;
            }
            case PowerUpType.Consumable:
            {
                if (itemToUse.ConsumableEffect != null)
                {
                    NetworkPlayer player = GetComponent<NetworkPlayer>();
                    itemToUse.ConsumableEffect.ApplyEffect(player);
                    InventorySlots.Set(NetworkSelectedSlot, 0);
                }

                break;
            }
            case PowerUpType.Handheld:
            {
                if (itemToUse.HandheldEffect != null)
                {
                    NetworkPlayer player = GetComponent<NetworkPlayer>();
                    itemToUse.HandheldEffect.UseHandheldItem(player);
                }

                break;
            }
        }
        
        // Remove item from the inventory
        // InventorySlots.Set(NetworkSelectedSlot, 0); -> Not the best for handheld items since they might have their own despawn
    }

    private void UpdateVisuals()
    {
        // Update inventory selection visuals
        if (Object.HasInputAuthority && PowerUpInventoryUI.Instance != null)
        {
            int[] currentItems = new int[4];
            for (int i = 0; i < 4; i++)
                currentItems[i] = InventorySlots[i];
            
            PowerUpInventoryUI.Instance.RefreshUI(currentItems, NetworkSelectedSlot, allPowerUpItems);
        }
        
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
                
                currentlySpawnedVisual.transform.localPosition = item.HeldPos;
                currentlySpawnedVisual.transform.localScale = item.HeldScale;
                
                currentlySpawnedVisual.transform.localRotation = item.HeldRot == Vector3.zero 
                    ? Quaternion.identity 
                    : Quaternion.Euler(item.HeldRot);
                    
                break;
            }
        }
    }
}