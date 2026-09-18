using System;
using Fusion;
using Fusion.Addons.Physics;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// Base class for the networked player. This script takes care of the input, component setup, syncing joints, calling
/// functions, as well as handlign states based on the input.
/// The partial scripts are where the functions live -> Movement, Ragdoll, Grounding, and Animation.
/// Camera, grabbing, and respawning are their own components.
/// </summary>
public partial class NetworkPlayer : NetworkBehaviour, IPlayerLeft
{
    #region Variables
    
    public static NetworkPlayer Local { get; set; }
    
    // Player number is networked so it can be synced across all clients
    [Networked, OnChangedRender(nameof(OnPlayerIdentityChanged))] public PlayerRef PlayerRefValue { get; set; }
    [Networked, OnChangedRender(nameof(OnPlayerCustomizationChanged))] public PlayerCustomizationData CustomizationData { get; set; }
    [Networked, OnChangedRender(nameof(OnEquippedCustomizationChanged)), Capacity(3)] public NetworkArray<int> EquippedItemIDs { get; }

    [Header("References")] 
    [SerializeField] private Vector3 spawnPoint;
    [SerializeField] private TextMeshProUGUI playerNumberText;
    [SerializeField] private Animator animatedModel;
    [SerializeField] private SkinnedMeshRenderer bodyMeshRenderer;
    [SerializeField] private Transform playerVest;
    [SerializeField] private GameObject burntPlayerVest;
    [SerializeField] private Image staminaFillImage;
    [SerializeField] private GameObject staminaBarParentObj;
    public Image StaminaFillImage => staminaFillImage;
    
    [Header("Juice - Dust Trail")]
    [SerializeField] private ParticleSystem dustFXParticles;
    [SerializeField] private Vector2 rateOverDistanceRange = new Vector2(3f, 15f);
    [SerializeField] private Vector2 startSizeRange = new Vector2(0.1f, 0.4f);
    [SerializeField] private Vector2 startSpeedRange = new Vector2(0.5f, 2f);
    
    [Networked, HideInInspector] public float NetworkedMovementSpeed { get; set; }
    [Networked, OnChangedRender(nameof(OnBurnedChanged)), HideInInspector] public NetworkBool IsBurned { get; set; } 
    
    // References (SubSystems)
    private NetworkPlayerRespawn playerRespawn;
    private NetworkPlayerCamera playerCamera;
    private NetworkPlayerGrab playerGrab;
    private PlayerCustomizationVisuals playerCustomizationVisuals;
    
    // References
    private Rigidbody rb;
    private NetworkRigidbody3D networkRB;
    private ConfigurableJoint mainJoint;
    private Collider mainCollider;
    private PlayerInput playerInput;
    private InputReader inputReader;
    private SyncPhysicsObject[] syncPhysicsObjects;
    private AudioManager audioManager;
    private ThemeSong themeSong;
    private NetworkGameManager networkGameManager;
    private LocalPlayerUIManager localPlayerUIManager;
    private AudioListener audioListener; // This is on the main camera
    private DissolvingController dissolvingController;
    private ChangeDetector ragdollChanges; // Change detector for flattening Blobby

    public NetworkPlayerCamera PlayerCamera => playerCamera;
    
    // Input
    private NetworkInputData networkInputData;
    private bool isReviveButtonPressed = false;
    private bool isGrabButtonPressed, isLeftGrabButtonPressed, isRightGrabButtonPressed, isLiftButtonPressed = false;
    private bool isUseItemButtonPressed = false;
    private byte localSelectedSlot = 0;
    
    // States
    private bool isGrabbingActive = false;
    public bool IsGrabbingActive => isGrabbingActive;
    
    private bool isLeftHandGrabbingActive = false;
    public bool IsLeftHandGrabbingActive => isLeftHandGrabbingActive;
    
    private bool isRightHandGrabbingActive = false;
    public bool IsRightHandGrabbingActive => isRightHandGrabbingActive;
    private bool isLiftingActive = false;
    public bool IsLiftingActive => isLiftingActive;
    
    #endregion
    
    #region Setup

    private void GetReferences()
    {
        rb = GetComponent<Rigidbody>();
        networkRB = GetComponent<NetworkRigidbody3D>();
        mainCollider = GetComponent<Collider>();
        mainJoint = GetComponent<ConfigurableJoint>();
        playerInput = GetComponent<PlayerInput>();
        inputReader = GetComponent<InputReader>();
        audioManager = FindFirstObjectByType<AudioManager>();
        themeSong = FindFirstObjectByType<ThemeSong>();
        dissolvingController = GetComponent<DissolvingController>();
        playerCustomizationVisuals = GetComponent<PlayerCustomizationVisuals>();
        
        syncPhysicsObjects = GetComponentsInChildren<SyncPhysicsObject>(); 
    }

    private void InitializeSubSystems()
    {
        // SubSystem Setup: Player Respawn
        playerRespawn = GetComponent<NetworkPlayerRespawn>();
        if (playerRespawn == null)
            playerRespawn = gameObject.AddComponent<NetworkPlayerRespawn>();
        playerRespawn.Initialize(this, networkRB, dissolvingController);
        
        // SubSystem Setup: Player Camera
        playerCamera = GetComponent<NetworkPlayerCamera>();
        if (playerCamera == null)
            playerCamera = gameObject.AddComponent<NetworkPlayerCamera>();
        
        // SubSystem Setup: Player Grab
        playerGrab = GetComponent<NetworkPlayerGrab>();
        if (playerGrab == null)
            playerGrab = gameObject.AddComponent<NetworkPlayerGrab>();
        playerGrab.Initialize(this);
        
        // (Not a sub-system) Send local player location to barriers
        if (Object.HasInputAuthority && Local != null)
        {
            var barriers = GameObject.FindObjectsByType<BarrierSection>(FindObjectsSortMode.None);
            foreach (BarrierSection barrier in barriers)
            {
                barrier.InitializeBarrierSections(Local.transform);
            }
        }
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        //startSlerpPositionSpring = mainJoint.slerpDrive.positionSpring;
    }

    public override void Spawned()
    {
        GetReferences();
        
        // Set local first so sub-systems don't throw a null reference error
        if (Object.HasInputAuthority)
            Local = this;
        
        InitializeSubSystems();
        
        startSlerpPositionSpring = mainJoint.slerpDrive.positionSpring;
        
        // Called on every instance when the object spawns locally. OnChangedRender is NOT invoked on initial spawn, so initialize here as well
        // UpdatePlayerNumberUI();
        //OnCustomizationChanged();
        
        networkGameManager = FindFirstObjectByType<NetworkGameManager>();
        localPlayerUIManager = FindFirstObjectByType<LocalPlayerUIManager>();
        ragdollChanges = GetChangeDetector(ChangeDetector.Source.SimulationState);

        if (Object.HasStateAuthority)
        {
            if (!networkGameManager.NetworkPlayers.ContainsKey(Object.InputAuthority))
                networkGameManager.NetworkPlayers.Add(Object.InputAuthority, this);
        }

        if (Object.HasInputAuthority)
        {
            // Observer pattern for the UI manager (to handle pause)
            var uiManager = FindFirstObjectByType<LocalPlayerUIManager>();
            if (uiManager != null && inputReader != null)
                uiManager.SetInputSource(inputReader);
            
            // Player camera
            playerCamera.SetupCamera(Object.HasInputAuthority);
            networkGameManager.ScoreText.text = 0.ToString();
            
            // Ensure PlayerInput is enabled for the local player only
            if (playerInput != null)
            {
                playerInput.enabled = true;

                if (Gamepad.current != null)
                {
                    playerInput.SwitchCurrentControlScheme("Gamepad", Gamepad.current);
                    Utils.DebugLog("Switched to Gamepad control scheme.");
                }
                else
                {
                    playerInput.SwitchCurrentControlScheme("Keyboard&Mouse", Keyboard.current, Mouse.current);
                    Debug.Log("Switched to Keyboard&Mouse control scheme.");
                }
            }
            
            // Load from PlayerPrefs and tell the host
            string localName = PlayerPrefs.GetString(Utils.GetKey("PlayerName"), "JOHN");
            string localHexColor = PlayerPrefs.GetString(Utils.GetKey("PlayerColor"), "#FFFFFF");
            ColorUtility.TryParseHtmlString(localHexColor, out Color localColor);
            
            RPC_SetCustomization(localName, localColor);
            transform.name = $"Player_{localName}";
            
            // Load our equipped items and tell the host
            int[] mySavedItems = LocalPlayerInventoryManager.LoadInventory().EquippedItemIDs;
            RPC_SyncEquippedItems(mySavedItems);
            
            // Enable InputReader for local player
            if (inputReader != null) inputReader.enabled = true;
        }
        else
        {
            // Disable PlayerInput for non-local players
            if (playerInput != null) playerInput.enabled = false;
            if (inputReader != null) inputReader.enabled = false;
            
            // Disable stamina bar UI for non-local players
            if (staminaBarParentObj != null) staminaBarParentObj.SetActive(false);
        }
        
        OnPlayerCustomizationChanged();
        OnEquippedCustomizationChanged();
    }

    public void AssignPlayerIdentity(PlayerRef playerRef)
    {
        if (Object.HasStateAuthority) PlayerRefValue = playerRef;
    }

    private void OnPlayerIdentityChanged()
    {
        OnPlayerCustomizationChanged();
    }
    
    private void UpdatePlayerNumberUI()
    {
        if (playerNumberText == null) return;
        
        // Make a readable number based on PlayerRef
        int playerNumber = PlayerRefValue.RawEncoded % 1000;
        playerNumber--; // Right now players start at 2, this is a hack to make it start at 1
        playerNumberText.text = $"Player {playerNumber}";
        
        // Give each player a distinct color based on their ID
        float hue = (playerNumber * 137.508f) % 360f;
        Color color = Color.HSVToRGB(hue / 360f, 0.65f, 0.9f);
        playerNumberText.color = color;
        
        // Update player body color to be the same as their name
        bodyMeshRenderer.material.SetColor("_ChromaKeyColorReplacement", color);
        
        // Update rim color to complement their body color
        Color.RGBToHSV(color, out float h, out float s, out float v);
        float rimV = Mathf.Clamp01(1.2f - v); // brighter rims on darker colors
        Color rimColor = Color.HSVToRGB((h + 180f) % 1f, s * 0.5f, rimV);
        
        bodyMeshRenderer.material.SetColor("_RimLightColor", rimColor);
    }

    private void OnPlayerCustomizationChanged()
    {
        if (playerNumberText == null) return;

        playerNumberText.text = CustomizationData.PlayerName.ToString();
        playerNumberText.color = CustomizationData.PlayerColor;
        bodyMeshRenderer.material.SetColor("_ChromaKeyColorReplacement", CustomizationData.PlayerColor);
        
        // Calculate and apply the rim color
        Color.RGBToHSV(CustomizationData.PlayerColor, out float h, out float s, out float v);
        float rimV = Mathf.Clamp01(1.2f - v); // Brighter rims on darker colors
        Color rimColor = Color.HSVToRGB((h + 180f) % 1f, s * 0.5f, rimV);
        bodyMeshRenderer.material.SetColor("_RimLightColor", rimColor);
    }

    private void OnEquippedCustomizationChanged()
    {
        if (playerCustomizationVisuals == null) return;
        
        // Copy the networked array into a standard C# array
        int[] ids = new int[3];
        for (int i = 0; i < 3; i++)
            ids[i] = EquippedItemIDs[i];
        
        // Update the visuals
        playerCustomizationVisuals.UpdateVisuals(ids);
    }

    public void RemovePlayerInputAuthority()
    {
        Local.Object.RemoveInputAuthority();
    }
    
    #endregion

    #region Update
    private void Update()
    {
        if (!Object || !Object.IsValid) return;
        if (!localPlayerUIManager.IsLocalGamePaused) ReadInputFromUnity();
    }

    private void ReadInputFromUnity()
    {
        // Only read input on the client that owns this player
        if (Object.HasInputAuthority)
        {
            moveInputVector = inputReader.MoveValue;
            isJumpButtonPressed = inputReader.IsJumpButtonPressed;
            isReviveButtonPressed = inputReader.IsReviveButtonPressed;
            isGrabButtonPressed = inputReader.IsGrabButtonPressed;
            isLeftGrabButtonPressed = inputReader.IsLeftGrabButtonPressed;
            isRightGrabButtonPressed = inputReader.IsRightGrabButtonPressed;
            isLiftButtonPressed = inputReader.IsLiftButtonPressed;
            isUseItemButtonPressed = isUseItemButtonPressed || inputReader.IsUseItemPressed;
            
            if (inputReader.IsSelectItem1Pressed) localSelectedSlot = 0;
            else if (inputReader.IsSelectItem2Pressed) localSelectedSlot = 1;
            else if (inputReader.IsSelectItem3Pressed) localSelectedSlot = 2;
            else if (inputReader.IsSelectItem4Pressed) localSelectedSlot = 3;
        }
        else
        {
            // Ensure no stale input on non-local player
            moveInputVector = Vector2.zero;
            isJumpButtonPressed = false;
            isReviveButtonPressed = false;
            isGrabButtonPressed = false;
            isLeftGrabButtonPressed = false;
            isRightGrabButtonPressed = false;
            isLiftButtonPressed = false;
            isUseItemButtonPressed = false;
        }
    }

    public override void FixedUpdateNetwork()
    {
        float localForwardVelocity = 0f;

        // Check if input was sucessfully retrieved this tick
        if (GetInput(out NetworkInputData currentInput))
            networkInputData = currentInput;

        // Set local flags based on network input
        isGrabbingActive = networkInputData.IsGrabPressed;
        isLeftHandGrabbingActive = networkInputData.IsLeftGrabPressed;
        isRightHandGrabbingActive = networkInputData.IsRightGrabPressed;
        isLiftingActive = networkInputData.IsLiftPressed;
        
        HandlePlayer();
    }

    private void HandlePlayer()
    {
        if (Object.HasStateAuthority)
        {
            GravityAndGrounding();
            
            // Calculate speed ONLY on the Host
            Vector3 localVelocity = transform.InverseTransformDirection(rb.linearVelocity);
            NetworkedMovementSpeed = new Vector3(localVelocity.x, 0, localVelocity.z).magnitude;
        }/*
        else if (Object.HasInputAuthority)
        {
            // Do a lightweight local estimate for visuals
            localForwardVelocity = rb.linearVelocity.magnitude;
            isGrounded = Physics.CheckSphere(transform.position, 0.25f);
        }*/
        
        // Respawn in place
        if (networkInputData.IsRevivePressed)
            playerRespawn.Respawn(false);
        
        // Only respawn if the timer was actually set and has now finished
        if (!IsActiveRagdoll && waitBeforeRespawn.IsRunning && waitBeforeRespawn.Expired(Runner))
        {
            playerRespawn.Respawn(false);
        }

        // Active ragdoll
        if (IsActiveRagdoll)
        {
            HandleStamina();
            HandleMovement();
        }
        
        SyncAnimations(NetworkedMovementSpeed);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.InputAuthority)]
    public void RPC_UpdateScoreUI(int newScore, int addedScore)
    {
        networkGameManager.ScoreText.text = newScore.ToString();
        ScorePopupManager.Instance.ShowScore(addedScore);
    }
    #endregion

    #region Other functions

    public void SpawnVestAfterBurning()
    {
        var vest = Runner.Spawn(burntPlayerVest, playerVest.position, playerVest.localRotation);
        vest.transform.parent = playerVest.transform.parent;
        playerVest.gameObject.SetActive(false);
    }
    
    private void TriggerBurnVisuals()
    {
        // Visuals/FX
        if (dissolvingController != null) dissolvingController.BeginFx();
    
        SpawnVestAfterBurning();
    
        if (audioManager != null) 
            audioManager.Play("PlayerBurn", transform.position);
    }

    // This function gets called from other objects to burn the player
    public void Burn()
    {
        if (Object.HasStateAuthority)
        {
            if (IsBurned) return; // Don't burn twice
        
            IsBurned = true;
            MakeRagdoll(); // Flatten the player
        }
    }

    private void ResetBurnVisuals()
    {
        if (dissolvingController != null) 
        {
            dissolvingController.ResetBurningFx(); 
        }
        
        if (playerVest != null) 
        {
            playerVest.gameObject.SetActive(true);
        }
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void RPC_SetCustomization(string newName, Color color)
    {
        try
        {
            if (string.IsNullOrEmpty(newName)) 
                newName = "JOHN";
            
            CustomizationData = new PlayerCustomizationData()
            {
                PlayerName = newName,
                PlayerColor = color
            };
        }
        catch (Exception e)
        {
            // If ANYTHING goes wrong, it prints the exact reason instead of crashing!
            Debug.LogError("Error in Customization RPC: " + e.Message);
        }
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_SyncEquippedItems(int[] myEquippedItems)
    {
        for (int i = 0; i < myEquippedItems.Length; i++)
        {
            EquippedItemIDs.Set(i, myEquippedItems[i]);
        }
    }

    #endregion
    
    #region Network Functions
    private void OnBurnedChanged()
    {
        if (IsBurned)
            TriggerBurnVisuals();
        else
            ResetBurnVisuals();
    }
    
    public void PlayerLeft(PlayerRef player)
    {
        if (Object.InputAuthority == player)
            Runner.Despawn(Object);
    }

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        if (Object.HasStateAuthority && networkGameManager != null)
        {
            networkGameManager.NetworkPlayers.Remove(Object.InputAuthority);
        }

        
        if (Object.HasInputAuthority)
        {
            // Unsubscribe from the pause function
            var uiManager = FindFirstObjectByType<LocalPlayerUIManager>();
            if (uiManager != null && inputReader != null)
                inputReader.OnPausePressed -= uiManager.TogglePause;
            
            playerCamera.DespawnCamera();
        }
    }

    public NetworkInputData GetNetworkInput()
    {
        NetworkInputData data = new NetworkInputData();
        
        data.RawInput = moveInputVector;
        
        // Set the network inputs to the local inputs so they can be sent to the host
        if (isJumpButtonPressed) data.IsJumpPressed = true;
        if (isReviveButtonPressed) data.IsRevivePressed = true;
        if (isGrabButtonPressed) data.IsGrabPressed = true;
        if (isLeftGrabButtonPressed) data.IsLeftGrabPressed = true;
        if (isRightGrabButtonPressed) data.IsRightGrabPressed = true;
        if (isLiftButtonPressed) data.IsLiftPressed = true;
        
        if (isUseItemButtonPressed) data.IsUseItemPressed = true;
        data.SelectedSlotIndex = localSelectedSlot;

        
        // Clear local flags since they've been sent to the host (We don't need to reset our grab buttons as they are a continuous press)
        isJumpButtonPressed = false;
        //isReviveButtonPressed = false; // Not sure if this should be cleared anymore since I switched it to a hold
        isUseItemButtonPressed = false;
        
        // Compute camera-relative world direction only on the local client (passing data by ref since struct value is changed)
        playerCamera.ComputeCameraRelativeWorldDirection(Object.HasInputAuthority, ref data);
        
        return data;
    }

    public override void Render()
    {
        // This should only run on clients, not the host
        if (!Object.HasStateAuthority)
        {
            var interpolated = new NetworkBehaviourBufferInterpolator(this);

            // Get the networked physics objects from the host and update the clients
            for (int i = 0; i < syncPhysicsObjects.Length; i++)
            {
                syncPhysicsObjects[i].transform.localRotation = Quaternion.Slerp(
                    syncPhysicsObjects[i].transform.localRotation,
                    NetworkPhysicsSyncedRotations.Get(i), interpolated.Alpha);
            }
        }
        
        UpdateSpineLean(NetworkedMovementSpeed);
        UpdateDustFX(NetworkedMovementSpeed);
        
        foreach (var change in ragdollChanges.DetectChanges(this))
        {
            if (change == nameof(flattenSignal) && flattenSignal > 0)
                LocalFlattenBlobby();
        }

        // Smoother camera movement for clients
        if (Object.HasInputAuthority)
        {
            playerCamera.Render(Runner.LocalAlpha);
        }
    }
    #endregion
}