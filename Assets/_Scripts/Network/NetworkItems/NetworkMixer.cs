using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Fusion;
using UnityEngine;
using Random = UnityEngine.Random;

public class NetworkMixer : NetworkBehaviour, ITriggerReceiver
{
    [SerializeField] private MasterRecipeContainerSO recipeContainerSO;
    [SerializeField] private Vial vialPrefab;
    [SerializeField] private Vial trashPrefab;
    [SerializeField] private Transform vialSpawnPoint;
    [SerializeField] private float spawnDelay = 0.5f;
    [SerializeField] private GameObject model;

    [Header("Lighting parameters")]
    [SerializeField] private Renderer light1;
    [SerializeField] private Renderer light2;
    [SerializeField] private Material redMat, greenMat;
    [SerializeField] private float greenDuration = 2f; // not used for now but kept for later

    [Header("Juice")]
    [SerializeField] private GameObject fireworksPrefab;
    [SerializeField] private Transform fireworkSpawnPoint;
    [SerializeField] private float fxDespawnDelay = 15f;
    
    [Header("New Mixer")]
    [SerializeField] private GameObject leftVial;
    [SerializeField] private GameObject rightVial;

    private List<RecipeSO> recipes;
    private List<VialType> currentInputs = new();
    private Queue<VialType> pendingResults = new();
    private int vialCount;
    private Vector3 originalPos;

    // --- Network timers ---
    [Networked] private TickTimer spawnDelayTimer { get; set; }
    [Networked] private bool lightsAreGreen { get; set; }

    private AudioManager audioManager;

    public override void Spawned()
    {
        recipes = recipeContainerSO.Recipes;
        audioManager = FindFirstObjectByType<AudioManager>();

        if (Object.HasStateAuthority)
            RPC_ResetMixerVisuals();
    }

    private void AddBox(Vial vial)
    {
        if (!Object.HasStateAuthority) return;
        if (vial.Type != VialType.OutputVial) return;

        currentInputs.Add(vial.Type);

        Runner.Despawn(vial.Object);

        if (recipes == null || recipes.Count == 0) return;

        var sortedInputs = currentInputs.OrderBy(x => x).ToList();

        var matchingRecipe = recipes.FirstOrDefault(r =>
            r != null &&
            r.Ingredients != null &&
            r.Ingredients.Count == sortedInputs.Count &&
            r.Ingredients.OrderBy(i => i).SequenceEqual(sortedInputs)
        );

        if (matchingRecipe != null)
            Mix();
    }

    private void Mix()
    {
        RPC_Play("Processor", transform.position);

        var sortedInput = currentInputs.OrderBy(x => x).ToList();
        var matchingRecipe = recipes.FirstOrDefault(r =>
            r.Ingredients.OrderBy(i => i).SequenceEqual(sortedInput)
        );

        if (matchingRecipe != null)
        {
            foreach (var r in matchingRecipe.Results)
                pendingResults.Enqueue(r);
        }
        else
        {
            pendingResults.Enqueue(VialType.TrashBag);
        }

        currentInputs.Clear();
        if (Object.HasStateAuthority)
            RPC_SetLightsGreen();

        if (!spawnDelayTimer.IsRunning)
            spawnDelayTimer = TickTimer.CreateFromSeconds(Runner, spawnDelay);
    }

    private void SpawnResult(VialType resultType)
    {
        Vial newVial = (resultType == VialType.TrashBag)
            ? Runner.Spawn(trashPrefab, vialSpawnPoint.position, Quaternion.identity)
            : Runner.Spawn(vialPrefab, vialSpawnPoint.position, Quaternion.identity);

        newVial.Initialize(resultType);
        vialCount++;
        
        RPC_PlayFireworks();
    }

    public override void FixedUpdateNetwork()
    {
        if (!Object.HasStateAuthority) return;

        if (spawnDelayTimer.Expired(Runner) && pendingResults.Count > 0)
        {
            var nextResult = pendingResults.Dequeue();
            SpawnResult(nextResult);

            if (pendingResults.Count > 0)
            {
                spawnDelayTimer = TickTimer.CreateFromSeconds(Runner, spawnDelay);
            }
            else
            {
                if (Object.HasStateAuthority)
                    RPC_ResetMixerVisuals();

                RPC_PlayFireworks();
            }
        }
    }

    // --- RPC Helpers ---

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_SetLightsGreen()
    {
        lightsAreGreen = true;
        if (light1 != null) light1.material = greenMat;
        if (light2 != null) light2.material = greenMat;
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_SetSingleLightAndVial(bool leftSide, bool active)
    {
        // Update vial visibility
        if (leftSide)
        {
            if (leftVial != null) leftVial.SetActive(active);
            if (light1 != null) light1.material = active ? greenMat : redMat;
        }
        else
        {
            if (rightVial != null) rightVial.SetActive(active);
            if (light2 != null) light2.material = active ? greenMat : redMat;
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_ResetMixerVisuals()
    {
        lightsAreGreen = false;

        if (light1 != null) light1.material = redMat;
        if (light2 != null) light2.material = redMat;

        if (leftVial != null) leftVial.SetActive(false);
        if (rightVial != null) rightVial.SetActive(false);

        vialCount = 0;
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_PlayFireworks()
    {
        if (fireworksPrefab == null || fireworkSpawnPoint == null) return;

        GameObject fx = Instantiate(fireworksPrefab, fireworkSpawnPoint.position, Quaternion.Euler(-90f, 0f, 0f));
        audioManager.Play("FireworksExplosion", transform.position);
        audioManager.Play("FireworksHighPitch", transform.position);

        if (fx != null) Destroy(fx, fxDespawnDelay);
    }
    
    [Rpc(RpcSources.StateAuthority, RpcTargets.All, TickAligned = false)]
    private void RPC_Play(string audioName, Vector3 position)
    {
        if (audioManager != null) audioManager.Play(audioName, position);
    }

    // --- Trigger Interface ---
    public void OnChildTriggerEnter(Collider other, TriggerType tType = TriggerType.Left)
    {
        if (!Object.HasStateAuthority) return;
        if (!other.TryGetComponent(out Vial vial)) return;
        if (vial.Type != VialType.OutputVial) return;

        int countBeforeAdd = currentInputs.Count;
        AddBox(vial);
        Runner.Despawn(vial.Object);

        if (countBeforeAdd >= 1)
        {
            print("Here!");
            RPC_SetSingleLightAndVial(true, true);
            RPC_SetSingleLightAndVial(false, true);
        }
        else
        {
            switch (tType)
            {
                case TriggerType.Left:
                    RPC_SetSingleLightAndVial(true, true);
                    break;
                case TriggerType.Right:
                    RPC_SetSingleLightAndVial(false, true);
                    break;
            }
        }
    }

    public void OnChildTriggerExit(Collider other, TriggerType tType = TriggerType.Left)
    {
        // Optional: turn off side when vial leaves
        // if (Object.HasStateAuthority)
        // {
        //     bool left = (tType == TriggerType.Left);
        //     RPC_SetSingleLightAndVial(left, false);
        // }
    }
}
