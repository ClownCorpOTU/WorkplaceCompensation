using UnityEngine;

public static class LocalEconomyManager
{
    public static int GetCoins()
    {
        string coinsKey = Utils.GetKey("PlayerCoins");
        return PlayerPrefs.GetInt(coinsKey, 0);
    }

    public static void AddCoins(int amount)
    {
        string coinsKey = Utils.GetKey("PlayerCoins");
        int currentScore =  PlayerPrefs.GetInt(coinsKey, 0);
        PlayerPrefs.SetInt(coinsKey, currentScore + amount);
    }
}