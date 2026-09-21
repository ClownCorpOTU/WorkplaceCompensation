using System.Collections.Generic;

[System.Serializable]
public class PlayerInventory
{
    public List<int> UnlockedItemIDs = new List<int>();
    public int[] EquippedItemIDs = new int[3]; // 0 - Hat; 1 - Suit; 2 - Boots
}