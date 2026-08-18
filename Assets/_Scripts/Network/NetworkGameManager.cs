using System.Linq;
using System.Collections.Generic;
using Fusion;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class NetworkGameManager : NetworkBehaviour
{
    [Header("Game Settings")]
    [SerializeField, Tooltip("Game runtime in minutes")] private float gameRuntime = 5f;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private Image timerClockFill;
    [SerializeField] private Image timerClockHand;
    [SerializeField] private GameObject gameOverPanel;
    
    [Header("Score UI")]
    public TextMeshProUGUI ScoreText;
    [SerializeField] private TextMeshProUGUI leaderboardText;
    [SerializeField] private TextMeshProUGUI finalScoreText;

    [Header("Medal UI")] [SerializeField] 
    private TextMeshProUGUI rankNumberText;
    [SerializeField] private Image localRankMedalImage;
    [SerializeField] private Sprite firstPlaceSprite;
    [SerializeField] private Sprite secondPlaceSprite;
    [SerializeField] private Sprite thirdPlaceSprite;
    [SerializeField] private Sprite noPlaceSprite;
    
    [Networked] private TickTimer GameTimer { get; set; }
    [Networked] private bool IsGameOver { get; set; }
    [Networked] private float remainingTime { get; set; }

    public float RemainingTime => remainingTime;

    public Dictionary<PlayerRef, NetworkPlayer> NetworkPlayers = new();
    private Dictionary<PlayerRef, int> playerScores = new();
    private int lastPlayerCount = 0;
    private float prevSeconds = 99999f;
    
    
    #region Spawning and Setup
    
    public override void Spawned()
    {
        gameOverPanel.SetActive(false);
        leaderboardText.text = "";
        
        if (Object.HasStateAuthority)
        {
            // Start a timer based on the runtime
            float gameTime = gameRuntime * 60f;
            GameTimer = TickTimer.CreateFromSeconds(Runner, gameTime);
            
            // Update Discord with the countdown!
            if (DiscordManager.Instance != null)
                DiscordManager.Instance.StartLevelTimer(gameTime);
        }
    }
    
    private NetworkPlayer FindPlayerByRef(PlayerRef playerRef)
    {
        return NetworkPlayers.GetValueOrDefault(playerRef);
    }
    
    #endregion
    
    #region Updates
    
    private void Update()
    {
        if (!Object || !Object.IsValid) return;
        
        // UI updates are client-only (not networked)
        if (GameTimer.IsRunning)
        {
            remainingTime = GameTimer.RemainingTime(Runner).GetValueOrDefault();
            UpdateTimerUI(remainingTime);
        }
    }
    
    public override void FixedUpdateNetwork()
    {
        if (!GameTimer.IsRunning || IsGameOver) return;
        
        // Refresh leaderboard if someone joins or leaves
        if (Object.HasStateAuthority && NetworkPlayers.Count != lastPlayerCount)
        {
            lastPlayerCount = NetworkPlayers.Count;
            UpdateLeaderboardData();
        }

        if (GameTimer.Expired(Runner))
        {
            IsGameOver = true;
            RPC_OnGameOver();
        }
    }

    private void UpdateTimerUI(float remainingTime)
    {
        int minutes = Mathf.FloorToInt(remainingTime / 60f);
        int seconds = Mathf.FloorToInt(remainingTime % 60f);
        
        if (prevSeconds != seconds) timerText.text = $"{minutes:00}:{seconds:00}";
        prevSeconds = seconds;
        
        timerClockFill.fillAmount = remainingTime / (gameRuntime*60f);
        timerClockHand.transform.localRotation = Quaternion.Euler(0, 0, Mathf.Lerp(360, 0, remainingTime / (gameRuntime*60f)));
    }
    
    #endregion

    #region Scoring
    
    public void AddScore(PlayerRef playerRef, int amount)
    {
        if (!Object.HasStateAuthority) return;

        if (!playerScores.ContainsKey(playerRef))
            playerScores[playerRef] = 0;

        playerScores[playerRef] += amount;

        // Update the specific player's personal score UI
        NetworkPlayer player = FindPlayerByRef(playerRef);
        if (player != null)
            player.RPC_UpdateScoreUI(playerScores[playerRef], amount);
        
        // Broadcast top scores to everyone's leaderbox
        UpdateLeaderboardData();
    }

    private void UpdateLeaderboardData()
    {
        // Sort EVERYONE to find everyone's true rank
        var allSorted = playerScores
            .OrderByDescending(kvp => kvp.Value)
            .ToList();
        
        // Prepare top 3 for the leaderboard
        PlayerRef[] topRefs = allSorted.Take(3).Select(x => x.Key).ToArray();
        int[] topScores = allSorted.Take(3).Select(x => x.Value).ToArray();
        
        // Find rank for every specific player
        foreach (var player in Runner.ActivePlayers)
        {
            int actualRank = allSorted.FindIndex(x => x.Key == player) + 1;

            if (actualRank <= 0) actualRank = Runner.ActivePlayers.Count();

            RPC_SyncPlayerRank(player, actualRank, topRefs, topScores);
        }
    }

    private void ShowPlayerScores()
    {
        // Build a complete list of all players with their scores (default 0 if not in dictionary)
        var allScores = new List<KeyValuePair<PlayerRef, int>>();
        foreach (var player in Runner.ActivePlayers)
        {
            int score = playerScores.ContainsKey(player) ? playerScores[player] : 0;
            allScores.Add(new KeyValuePair<PlayerRef, int>(player, score));
        }

        // Sort descending by score
        var sortedScores = allScores
            .OrderByDescending(kvp => kvp.Value)
            .ToList();

        // Build display text
        System.Text.StringBuilder sb = new System.Text.StringBuilder();

        int rank = 1;
        foreach (var kvp in sortedScores)
        {
            int playerNumber = kvp.Key.RawEncoded % 1000;
            int score = kvp.Value;

            string colorTag = rank switch
            {
                1 => "#FFD700", // Gold
                2 => "#FFA500", // Orange
                3 => "#CD7F32", // Bronze
                _ => "#FFFFFF"  // White for others
            };

            sb.AppendLine($"<color={colorTag}>Player {playerNumber-1}: {score}</color>");
            rank++;
        }

        // Send an RPC to clients to set final score
        string finalLeaderboard = sb.ToString();
        RPC_DisplayFinalScore(finalLeaderboard);
    }

    #endregion

    #region RPCs
    
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)] 
    private void RPC_SyncPlayerRank(PlayerRef targetPlayer, int myRank, PlayerRef[] topRefs, int[] topScores)
    {
        // Only execute logic for the player this message is intended for
        if (Runner.LocalPlayer != targetPlayer) return;

        // --- Update the Medal & Rank Number ---
        if (localRankMedalImage != null)
        {
            localRankMedalImage.sprite = myRank switch
            {
                1 => firstPlaceSprite,
                2 => secondPlaceSprite,
                3 => thirdPlaceSprite,
                _ => noPlaceSprite
            };

            rankNumberText.text = myRank.ToString();
        }

        // --- Update the "Others" Leaderboard Text ---
        UpdateLeaderboardText(topRefs, topScores);
    }
    
    private void UpdateLeaderboardText(PlayerRef[] topRefs, int[] topScores)
    {
        if (leaderboardText == null) return;
        
        PlayerRef localPlayer = Runner.LocalPlayer;

        // Single player edge case
        if (Runner.ActivePlayers.Count() <= 1)
        {
            leaderboardText.text = "<size=60%>WAITING FOR\nCHALLENGERS</size>";
            return;
        }

        List<string> displayEntries = new List<string>();
        
        // Who do we show?
        for (int i = 0; i < topRefs.Length; i++)
        {
            // Skip MYSELF - We only want to see others in this box
            if (topRefs[i] == localPlayer) continue;
            
            // Format player string
            int pNum = (topRefs[i].RawEncoded % 1000) - 1;
            displayEntries.Add($"<size=80%>P{pNum}:</size> {topScores[i]}");
            
            // We only have room for 2 "others"
            if (displayEntries.Count >=2) break;
        }
        
        // Final display
        leaderboardText.text = string.Join("\n", displayEntries);
    }
    
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_OnGameOver()
    {
        gameOverPanel.SetActive(true);
        ShowPlayerScores();
        NetworkPlayer.Local.RemovePlayerInputAuthority();
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
    
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_DisplayFinalScore(string scoresText)
    {
        // This runs on all clients (and host)
        finalScoreText.text = scoresText;
    }
    
    #endregion
    
    #region Menu Functions
    
    public void QuitGame()
    {
        Application.Quit();
    }
    
    #endregion
}