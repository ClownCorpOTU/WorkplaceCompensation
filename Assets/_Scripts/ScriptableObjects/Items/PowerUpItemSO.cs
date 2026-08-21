using UnityEngine;

public enum PowerUpType { Consumable, Deployable }

[CreateAssetMenu(fileName = "New PowerUp", menuName = "WorkplaceComp/PowerUpItem", order = 1)]
public class PowerUpItemSO : ScriptableObject
{
    public int ItemID;
    public string PowerUpName;
    public Sprite PowerUpIcon;
    public PowerUpType PowerUpType;
    public GameObject HeldPrefab;        // Visual model that appears in players hands
    public GameObject DeployablePrefab;  // Optional object that gets physically placed (like a landmine)
    
    public Vector3 HeldScale = new Vector3(1, 1, 1); // Some objects are way too big when spawned in hand
}