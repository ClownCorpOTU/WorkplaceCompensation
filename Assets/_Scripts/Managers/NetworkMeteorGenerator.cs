using Fusion;
using UnityEngine;
using UnityEngine.Serialization;

public class NetworkMeteorGenerator : NetworkBehaviour
{
    [SerializeField] private Transform targetPlane;
    
    [Header("Meteor Settings")]
    [SerializeField] private GameObject meteorPrefab;
    [SerializeField] private Vector2 timeBetweenSpawns = new Vector2(2f, 10f);
    [SerializeField] private float meteorBaseVelocity = 75f;
    [SerializeField] private Vector2 meteorVelocityMultiplierRange = new Vector2(0f, 0.6f);
    [SerializeField] private Vector2 meteorSizeMultiplierRange = new Vector2(0f, 0.6f);
    
    [Networked] private TickTimer spawnDelayTimer { get; set; }
    private GameObject[] allSpawnPoints;
    private int spawnPosMaxSize;

    public override void Spawned()
    {
        if (!Object.HasStateAuthority) return;
        
        allSpawnPoints = GameObject.FindGameObjectsWithTag("MeteorSpawnPoint");
        spawnPosMaxSize = allSpawnPoints.Length;
        
        spawnDelayTimer = TickTimer.CreateFromSeconds(Runner, Random.Range(timeBetweenSpawns.x, timeBetweenSpawns.y));
    }

    private Vector3 GetRandomPointOnPlane()
    {
        // Default unity plane is 10x10 units, so we calculate the distance from the center to edge based on scale
        float xRange = targetPlane.localScale.x;
        float zRange = targetPlane.localScale.z;

        float randomX = Random.Range(-xRange, xRange);
        float randomZ = Random.Range(-zRange, zRange);

        Vector3 localPoint = new Vector3(randomX, 0, randomZ);
        return targetPlane.TransformPoint(localPoint);
    }

    public override void FixedUpdateNetwork()
    {
        if (!Object.HasStateAuthority) return;
        
        //print(spawnDelayTimer.RemainingTime(Runner));
        //if (spawnDelayTimer.ExpiredOrNotRunning(Runner))
        if (Input.GetKeyDown(KeyCode.L))
            SpawnMeteor();
    }

    private void SpawnMeteor()
    {
        // Pick spawn point
        int rand = Random.Range(0, spawnPosMaxSize);
        var thisSpawnPos = allSpawnPoints[rand].transform.position;
        
        // Pick random target on plane
        Vector3 targetLandingPos = GetRandomPointOnPlane();

        // Spawn meteor
        var spawnedMeteor = Runner.Spawn(meteorPrefab, thisSpawnPos, Quaternion.identity);
        
        // Scale meteor
        var spawnedMeteorObj = spawnedMeteor.gameObject.transform;
        var meteorOriginalScale = spawnedMeteorObj.localScale;
        var scaleMultiplier = Random.Range(meteorSizeMultiplierRange.x, meteorSizeMultiplierRange.y);
        
        spawnedMeteorObj.localScale = new Vector3(
            meteorOriginalScale.x * scaleMultiplier,
            meteorOriginalScale.y * scaleMultiplier,
            meteorOriginalScale.z * scaleMultiplier
        );
        
        // Calculate direction and speed
        Vector3 direction = (targetLandingPos - thisSpawnPos).normalized;
        float finalSpeed = meteorBaseVelocity * (Random.Range(meteorVelocityMultiplierRange.x, meteorVelocityMultiplierRange.y));
        
        if (spawnedMeteorObj.TryGetComponent(out NetworkMeteor nM))
        {
            nM.InitializeMeteor(direction * finalSpeed, targetLandingPos);
        }

        // Reset timer
        spawnDelayTimer = TickTimer.CreateFromSeconds(Runner, Random.Range(timeBetweenSpawns.x, timeBetweenSpawns.y));
    }
}