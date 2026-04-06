using System.Collections.Generic;
using Fusion;
using UnityEngine;

public class NetworkLandmineManager : NetworkBehaviour
{
    [Header("Settings")]
    [SerializeField] private NetworkPrefabRef landminePrefab;
    [SerializeField] private Transform landmineParent; 
    [SerializeField] private int maxLandmines = 8;
    [SerializeField] private int minLandmines = 3;
    [SerializeField] private LayerMask groundLayer;

    // Track active landmine objects
    private List<NetworkObject> _activeLandmines = new List<NetworkObject>();
    private GameObject[] _allSpawnPoints;

    public override void Spawned()
    {
        // Only the State Authority (Host/Server) handles spawning logic
        if (Object.HasStateAuthority)
        {
            _allSpawnPoints = GameObject.FindGameObjectsWithTag("LandmineSpawnPoint");
            RefreshLandmineCount();
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (!Object.HasStateAuthority) return;

        // Clean up the list by removing landmines that have exploded (despawned)
        _activeLandmines.RemoveAll(item => item == null || !item.IsValid);

        // If we drop below the minimum threshold, spawn back up to max
        if (_activeLandmines.Count < minLandmines)
        {
            RefreshLandmineCount();
        }
    }

    private void RefreshLandmineCount()
    {
        int amountToSpawn = maxLandmines - _activeLandmines.Count;
        
        // Shuffle spawn points to ensure randomness
        List<GameObject> availablePoints = new List<GameObject>(_allSpawnPoints);
        ShuffleList(availablePoints);

        int spawnedThisPass = 0;
        foreach (var point in availablePoints)
        {
            if (spawnedThisPass >= amountToSpawn) break;

            // Optional: Check if a landmine is already too close to this spawn point
            if (IsPointOccupied(point.transform.position)) continue;

            SpawnLandmineAtPoint(point.transform);
            spawnedThisPass++;
        }
    }

    private void SpawnLandmineAtPoint(Transform spawnPoint)
    {
        // 1. Determine terrain height and rotation using a Raycast
        // We cast from 1 unit above the spawn point downwards
        Vector3 rayStart = spawnPoint.position + Vector3.up;
        if (Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, 5f, groundLayer))
        {
            Vector3 spawnPos = hit.point;
            
            // 2. Align rotation to the terrain normal
            // Vector3.up is the default 'top' of the landmine; hit.normal is the 'top' of the ground
            Quaternion spawnRot = Quaternion.FromToRotation(Vector3.up, hit.normal);

            // 3. Spawn on the network
            NetworkObject newMine = Runner.Spawn(landminePrefab, spawnPos, spawnRot, Object.InputAuthority);
            newMine.transform.SetParent(landmineParent);
            _activeLandmines.Add(newMine);
        }
    }

    private bool IsPointOccupied(Vector3 position)
    {
        foreach (var mine in _activeLandmines)
        {
            if (mine != null && Vector3.Distance(mine.transform.position, position) < 1f)
                return true;
        }
        return false;
    }

    private void ShuffleList<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            T temp = list[i];
            int randomIndex = Random.Range(i, list.Count);
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }
}