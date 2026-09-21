using System;
using UnityEngine;
using UnityEngine.UI;

public class PowerUpInventoryUI : MonoBehaviour
{
    public static PowerUpInventoryUI Instance;

    [SerializeField] private Image[] slotImages;
    [SerializeField] private Color defaultColor = Color.white;
    [SerializeField] private Color selectedColor = Color.yellow;
    
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void RefreshUI(int[] currentItems, int selectedSlot, PowerUpItemSO[] database)
    {
        for (int i = 0; i < slotImages.Length; i++)
        {
            int itemID = currentItems[i];
            
            slotImages[i].color = (i == selectedSlot) ? selectedColor : defaultColor;

            if (itemID == 0)
            {
                slotImages[i].sprite = null;
                slotImages[i].enabled = true; 
            }
            else
            {
                foreach (var item in database)
                {
                    if (item.ItemID == itemID)
                    {
                        slotImages[i].sprite = item.PowerUpIcon;
                        break;
                    }
                }
            }
        }
    }
}