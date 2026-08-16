using System.Collections.Generic;
using Fusion;
using UnityEngine;

public class NetworkFossilManager : NetworkBehaviour
{
    [Networked, Capacity(8)] public NetworkArray<Vector3> ActiveFossilPositions { get; }
    private GameObject[] allSpawnPoints;

    public override void Spawned()
    {
        if (Object.HasStateAuthority)
        {
            allSpawnPoints = GameObject.FindGameObjectsWithTag("FossilSpawnPoint");
            SelectRandomFossils();
        }
    }

    private void SelectRandomFossils()
    {
        if (!Object.HasStateAuthority) return;

        List<int> indices = new List<int>();
        for (int i = 0; i < allSpawnPoints.Length; i++) indices.Add(i);

        for (int i = 0; i < 8; i++)
        {
            // Only fill if the slot is currently "empty" (Vector3.zero)
            if (ActiveFossilPositions[i] == Vector3.zero)
            {
                int randomIndex = Random.Range(0, indices.Count);
                int selectedPointIndex = indices[randomIndex];

                ActiveFossilPositions.Set(i, allSpawnPoints[selectedPointIndex].transform.position);

                // Remove to ensure no duplicates
                indices.RemoveAt(randomIndex);
            }
        }
    }

    public int GetClosestFossilIndex(Vector3 playerPos, out Vector3 position)
    {
        int closestIndex = -1;
        position = Vector3.zero;
        float minDistance = float.MaxValue;

        for (int i = 0; i < ActiveFossilPositions.Length; i++)
        {
            Vector3 pos = ActiveFossilPositions[i];
            if (pos == Vector3.zero) continue;

            float dist = Vector3.Distance(playerPos, pos);

            if (dist < minDistance)
            {
                minDistance = dist;
                closestIndex = i;
                position = pos;
            }
        }

        return closestIndex;
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_ClearFossil(int index)
    {
        if (index >= 0 && index < ActiveFossilPositions.Length)
        {
            ActiveFossilPositions.Set(index, Vector3.zero);
        }
        
        // Manual count of active fossils
        int activeCount = 0;
        foreach (Vector3 fossilPos in ActiveFossilPositions)
        {
            if (fossilPos != Vector3.zero) activeCount++;
        }

        // Trigger respawn if we drop below threshold
        if (activeCount < 3) SelectRandomFossils();
    }
}