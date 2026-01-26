using UnityEngine;

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

                    currentDeathTimer = 0f;
                }
            }
        }
        else
        {
            currentDeathTimer = 0f;
        }
    }
}
