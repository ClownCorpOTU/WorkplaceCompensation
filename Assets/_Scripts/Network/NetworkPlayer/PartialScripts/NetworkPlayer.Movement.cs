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

    [Header("Audio Settings")] 
    [SerializeField] private float footstepInterval = 0.2f;

    private Vector2 moveInputVector = Vector2.zero;
    public Vector2 MoveInputVector => moveInputVector;
    private bool isJumpButtonPressed = false;

    private float footstepTimer;
    //private TickTimer jumpBuffer;
    //private bool jumpConsumed = false;
    
    [Networked] private TickTimer jumpCooldown { get; set; }
    [Networked, OnChangedRender(nameof(OnJumpTriggered))] private int jumpCount { get; set; }
    
    
    private void HandleMovement()
    {
        float inputMagnitude = networkInputData.RawInput.magnitude;
        Vector3 moveDir = networkInputData.MoveDirection;

        if (moveDir.sqrMagnitude > 0.001f)
        {
            ApplyRotation(moveDir);
            
            // Apply movement (host handles physics)
            if (NetworkedMovementSpeed < maxSpeed)
            {
                rb.AddForce(moveDir * (inputMagnitude * acceleration), ForceMode.Acceleration);

                if (IsGrounded)
                {
                    //RPC_Play("Walk", transform.position);

                    footstepTimer -= Runner.DeltaTime;

                    if (footstepTimer <= 0f)
                    {
                        RPC_Play("Walk", transform.position);
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
        CurrentStamina -= 3f;
        
        jumpCooldown = TickTimer.CreateFromSeconds(Runner, 0.35f);
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
    
    [Rpc(RpcSources.StateAuthority, RpcTargets.All, TickAligned = false)]
    private void RPC_Play(string audioName, Vector3 position)
    {
        if (audioManager != null) audioManager.Play(audioName, position);
    }
    
    
    /*
    private void HandleJump()
    {
        if (!Object.HasStateAuthority) return;
        
        // Buffer input (150ms coyote time)
        if (networkInputData.IsJumpPressed)
            jumpBuffer = TickTimer.CreateFromSeconds(Runner, 0.15f);
        
        // Only jump if grounded, buffer is active, and we haven't consumed it yet
        if (IsGrounded && jumpBuffer.IsRunning && !jumpConsumed)
        {
            // Check stamina before allowing jump
            if (CurrentStamina >= 3f)
            {
                jumpConsumed = true;
                jumpBuffer = TickTimer.None;
                PerformJump();
            }
        }
        
        // Reset jump consumption once we have landed
        if (IsGrounded && !jumpBuffer.IsRunning)
            jumpConsumed = false;

        /*
        if (!Object.HasStateAuthority) return;

        // Buffer jump input
        if (networkInputData.IsJumpPressed)
            jumpBuffer = TickTimer.CreateFromSeconds(Runner, 0.15f); // 150ms coyote time

        // Execute jump once per liftoff
        if (IsGrounded && jumpBuffer.IsRunning && !jumpConsumed)
        {
            jumpBuffer = TickTimer.None;
            jumpConsumed = true;

            PerformJump(); // separate function for clarity
        }

        if (IsGrounded && !jumpBuffer.IsRunning)
        {
            //print("Jump consumed = false");
            jumpConsumed = false;
        }
        *\
    }

    private void PerformJump()
    {
        // Runner.IsForward is true only when this is a new tick. Prevents audio from playing multiple times
        if (Runner.IsForward)
        {
            audioManager.Play("Jump", transform.position);
            CurrentStamina -= 3f;
        }
        
        // Calculate velocity
        Vector3 launchDir = (networkInputData.MoveDirection + Vector3.up).normalized;
        rb.AddForce(launchDir * jumpForce, ForceMode.Impulse);
        
        // Set variables
        IsGrounded = false;
        lastActivityTime = Runner.SimulationTime;

        /*
        if (CurrentStamina < 3f) return; // not enough energy

        // ONLY play audio if this is the "Forward" (first) execution of this tick
        if (Runner.IsForward)
        {
            audioManager.Play("Jump", transform.position);
        }

        float gravityMagnitude = Mathf.Abs(Physics.gravity.y);

        float gravityRatio = 9.81f / gravityMagnitude;
        float adjustedHeight = jumpHeight * gravityRatio;

        float requiredVelocity = Mathf.Sqrt(2f * adjustedHeight * gravityMagnitude);
        float totalImpulse = requiredVelocity * rb.mass;

        Vector3 launchDir = (networkInputData.MoveDirection + Vector3.up).normalized;
        //rb.AddForce(launchDir * totalImpulse, ForceMode.Impulse);
        rb.AddForce(launchDir * jumpForce, ForceMode.Impulse);

        lastActivityTime = Runner.SimulationTime;

        // Even older
        // Drain once
        //CurrentStamina = Mathf.Max(0f, CurrentStamina - 3f);

        // Jump logic
        Vector3 launchDir = (networkInputData.MoveDirection + Vector3.up).normalized;
        rb.AddForce(launchDir * jumpForce, ForceMode.Impulse);

        // Reset regen timer
        lastActivityTime = Runner.SimulationTime;
        *\
    }
    */
}