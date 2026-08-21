using UnityEngine;

public enum PowerUpType { Consumable, Deployable }

[CreateAssetMenu(fileName = "New PowerUp", menuName = "WorkplaceComp/PowerUpItem", order = 1)]
public class PowerupItemSO : ScriptableObject
{
    public int ItemID;
    public string PowerUpName;
    public Sprite PowerUpIcon;
    public PowerUpType PowerUpType;
    public GameObject HeldPrefab;        // Visual model that appears in players hands
    public GameObject DeployablePrefab;  // Optional object that gets physically placed (like a landmine)
}