
using System.Collections.Generic; // For HashSet
using UnityEngine;
using Fusion;
using Random = UnityEngine.Random;

public class RadiationZone : NetworkBehaviour
{
    [SerializeField] private float radiationStartDistance = 15.0f;
    [SerializeField] private float radiationDeathDistance = 5.0f;
    [SerializeField] private float radiationDeathDelay = 3.0f;
    [SerializeField] private Vector2 tickPitchRange = new Vector2(1.0f, 1.2f);
    [SerializeField] private Vector2 tickDelayRange = new Vector2(0.6f, 0.01f);
    [SerializeField] private Vector2 nextTickDelay = new Vector2(0.1f, 0.6f);
    
    private AudioSource tickSource;
    private AudioLowPassFilter lowPassFilter;
    private LocalPlayerUIManager localPlayerUIManager;

    // Track multiple players inside the zone
    private HashSet<NetworkPlayer> playersInZone = new HashSet<NetworkPlayer>();

    [Networked] private TickTimer nextTickTimer { get; set; }
    // Note: To track death timers for MULTIPLE players, we use a simple local float 
    // since the logic runs on the State Authority anyway.
    private Dictionary<NetworkPlayer, float> playerDeathTimers = new Dictionary<NetworkPlayer, float>();

    public override void Spawned()
    {
        tickSource = GetComponent<AudioSource>();
        lowPassFilter = GetComponent<AudioLowPassFilter>();
        localPlayerUIManager = FindFirstObjectByType<LocalPlayerUIManager>();
    }

    // Use Physics triggers to find players
    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<NetworkPlayer>(out var player))
        {
            if (!playersInZone.Contains(player))
                playersInZone.Add(player);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent<NetworkPlayer>(out var player))
        {
            localPlayerUIManager.UpdateRadiationFullScreenEffect(0f);
            playersInZone.Remove(player);
            if (playerDeathTimers.ContainsKey(player))
                playerDeathTimers.Remove(player);
        }
    }

    public override void FixedUpdateNetwork()
    {
        // Clean up any null references (players who disconnected while in zone)
        playersInZone.RemoveWhere(p => p == null);

        foreach (var player in playersInZone)
        {
            float dist = Vector3.Distance(transform.position, player.transform.position);
            float intensity = 1.0f - Mathf.Clamp01(dist / radiationStartDistance);

            // Only play audio/visuals for the LOCAL player (Input Authority)
            if (player.HasInputAuthority)
            {
                UpdateLocalEffects(intensity, player);
            }

            // Only process death on the State Authority (Server/Host)
            if (HasStateAuthority)
            {
                HandleRadiationDeath(player, intensity, dist);
            }
        }
    }

    private void UpdateLocalEffects(float intensity, NetworkPlayer player)
    {
        Debug.Log("Updating local effect!");
        localPlayerUIManager.UpdateRadiationFullScreenEffect(intensity);

        if (intensity > 0 && nextTickTimer.ExpiredOrNotRunning(Runner))
        {
            lowPassFilter.cutoffFrequency = !player.IsActiveRagdoll ? 500f : 22000f;
            tickSource.pitch = Random.Range(tickPitchRange.x, tickPitchRange.y);
            tickSource.volume = intensity;
            tickSource.PlayOneShot(tickSource.clip);

            float delay = Mathf.Lerp(tickDelayRange.x, tickDelayRange.y, intensity);
            nextTickTimer = TickTimer.CreateFromSeconds(Runner, delay * Random.Range(nextTickDelay.x, nextTickDelay.y));
        }
    }

    private void HandleRadiationDeath(NetworkPlayer player, float intensity, float dist)
    {
        if (intensity > 0 && dist < radiationDeathDistance)
        {
            if (!playerDeathTimers.ContainsKey(player))
                playerDeathTimers[player] = 0;

            playerDeathTimers[player] += Runner.DeltaTime;

            if (playerDeathTimers[player] >= radiationDeathDelay)
            {
                if (player.IsActiveRagdoll)
                {
                    player.CreateRespawnTimer();
                    player.MakeRagdoll();
                    AudioManager.instance.Play("Death", player.transform.position);
                    playerDeathTimers[player] = 0f;
                }
            }
        }
        else if (playerDeathTimers.ContainsKey(player))
        {
            playerDeathTimers[player] = 0f;
        }
    }
}

/*
using UnityEngine;

public class RadiationZoneLocal : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float radiationStartDistance = 15.0f; // Ensure your Trigger Radius matches this!
    [SerializeField] private float radiationDeathDistance = 5.0f;
    [SerializeField] private float radiationDeathDelay = 3.0f;

    [Header("Audio Settings")]
    [SerializeField] private Vector2 tickPitchRange = new Vector2(1.0f, 1.2f);
    [SerializeField] private Vector2 tickDelayRange = new Vector2(0.6f, 0.01f);

    private AudioSource tickSource;
    private AudioLowPassFilter lowPassFilter;
    private LocalPlayerUIManager localPlayerUIManager;

    private float localTickTimer;
    private float deathTimer;
    private bool isPlayerInside;
    private NetworkPlayer localPlayerCache;

    // Static counter to track if the player is in ANY radiation zone
    private static int barrelsActiveCount = 0;

    private void Start()
    {
        tickSource = GetComponent<AudioSource>();
        lowPassFilter = GetComponent<AudioLowPassFilter>();
        localPlayerUIManager = Object.FindFirstObjectByType<LocalPlayerUIManager>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Only care if it's the LOCAL player
            if (localPlayerCache == null) FindLocalPlayer();
            
            if (other.gameObject == localPlayerCache.gameObject)
            {
                isPlayerInside = true;
                barrelsActiveCount++;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (localPlayerCache != null && other.gameObject == localPlayerCache.gameObject)
        {
            isPlayerInside = false;
            barrelsActiveCount--;
            deathTimer = 0f;

            // Only clear UI if this was the LAST barrel the player was near
            if (barrelsActiveCount <= 0)
            {
                barrelsActiveCount = 0; // Safety reset
                if (localPlayerUIManager) localPlayerUIManager.UpdateRadiationFullScreenEffect(0f);
            }
        }
    }

    private void Update()
    {
        if (!isPlayerInside || localPlayerCache == null) return;

        float dist = Vector3.Distance(transform.position, localPlayerCache.transform.position);

        // Calculate intensity based on distance (within the trigger)
        float intensity = 1.0f - Mathf.Clamp01(dist / radiationStartDistance);
        
        UpdateLocalEffects(intensity);

        if (dist < radiationDeathDistance)
        {
            HandleDeathTimer();
        }
        else
        {
            deathTimer = 0f;
        }
    }

    private void FindLocalPlayer()
    {
        var players = Object.FindObjectsByType<NetworkPlayer>(FindObjectsSortMode.None);
        foreach (var p in players)
        {
            if (p.HasInputAuthority)
            {
                localPlayerCache = p;
                break;
            }
        }
    }

    private void UpdateLocalEffects(float intensity)
    {
        if (localPlayerUIManager) localPlayerUIManager.UpdateRadiationFullScreenEffect(intensity);

        localTickTimer -= Time.deltaTime;
        if (localTickTimer <= 0 && intensity > 0)
        {
            // Muffle sound if the player is a ragdoll (Adjusted logic from your snippet)
            if (lowPassFilter)
                lowPassFilter.cutoffFrequency = localPlayerCache.IsActiveRagdoll ? 22000f : 500f;
            
            tickSource.pitch = Random.Range(tickPitchRange.x, tickPitchRange.y);
            tickSource.volume = intensity;
            tickSource.PlayOneShot(tickSource.clip);

            localTickTimer = Mathf.Lerp(tickDelayRange.x, tickDelayRange.y, intensity);
        }
    }

    private void HandleDeathTimer()
    {
        // Don't process death if already a ragdoll
        if (!localPlayerCache.IsActiveRagdoll) return;

        deathTimer += Time.deltaTime;

        if (deathTimer >= radiationDeathDelay)
        {
            localPlayerCache.MakeRagdoll(); 
            deathTimer = 0f;
        }
    }
}
*/