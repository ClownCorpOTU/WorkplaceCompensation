using Fusion;
using UnityEngine;

public class NetworkBoulderGenerator : NetworkBehaviour
{
    [SerializeField] private Transform[] spawnPos;
    [SerializeField] private GameObject boulderPrefab;
    [SerializeField] private Vector2 timeBetweenSpawns = new Vector2(2f, 10f);
    
    [Networked] private TickTimer spawnDelayTimer { get; set; }
    
    private int spawnPosMaxSize;

    public override void Spawned()
    {
        spawnPosMaxSize = spawnPos.Length;
        
        spawnDelayTimer = TickTimer.CreateFromSeconds(Runner, Random.Range(timeBetweenSpawns.x, timeBetweenSpawns.y));
    }

    public override void FixedUpdateNetwork()
    {
        print(spawnDelayTimer.RemainingTime(Runner));
        
        if (spawnDelayTimer.ExpiredOrNotRunning(Runner))
        {
            print("Timer ran out!");
            SpawnBoulder();
        }
    }

    private void SpawnBoulder()
    {
        int rand = Random.Range(0, spawnPosMaxSize);
        var thisPos = spawnPos[rand].position;

        var spawnedBoulder = Runner.Spawn(boulderPrefab, thisPos, Quaternion.Euler(thisPos.x, thisPos.y, thisPos.z));
        
        spawnDelayTimer = TickTimer.CreateFromSeconds(Runner, Random.Range(timeBetweenSpawns.x, timeBetweenSpawns.y));
    }
}