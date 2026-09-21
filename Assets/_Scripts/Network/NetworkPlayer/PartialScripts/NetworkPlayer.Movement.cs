using Fusion;
using UnityEngine;

public partial class NetworkPlayer
{
    [Header("Movement")]
    [SerializeField] private float maxSpeed = 4f;
    [SerializeField] private float acceleration = 30f;
    [SerializeField] private float rotationAngle = 300f;
    [SerializeField] private float jumpForce = 20f;
    [SerializeField] private float jumpHeight = 1.5f;
    [SerializeField] private float jumpCooldownAmount = 0.4f;
    [SerializeField] private float jumpStaminaReduction = 3f;
    [SerializeField, Range(0, 1)] private float jumpStaminaDecreaseMultiplierWhileHolding = 0.6f; 

    [Header("Audio Settings")] 
    [SerializeField] private float footstepInterval = 0.2f;

    private Vector2 moveInputVector = Vector2.zero;
    private bool isJumpButtonPressed = false;
    private bool hasMovedBefore;

    private float footstepTimer;
    
    [Networked] private float currentSpeedMultiplier { get; set; }
    
    [Networked] private TickTimer jumpCooldown { get; set; }
    [Networked] private TickTimer speedBoostCountdown { get; set; }
    [Networked, OnChangedRender(nameof(OnJumpTriggered))] private int jumpCount { get; set; }
    

    public void ApplySpeedBoostEffect(float boost, float duration)
    {
        if (Object.HasStateAuthority)
        {
            if (currentSpeedMultiplier == 0f) currentSpeedMultiplier = 1f;
            
            // Add extra speed on top of whatever player already has
            // (boost - 1f) turns a 1.5x boost into a flat +0.5 addition
            currentSpeedMultiplier += (boost - 1f);
            speedBoostCountdown = TickTimer.CreateFromSeconds(Runner, duration);
        }
    }
    
    private void HandleMovement()
    {
        if (currentSpeedMultiplier == 0f) currentSpeedMultiplier = 1f; // At spawn it's set to 0, but we want to move

        if (speedBoostCountdown.Expired(Runner))
        {
            currentSpeedMultiplier = 1f;
            speedBoostCountdown = TickTimer.None;
        }
        
        float inputMagnitude = networkInputData.RawInput.magnitude;
        Vector3 moveDir = networkInputData.MoveDirection;
        
        float actualMaxSpeed = maxSpeed * currentSpeedMultiplier;
        float actualAcceleration = acceleration * currentSpeedMultiplier;

        if (moveDir.sqrMagnitude > 0.001f)
        {
            ApplyRotation(moveDir);
            
            // Apply movement (host handles physics)
            if (NetworkedMovementSpeed < actualMaxSpeed)
            {
                rb.AddForce(moveDir * (inputMagnitude * actualAcceleration), ForceMode.Acceleration);
                if (!hasMovedBefore && Object.HasInputAuthority)
                {
                    GameEventManager.TriggerEvent(GameEvent.PlayerMoved);
                    hasMovedBefore = true;
                }

                if (IsGrounded)
                {
                    //RPC_Play("Walk", transform.position);

                    footstepTimer -= Runner.DeltaTime;

                    if (footstepTimer <= 0f)
                    {
                        RPC_PlayWalkSound("Walk", transform.position);
                        footstepTimer = footstepInterval;
                    }
                }
            }
        }
        else
        {
            footstepTimer = 0f;
        }
        
        HandleJump();
    }

    private void ApplyRotation(Vector3 moveDir)
    {
        // Rotate character to face camera-relative movement direction
        Vector3 fixedMoveDir = new Vector3(-moveDir.x, moveDir.y, moveDir.z); // -x since model is facing -Z
        
        if (fixedMoveDir.sqrMagnitude < 0.001f) return;
        
        Quaternion desiredRotation = Quaternion.LookRotation(fixedMoveDir, Vector3.up);

        mainJoint.targetRotation =
            Quaternion.RotateTowards(mainJoint.targetRotation, desiredRotation, Runner.DeltaTime * rotationAngle);
    }

    private void HandleJump()
    {
        if (!Object.HasStateAuthority) return;

        if (networkInputData.IsJumpPressed && IsGrounded && jumpCooldown.ExpiredOrNotRunning(Runner))
        {
            if (CurrentStamina >= 3f) ExecuteJump();
        }
    }

    private void ExecuteJump()
    {
        var staminaToReduce = 0f;
        if (IsLeftHandGrabbingActive || IsRightHandGrabbingActive || IsGrabbingActive)
            staminaToReduce = jumpStaminaReduction * jumpStaminaDecreaseMultiplierWhileHolding;
        else
            staminaToReduce = jumpStaminaReduction;
        
        CurrentStamina -= staminaToReduce;
        
        jumpCooldown = TickTimer.CreateFromSeconds(Runner, jumpCooldownAmount);
        jumpCount++;

        Vector3 jumpDir = (networkInputData.MoveDirection + Vector3.up).normalized;
        rb.AddForce(jumpDir * jumpForce, ForceMode.Impulse);

        IsGrounded = false;
        lastActivityTime = Runner.SimulationTime;
    }

    private void OnJumpTriggered()
    {
        audioManager.Play("Jump", transform.position);
    }
    
    [Rpc(RpcSources.All, RpcTargets.StateAuthority, TickAligned = false)]
    private void RPC_PlayWalkSound(string audioName, Vector3 position)
    {
        if (Object.HasStateAuthority)
            if (audioManager != null) audioManager.Play(audioName, position);
        else
            RPC_Play("Walk", transform.position);
    }
    
    [Rpc(RpcSources.StateAuthority, RpcTargets.All, TickAligned = false)]
    private void RPC_Play(string audioName, Vector3 position)
    {
        if (audioManager != null) audioManager.Play(audioName, position);
    }
}