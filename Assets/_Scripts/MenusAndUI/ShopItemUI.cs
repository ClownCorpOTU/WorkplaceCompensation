using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopItemUI : MonoBehaviour
{
    public TextMeshProUGUI NameText;
    public TextMeshProUGUI PriceText;
    public Image IconImage;
    public Button InteractionButton;
    public TextMeshProUGUI ButtonText;

    private CustomizationItemSO myItem;
    private LocalShopUIManager myManager;

    public void Setup(CustomizationItemSO item, LocalShopUIManager manager)
    {
        myItem = item;
        myManager = manager;
        
        NameText.text = item.name;
        PriceText.text = $"{item.Cost.ToString()} coins";
        IconImage.sprite = item.Icon;

        RefreshButton();
        
        InteractionButton.onClick.AddListener(() => myManager.OnItemClicked(myItem));
    }

    public void RefreshButton()
    {
        PlayerInventory inv = LocalPlayerInventoryManager.LoadInventory();

        if (inv != null && inv.EquippedItemIDs[(int)myItem.Category] == myItem.ItemID)
            ButtonText.text = "Equipped";
        else if (inv.UnlockedItemIDs.Contains(myItem.ItemID))
            ButtonText.text = "Equip";
        else
            ButtonText.text = "Buy";
    }
}