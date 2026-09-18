using UnityEngine;

[System.Serializable]
public struct ItemVisualMapping
{
    public CustomizationItemSO ItemData;
    public GameObject Model;
}

public class PlayerCustomizationVisuals : MonoBehaviour
{
    [SerializeField] ItemVisualMapping[] hatMappings;

    public void UpdateVisuals(int[] equippedIDs)
    {
        int equippedHatID = equippedIDs[0];
        
        // Turn off all hats first
        foreach (ItemVisualMapping mapping in hatMappings)
        {
            if (mapping.Model != null)
                mapping.Model.SetActive(false);
        }
        
        // Check if any of the hats are equipped, and turn it on
        foreach (ItemVisualMapping mapping in hatMappings)
        {
            if (mapping.ItemData != null && mapping.ItemData.ItemID == equippedHatID)
            {
                if (mapping.Model != null)
                    mapping.Model.SetActive(true);

                break;
            }
        }
    }
}