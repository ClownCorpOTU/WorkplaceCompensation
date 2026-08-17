using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using Discord;

public class DiscordManager : MonoBehaviour
{
    public static DiscordManager Instance;
    [SerializeField] private string menuSceneName, level1SceneName, level2SceneName;
    
    private Discord.Discord discord;
    private int currentPlayerCount;
    private long sessionStartTime;
    private long? currentLevelEndTime; // Nullable: only used for countdowns

    private void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); return; }
    }

    private void Start()
    {
        discord = new Discord.Discord(1490781768733687809, (ulong)Discord.CreateFlags.NoRequireDiscord);
        sessionStartTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        SceneManager.sceneLoaded += (scene, mode) => UpdatePresence();
        UpdatePresence();
    }

    // Call this from NetworkGameManager when the match actually starts
    public void StartLevelTimer(float secondsRemaining)
    {
        currentLevelEndTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds() + (long)secondsRemaining;
        UpdatePresence();
    }

    public void UpdatePlayerCount(int newPlayerCount)
    {
        currentPlayerCount = newPlayerCount;
        UpdatePresence();
    }

    public void UpdatePresence()
    {
        var activityManager = discord.GetActivityManager();
        string sceneName = SceneManager.GetActiveScene().name;

        // Default Values (Lobby/Menus)
        string state = "In-Game";
        string details = "Being productive!";
        string imageKey = "gameheroimage"; 

        // Level-Specific Mapping
        if (sceneName.Equals(menuSceneName))
        {
            state = "In the Lobby";
            details = "Waiting for coworkers...";
            imageKey = "croppedhero";
            currentLevelEndTime = null; // Show elapsed time instead
        }
        else if (sceneName.Equals(level1SceneName))
        {
            state = "In the Lab";
            details = "Handling Very Safe Chemicals™";
            imageKey = "labicon";
        }
        else if (sceneName.Equals(level2SceneName))
        {
            state = "On Mars";
            details = "Reviving Ancient Beings";
            imageKey = "marsicon";
        }

        // Rich presence
        var activity = new Discord.Activity
        {
            State = state,
            Details = details,
            Assets = { LargeImage = imageKey, LargeText = "Workplace Compensation" },
            Party = {
                Id = "workplace_session",
                Size = { CurrentSize = Math.Max(1, currentPlayerCount), MaxSize = 6 }
            }
        };

        // Timer Logic
        if (currentLevelEndTime.HasValue)
        {
            activity.Timestamps.End = currentLevelEndTime.Value; // Shows "X:XX left"
        }
        else
        {
            activity.Timestamps.Start = sessionStartTime; // Shows "X:XX elapsed"
        }

        activityManager.UpdateActivity(activity, (res) => { });
    }

    private void Update() { if (discord != null) discord.RunCallbacks(); }
    private void OnDisable() { if (discord != null) discord.Dispose(); }
}