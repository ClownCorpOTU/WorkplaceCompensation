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
    
    private List<RecipeSO> recipes;
    private List<VialType> currentInputs = new();
    private Queue<VialType> pendingResults = new(); // queue for multiple results
    [Networked] private TickTimer spawnDelayTimer { get; set; }

    private void Start()
    {
        recipes = recipeContainerSO.Recipes;
    }

    private void AddBox(Vial vial)
    {
        if (!Object.HasStateAuthority) return;
        if (vial.Type == VialType.OutputBox || vial.Type == VialType.TrashBag) return;

        currentInputs.Add(vial.Type);
        Utils.DebugLog($"Added vial: {vial.Type}");
        Runner.Despawn(vial.Object);

        if (currentInputs.Count >= 2)
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
    }

    public override void FixedUpdateNetwork()
    {
        if (!Object.HasStateAuthority) return;

        // Spawn next vial when timer expires
        if (spawnDelayTimer.Expired(Runner) && pendingResults.Count > 0)
        {
            var nextResult = pendingResults.Dequeue();
            SpawnResult(nextResult);

            // Restart timer if more results remain
            if (pendingResults.Count > 0)
                spawnDelayTimer = TickTimer.CreateFromSeconds(Runner, spawnDelay);
        }
    }

    public void OnChildTriggerEnter(Collider other)
    {
        if (!Object.HasStateAuthority) return;

        if (other.TryGetComponent(out Vial vial))
            AddBox(vial);
    }

    public void OnChildTriggerExit(Collider other)
    {
        // Not needed right now
    }
}
