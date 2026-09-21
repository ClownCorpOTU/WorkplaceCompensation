using UnityEngine;

public static class LocalPlayerInventoryManager
{
    public static PlayerInventory LoadInventory()
    {
        var json = PlayerPrefs.GetString(Utils.GetKey("PlayerInventory"), "");

        if (string.IsNullOrEmpty(json))
            return new PlayerInventory();

        return JsonUtility.FromJson<PlayerInventory>(json);
    }

    public static void SaveInventory(PlayerInventory inventory)
    {
        string json = JsonUtility.ToJson(inventory);
        PlayerPrefs.SetString(Utils.GetKey("PlayerInventory"), json);
        PlayerPrefs.Save();
    }
}
