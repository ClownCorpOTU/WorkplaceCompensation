/*
using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class RadiationZone : MonoBehaviour
{
    [SerializeField] private float radiationStartDistance = 15.0f;
    [SerializeField] private float radiationDeathDistance = 5.0f;
    [SerializeField] private float radiationDeathDelay = 3.0f;
    [SerializeField] private Material radiationScreenMat;
    [SerializeField] private Vector2 tickPitchRange = new Vector2(1.0f, 1.2f);
    [SerializeField] private Vector2 tickDelayRange = new Vector2(0.6f, 0.01f);
    [SerializeField] private Vector2 nextTickDelay = new Vector2(0.1f, 0.6f);
    
    private Transform playerTransform;
    private NetworkPlayer networkPlayer;
    private AudioSource tickSource;
    private AudioLowPassFilter lowPassFilter;
    private float nextTickTime;
    private float currentDeathTimer;
    
    private void Start()
    {
        GetReferences();
    }

    private void GetReferences()
    {
        playerTransform = GameObject.FindWithTag("Player").transform;
        networkPlayer = playerTransform.GetComponent<NetworkPlayer>();
        tickSource = GetComponent<AudioSource>();
        lowPassFilter = GetComponent<AudioLowPassFilter>();
    }
    
    private void Update()
    {
        if (playerTransform == null || networkPlayer == null || tickSource == null)
        {
            GetReferences();
        }

        float dist = Vector3.Distance(transform.position, playerTransform.position);
        float intensity = 1.0f - Mathf.Clamp01(dist / radiationStartDistance);
        
        radiationScreenMat.SetFloat("_RadiationIntensity", intensity);
        
        // Sound
        HandleGeigierAudio(intensity);
        
        // Kill player if they are too close
        HandleRadiationDeath(intensity, dist);
    }

    private void HandleGeigierAudio(float intensity)
    {
        if (intensity > 0 && Time.time >= nextTickTime)
        {
            lowPassFilter.cutoffFrequency = !networkPlayer.IsActiveRagdoll ? 500f : 22000f;
            
            tickSource.pitch = Random.Range(tickPitchRange.x, tickPitchRange.y);
            tickSource.volume = intensity;
            tickSource.PlayOneShot(tickSource.clip);

            float delay = Mathf.Lerp(tickDelayRange.x, tickDelayRange.y, intensity); // The closer you get, the faster the sound
            nextTickTime = Time.time + (delay * Random.Range(nextTickDelay.x, nextTickDelay.y)); // Random jitter to make it feel more real
        }
    }

    private void HandleRadiationDeath(float intensity, float dist)
    {
        if (intensity > 0 && dist < radiationDeathDistance)
        {
            currentDeathTimer += Time.deltaTime;

            if (currentDeathTimer >= radiationDeathDelay)
            {
                if (networkPlayer != null && networkPlayer.IsActiveRagdoll)
                {
                    networkPlayer.CreateRespawnTimer();
                    networkPlayer.MakeRagdoll();
                    AudioManager.instance.Play("Death", networkPlayer.transform.position);

                    currentDeathTimer = 0f;
                }
            }
        }
        else
        {
            currentDeathTimer = 0f;
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Radiation zone
        Gizmos.color = new Color(0, 1, 0, 0.75F);
        Gizmos.DrawSphere(transform.position, radiationStartDistance);
        
        // Death zone
        Gizmos.color = new Color(1, 0, 0, 0.75F);
        Gizmos.DrawSphere(transform.position, radiationDeathDistance);
    }
}
*/

using System.Collections.Generic; // For HashSet
using UnityEngine;
using Fusion;
using Random = UnityEngine.Random;

public class RadiationZone : NetworkBehaviour
{
    [SerializeField] private float radiationStartDistance = 15.0f;
    [SerializeField] private float radiationDeathDistance = 5.0f;
    [SerializeField] private float radiationDeathDelay = 3.0f;
    [SerializeField] private Material radiationScreenMat;
    [SerializeField] private Vector2 tickPitchRange = new Vector2(1.0f, 1.2f);
    [SerializeField] private Vector2 tickDelayRange = new Vector2(0.6f, 0.01f);
    [SerializeField] private Vector2 nextTickDelay = new Vector2(0.1f, 0.6f);
    
    private AudioSource tickSource;
    private AudioLowPassFilter lowPassFilter;

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
        radiationScreenMat.SetFloat("_RadiationIntensity", intensity);

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