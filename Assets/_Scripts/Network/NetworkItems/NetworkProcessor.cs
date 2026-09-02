using System;
using System.Collections.Generic;
using System.Linq;
using Fusion;
using UnityEngine;

public class NetworkProcessor : NetworkBehaviour, ITriggerReceiver
{
    [SerializeField] private MasterRecipeContainerSO recipeContainerSO;
    [SerializeField] private Vial vialPrefab;
    [SerializeField] private Vial trashPrefab;
    [SerializeField] private Transform vialSpawnPoint;
    [SerializeField] private float spawnDelay = 0.5f;
    
    [Header("Juice")]
    [SerializeField] private GameObject fireworksPrefab;
    [SerializeField] private Transform fireworkSpawnPoint;
    [SerializeField] private float fxDespawnDelay = 15;
    
    [Header("Humming Effect")]
    [SerializeField] private Renderer machineRenderer;
    [SerializeField] private string fresnelParamName = "_FresnelPower";
    [SerializeField] private float minHumPower = 0.1f;
    [SerializeField] private float maxHumPower = 0.5f;
    [SerializeField] private float humSpeed = 15f; 

    private List<RecipeSO> recipes;
    private List<ObjectType> currentInputs = new();
    private Queue<ObjectType> pendingResults = new(); // queue for multiple results
    private NetworkGameManager networkGameManager;
    
    private float originalFresnelPower;
    private Material machineMaterial;
    private float targetHumPower;

    // --- Network timers ---
    [Networked] private TickTimer spawnDelayTimer { get; set; }
    [Networked] private NetworkBool isProcessing { get; set; }
    
    private AudioManager audioManager;
    private bool hasAddedBoxBefore;


    public override void Spawned()
    {
        recipes = recipeContainerSO.Recipes;
        audioManager = FindFirstObjectByType<AudioManager>();
        networkGameManager = FindFirstObjectByType<NetworkGameManager>();
        
        // Initialize the humming material
        if (machineRenderer != null)
        {
            machineMaterial = machineRenderer.material; 
            if (machineMaterial.HasFloat(fresnelParamName))
            {
                originalFresnelPower = machineMaterial.GetFloat(fresnelParamName);
                targetHumPower = originalFresnelPower;
            }
            else
            {
                Debug.LogWarning($"Material doesn't have parameter: {fresnelParamName}");
            }
        }
    }

    private void AddBox(Vial vial)
    {
        if (!Object.HasStateAuthority) return;

        if (vial.Type is not ObjectType.InputCrystal)
        {
            Utils.DebugLog($"Invalid vial type: {vial.Type}");
            return;
        }

        currentInputs.Add(vial.Type);
        Utils.DebugLog($"Added vial: {vial.Type}");

        //OnBoxAdded(vial.Type);
        
        // --- Give the correct player a score ---
        if (vial.TryGetComponent(out GrabbedByTracker grabbedByTracker))
        {
            networkGameManager.AddScore(grabbedByTracker.LastHeldBy, 1);
            RPC_TriggerTutorialEvent(grabbedByTracker.LastHeldBy, (int)GameEvent.VialsMixed);
        }

        // --- Despawn vial ---
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
        RPC_Play("Processor", transform.position);

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
            pendingResults.Enqueue(ObjectType.TrashBag);
        }

        currentInputs.Clear();
        isProcessing = true;

        // Start timer for the first result
        if (!spawnDelayTimer.IsRunning)
            spawnDelayTimer = TickTimer.CreateFromSeconds(Runner, spawnDelay);
    }

    private void SpawnResult(ObjectType resultType)
    {
        Vial newVial = Runner.Spawn(resultType == ObjectType.TrashBag ? trashPrefab : vialPrefab, vialSpawnPoint.position, Quaternion.identity);

        newVial.Initialize(resultType);
        RPC_PlayFireworks();
    }

    private void OnBoxAdded(ObjectType objectType)
    {
        if (!Object.HasStateAuthority) return;
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
                isProcessing = false;
                RPC_PlayFireworks();
                spawnDelayTimer = TickTimer.None; 
            }
        }
    }
    
    public override void Render()
    {
        if (machineMaterial == null) return;

        if (isProcessing) 
        {
            if (Mathf.Abs(machineMaterial.GetFloat(fresnelParamName) - targetHumPower) < 0.05f)
            {
                targetHumPower = UnityEngine.Random.Range(minHumPower, maxHumPower);
            }

            float currentPower = machineMaterial.GetFloat(fresnelParamName);
            float newPower = Mathf.Lerp(currentPower, targetHumPower, Time.deltaTime * humSpeed);
            machineMaterial.SetFloat(fresnelParamName, newPower);
        }
        else
        {
            float currentPower = machineMaterial.GetFloat(fresnelParamName);
            if (Mathf.Abs(currentPower - originalFresnelPower) > 0.001f)
            {
                float newPower = Mathf.Lerp(currentPower, originalFresnelPower, Time.deltaTime * (humSpeed / 2f));
                machineMaterial.SetFloat(fresnelParamName, newPower);
            }
        }
    }

    
    
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_TriggerTutorialEvent([RpcTarget] PlayerRef player, int eventEnumInt)
    {
        if (!hasAddedBoxBefore)
        {
            GameEventManager.TriggerEvent(GameEvent.BoxProcessed);
            hasAddedBoxBefore = true;
        }
    }

    
    // --- Firework RPCs ---
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_PlayFireworks()
    {
        if (fireworksPrefab == null || fireworkSpawnPoint == null) return;

        GameObject fx = Instantiate(fireworksPrefab, fireworkSpawnPoint.position, Quaternion.Euler(-90f, 0f, 0f));
        audioManager.Play("FireworksExplosion", transform.position);
        audioManager.Play("FireworksHighPitch", transform.position);

        // Auto-destroy if vfx didn't destroy itself
        if (fx != null) Destroy(fx, fxDespawnDelay);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All, TickAligned = false)]
    private void RPC_Play(string audioName, Vector3 position)
    {
        if (audioManager != null) audioManager.Play(audioName, position);
    }

    
    
    // --- Trigger interface ---
    public void OnChildTriggerEnter(Collider other, TriggerType tType=TriggerType.Left)
    {
        if (!Object.HasStateAuthority) return;

        print("Triggered");

        if (other.TryGetComponent(out Vial vial) && vial.Type is ObjectType.InputCrystal)
            AddBox(vial);
    }

    public void OnChildTriggerExit(Collider other, TriggerType tType=TriggerType.Left)
    {
        // Not needed right now
    }
}