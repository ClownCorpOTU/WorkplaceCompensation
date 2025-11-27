using System.Linq;
using System.Collections.Generic;
using Fusion;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class NetworkGameManager : NetworkBehaviour
{
    [SerializeField, Tooltip("Game runtime in minutes")] private float gameRuntime = 5f;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private Image timerClockFill;
    [SerializeField] private Image timerClockHand;
    [SerializeField] private GameObject gameOverPanel;
    public TextMeshProUGUI ScoreText;
    [SerializeField] private TextMeshProUGUI finalScoreText;
    
    [Networked] private TickTimer GameTimer { get; set; }
    [Networked] private bool IsGameOver { get; set; }

    private Dictionary<PlayerRef, int> playerScores = new();
    
    
    #region Spawning and Setup
    
    public override void Spawned()
    {
        gameOverPanel.SetActive(false);
        
        if (Object.HasStateAuthority)
        {
            // Start a timer based on the runtime
            float gameTime = gameRuntime * 60f;
            GameTimer = TickTimer.CreateFromSeconds(Runner, gameTime);
        }
    }
    
    private NetworkPlayer FindPlayerByRef(PlayerRef playerRef)
    {
        foreach (var player in FindObjectsByType<NetworkPlayer>(FindObjectsSortMode.None))
        {
            if (player.Object.InputAuthority == playerRef)
                return player;
        }
        return null;
    }
    
    #endregion
    
    #region Updates
    
    private void Update()
    {
        // UI updates are client-only (not networked)
        if (GameTimer.IsRunning)
        {
            float remainingTime = GameTimer.RemainingTime(Runner).GetValueOrDefault();
            UpdateTimerUI(remainingTime);
        }
    }
    
    public override void FixedUpdateNetwork()
    {
        if (!GameTimer.IsRunning || IsGameOver) return;

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
        timerText.text = $"{minutes:00}:{seconds:00}";
        
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

        // Find that player's object
        NetworkPlayer player = FindPlayerByRef(playerRef);

        if (player != null)
            player.RPC_UpdateScoreUI(playerScores[playerRef]);
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

            sb.AppendLine($"<color={colorTag}>Player {playerNumber}: {score}</color>");
            rank++;
        }

        // Send an RPC to clients to set final score
        string finalLeaderboard = sb.ToString();
        RPC_DisplayFinalScore(finalLeaderboard);
    }

    #endregion

    #region RPCs
    
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