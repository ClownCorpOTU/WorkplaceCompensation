using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LocalShopUIManager : MonoBehaviour
{
	[SerializeField] private CustomizationItemSO[] allHats;
	[SerializeField] private GameObject shopItemTemplate;
	[SerializeField] private Transform shopContainer;
	[SerializeField] private Image hatImage;
	
	private List<ShopItemUI> spawnedItems = new List<ShopItemUI>();
	

	private void Start()
	{
		foreach (CustomizationItemSO item in allHats)
		{
			GameObject newObj = Instantiate(shopItemTemplate, shopContainer);
			ShopItemUI uiScript = newObj.GetComponent<ShopItemUI>();
			uiScript.Setup(item, this);
			spawnedItems.Add(uiScript);
		}
		
		PlayerInventory inv = LocalPlayerInventoryManager.LoadInventory();
		UpdateHatPreview(inv);
	}

	private void RefreshAllButtons()
	{
		foreach (ShopItemUI item in spawnedItems)
			item.RefreshButton();
	}

	public void OnItemClicked(CustomizationItemSO myItem)
	{
		PlayerInventory inv = LocalPlayerInventoryManager.LoadInventory();

		if (inv.UnlockedItemIDs.Contains(myItem.ItemID))
			EquipItem(inv, myItem);
		else
		{
			int coins = LocalEconomyManager.GetCoins();

			if (coins >= myItem.Cost)
			{
				LocalEconomyManager.AddCoins(-myItem.Cost);
				inv.UnlockedItemIDs.Add(myItem.ItemID);
				EquipItem(inv, myItem);
			}
		}

		LocalPlayerInventoryManager.SaveInventory(inv);
		RefreshAllButtons();
	}

	private void EquipItem(PlayerInventory inv, CustomizationItemSO item)
	{
		inv.EquippedItemIDs[(int)item.Category] = item.ItemID;
		
		// Send the new equipped items to the network instantly!
		if (NetworkPlayer.Local != null)
			NetworkPlayer.Local.RPC_SyncEquippedItems(inv.EquippedItemIDs);
		
		UpdateHatPreview(inv);
	}

	private void UpdateHatPreview(PlayerInventory inv)
	{
		if (hatImage == null) return;

		int equippedHatID = inv.EquippedItemIDs[0];
		CustomizationItemSO equippedHat = GetItemByID(equippedHatID);
		
		if (equippedHat != null && equippedHat.Icon != null)
		{
			hatImage.sprite = equippedHat.Icon;
			hatImage.enabled = true;
		}
		else
		{
			hatImage.sprite = null;
			hatImage.enabled = false;
		}
	}

	private CustomizationItemSO GetItemByID(int id)
	{
		foreach (CustomizationItemSO item in allHats)
		{
			if (item.ItemID == id)
				return item;
		}

		return null;
	}
}