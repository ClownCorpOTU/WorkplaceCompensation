using System;
using System.Collections.Generic;
using System.Linq;
using Fusion;
using UnityEngine;

public class NetworkMixer : NetworkBehaviour, ITriggerReceiver
{
    [SerializeField] private MasterRecipeContainerSO recipeContainerSO;
    [SerializeField] private Vial vialPrefab;
    [SerializeField] private Vial trashPrefab;
    [SerializeField] private Transform vialSpawnPoint;
    [SerializeField] private float spawnDelay = 0.5f;

    [Header("Lighting parameters")] [SerializeField]
    private Renderer light1;

    [SerializeField] private Renderer light2;
    [SerializeField] private Material redMat, greenMat;
    [SerializeField] private float greenDuration = 2f; // not used for now but kept for later

    [Header("Juice")] [SerializeField] private GameObject fireworksPrefab;
    [SerializeField] private Transform fireworkSpawnPoint;
    [SerializeField] private float fxDespawnDelay = 15;
    
    [Header("New Mixer")]
    [SerializeField] private GameObject leftVial;
    [SerializeField] private GameObject rightVial;

    private List<RecipeSO> recipes;
    private List<VialType> currentInputs = new();
    private Queue<VialType> pendingResults = new(); // queue for multiple results
    private int vialCount;

    // --- Network timers ---
    [Networked] private TickTimer spawnDelayTimer { get; set; }
    [Networked] private bool lightsAreGreen { get; set; } // track current light state

    private AudioManager audioManager;

    private void Start()
    {
        recipes = recipeContainerSO.Recipes;
        audioManager = FindFirstObjectByType<AudioManager>();
        
        leftVial.SetActive(false);
        rightVial.SetActive(false);

        if (Object.HasStateAuthority) RPC_ResetLights();
    }

    private void AddBox(Vial vial)
    {
        if (!Object.HasStateAuthority) return;
        if (vial.Type == VialType.OutputBox || vial.Type == VialType.TrashBag) return;

        currentInputs.Add(vial.Type);
        Utils.DebugLog($"Added vial: {vial.Type}");

        Runner.Despawn(vial.Object);

        // --- Check if current inputs match any recipe exactly ---
        if (recipes == null || recipes.Count == 0) return;

        // Sort the current inputs to make comparison order-independent
        var sortedInputs = currentInputs.OrderBy(x => x).ToList();

        // Look for a recipe with the same ingredient count *and* same ingredients
        var matchingRecipe = recipes.FirstOrDefault(r =>
            r != null &&
            r.Ingredients != null &&
            r.Ingredients.Count == sortedInputs.Count &&
            r.Ingredients.OrderBy(i => i).SequenceEqual(sortedInputs)
        );

        // Only mix if a recipe fully matches
        if (matchingRecipe != null)
            Mix();
    }

    private void Mix()
    {
        AudioManager.instance.Play("Processor", transform.position);

        // Order inputs alphabetically
        var sortedInput = currentInputs.OrderBy(x => x).ToList();

        // Find matching recipe
        var matchingRecipe = recipes.FirstOrDefault(r =>
            r.Ingredients.OrderBy(i => i).SequenceEqual(sortedInput));
        

        // If we found a recipe, queue its results
        if (matchingRecipe != null)
        {
            foreach (var r in matchingRecipe.Results)
                pendingResults.Enqueue(r);
        }
        else
        {
            // No recipe matched — spawn a trash bag instead
            pendingResults.Enqueue(VialType.TrashBag);
        }

        currentInputs.Clear();

        // Keep both lights green while results are being processed
        if (Object.HasStateAuthority) RPC_SetLightsGreen();

        // Start timer for the first result
        if (!spawnDelayTimer.IsRunning)
            spawnDelayTimer = TickTimer.CreateFromSeconds(Runner, spawnDelay);
    }

    private void SpawnResult(VialType resultType)
    {
        Vial newVial;

        if (resultType == VialType.TrashBag)
            newVial = Runner.Spawn(trashPrefab, vialSpawnPoint.position, Quaternion.identity);
        else
            newVial = Runner.Spawn(vialPrefab, vialSpawnPoint.position, Quaternion.identity);

        newVial.Initialize(resultType);
        vialCount++;
    }

    private void OnBoxAdded()
    {
        // --- Turn on correct light based on input count ---
        if (currentInputs.Count == 1)
        {
            light1.material = greenMat;
        }
        else if (currentInputs.Count == 2)
        {
            light2.material = greenMat;
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (!Object.HasStateAuthority) return;

        // --- Handle vial spawning ---
        if (spawnDelayTimer.Expired(Runner) && pendingResults.Count > 0)
        {
            var nextResult = pendingResults.Dequeue();
            SpawnResult(nextResult);

            // Restart timer if more results remain
            if (pendingResults.Count > 0)
            {
                spawnDelayTimer = TickTimer.CreateFromSeconds(Runner, spawnDelay);
            }
            else
            {
                // --- All vials have spawned ---
                if (Object.HasStateAuthority) RPC_ResetLights();

                // Send an RPC so all players play fireworks
                RPC_PlayFireworks();
            }
        }
    }

    // --- Light helpers ---
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_SetLightsGreen()
    {
        lightsAreGreen = true;
        light1.material = greenMat;
        light2.material = greenMat;
    }
    
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_ResetLights()
    {
        lightsAreGreen = false;
        light1.material = redMat;
        light2.material = redMat;
        leftVial.SetActive(false);
        rightVial.SetActive(false);
        vialCount = 0;
    }

    // --- Firework RPCs ---
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_PlayFireworks()
    {
        if (fireworksPrefab == null || fireworkSpawnPoint == null) return;

        GameObject fx = Instantiate(fireworksPrefab, fireworkSpawnPoint.position, Quaternion.Euler(-90f, 0f, 0f));
        audioManager.Play("FireworksExplosion", transform.position);
        audioManager.Play("FireworksHighPitch", transform.position);

        // Auto-destroy if vfx didn't destory itself
        if (fx != null) Destroy(fx, fxDespawnDelay);
    }

    // --- Trigger interface ---
    public void OnChildTriggerEnter(Collider other, TriggerType tType=TriggerType.Left)
    {
        if (!Object.HasStateAuthority) return;
        if (!other.TryGetComponent(out Vial v)) return;

        switch (tType)
        {
            case TriggerType.Left:
                if (v != null)
                {
                    AddBox(v);
                    Runner.Despawn(v.Object);
                }

                RPC_SetVialsActive(true, false);
                
                break;
            case TriggerType.Right:
                if (v != null)
                {
                    AddBox(v);
                    Runner.Despawn(v.Object);
                }

                RPC_SetVialsActive(true, true);
                
                break;
        }
    }
    
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_SetVialsActive(bool leftVialActive, bool rightVialActive)
    {
        leftVial.SetActive(leftVialActive);
        light1.material = leftVialActive ? greenMat : redMat;
        rightVial.SetActive(rightVialActive);
        light2.material = rightVialActive ? greenMat : redMat;
    }

    public void OnChildTriggerExit(Collider other, TriggerType tType=TriggerType.Left)
    {
        // Not needed right now
    }
}