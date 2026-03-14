using Fusion;
using UnityEngine;

public class NetworkBoulderGenerator : NetworkBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private Transform[] spawnPos;

    [Header("Boulder Settings")]
    [SerializeField] private GameObject boulderPrefab;
    [SerializeField] private Vector2 timeBetweenSpawns = new Vector2(2f, 10f);
    [SerializeField] private Vector2 boulderSizeMultiplierRange = new Vector2(0f, 0.6f);
    
    [Networked] private TickTimer spawnDelayTimer { get; set; }
    private int spawnPosMaxSize;

    public override void Spawned()
    {
        spawnPosMaxSize = spawnPos.Length;
        
        spawnDelayTimer = TickTimer.CreateFromSeconds(Runner, Random.Range(timeBetweenSpawns.x, timeBetweenSpawns.y));
    }

    public override void FixedUpdateNetwork()
    {
        if (!Object.HasStateAuthority) return;
        
        //print(spawnDelayTimer.RemainingTime(Runner));
        if (spawnDelayTimer.ExpiredOrNotRunning(Runner))
        {
            SpawnBoulder();
        }
    }

    private void SpawnBoulder()
    {
        int rand = Random.Range(0, spawnPosMaxSize);
        var thisPos = spawnPos[rand].position;

        // Spawn boulder
        var spawnedBoulder = Runner.Spawn(boulderPrefab, thisPos, Quaternion.Euler(thisPos.x, thisPos.y, thisPos.z));

        // Scale boulder
        var spawnedBoulderObj = spawnedBoulder.gameObject.transform;
        var boulderOriginalScale = spawnedBoulderObj.localScale;
        var scaleMultiplier = Random.Range(boulderSizeMultiplierRange.x, boulderSizeMultiplierRange.y);
        
        // Randomly choose between a postive or negative scale
        if (Random.Range(0, 2) == 0)
        {
            scaleMultiplier *= -1f;
        }
        
        spawnedBoulderObj.localScale = new Vector3(
            boulderOriginalScale.x * scaleMultiplier,
            boulderOriginalScale.y * scaleMultiplier,
            boulderOriginalScale.z * scaleMultiplier
            );
        
        // Change weight according to scale
        //var boulderRB = spawnedBoulderObj.GetComponent<Rigidbody>();
        //boulderRB.mass *= scaleMultiplier;

        // Reset timer
        spawnDelayTimer = TickTimer.CreateFromSeconds(Runner, Random.Range(timeBetweenSpawns.x, timeBetweenSpawns.y));
    }
}