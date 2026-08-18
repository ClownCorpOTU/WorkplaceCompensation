using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "WorkplaceComp/CustomizationItem", order = 0)]
public class CustomizationItemSO : ScriptableObject
{
    public int ItemID;
    public string ItemName;
    public int Cost;
    public ItemCategory Category;
    public Sprite Icon;
}