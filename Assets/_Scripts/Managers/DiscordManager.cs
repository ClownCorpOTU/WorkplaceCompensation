using System;
using UnityEngine;
using Discord;

public class DiscordManager : MonoBehaviour
{
    public static DiscordManager Instance;
    
    private Discord.Discord discord;
    private int currentPlayerCount;

    private void Awake()
    {
        if (Instance == null) {
            Instance = this;
        }
        else {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        discord = new Discord.Discord(1490781768733687809, (ulong)Discord.CreateFlags.NoRequireDiscord);
        ChangeActivity();
    }

    private void OnDisable()
    {
        if (discord != null)
            discord.Dispose();
    }

    public void UpdatePlayerCount(int newPlayerCount)
    {
        currentPlayerCount = newPlayerCount;
        ChangeActivity();
    }

    public void ChangeActivity()
    {
        var activityManager = discord.GetActivityManager();

        var activity = new Discord.Activity
        {
            State = "In-Game",
            Details = "Being productive!",
            Assets = {
                LargeImage = "gameheroimage", // Must match the Key in Art Assets
                LargeText = "Workplace Compensation"
            },
            Party = {
                Id = "workplace_comp_lobby", // Required to show player count
                Size = {
                    CurrentSize = currentPlayerCount,
                    MaxSize = 6
                }
            }
        };
        
        activityManager.UpdateActivity(activity, (res) =>
        {
            if (res == Discord.Result.Ok)
            {
                Debug.Log("Discord Activity updated successfully!");
            }
            else
            {
                Debug.LogError("Discord Activity failed: " + res);
            }
        });
    }

    private void Update()
    {
        if (discord != null)
            discord.RunCallbacks();
    }
}