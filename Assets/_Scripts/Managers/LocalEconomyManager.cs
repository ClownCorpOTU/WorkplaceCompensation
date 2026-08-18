using UnityEngine;

public static class LocalEconomyManager
{
    public static int GetCoins()
    {
        string coinsKey = GetCoinKey();
        return PlayerPrefs.GetInt(coinsKey, 0);
    }

    public static void AddCoins(int amount)
    {
        string coinsKey = GetCoinKey();
        int currentScore =  PlayerPrefs.GetInt(coinsKey, 0);
        PlayerPrefs.SetInt(coinsKey, currentScore + amount);
    }

    private static string GetCoinKey()
    {
        string key = "PlayerCoins";
        
        #if UNITY_EDITOR
        if (ParrelSync.ClonesManager.IsClone())
            key += "_clone";
        #endif

        return key;
    } 
}