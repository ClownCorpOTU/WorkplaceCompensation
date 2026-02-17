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

    private Vector2 moveInputVector = Vector2.zero;
    public Vector2 MoveInputVector => moveInputVector;
    private bool isJumpButtonPressed = false;
    private TickTimer jumpBuffer;
    private bool jumpConsumed = false;
    
    
    private void HandleMovement(float localForwardVelocity)
    {
        float inputMagnitude = networkInputData.RawInput.magnitude;
        Vector3 moveDir = networkInputData.MoveDirection;

        if (moveDir.sqrMagnitude > 0.001f)
        {
            ApplyRotation(moveDir);
            
            // Apply movement (host handles physics)
            if (localForwardVelocity < maxSpeed)
            {
                rb.AddForce(moveDir * (inputMagnitude * acceleration), ForceMode.Acceleration);
                audioManager.Play("Walk", transform.position);
            }
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
    }

    private void PerformJump()
    {
        if (CurrentStamina < 3f) return; // not enough energy

        //print("Jumping!");
        audioManager.Play("Jump", transform.position);

        float gravityMagnitude = Mathf.Abs(Physics.gravity.y);

        float gravityRatio = 9.81f / gravityMagnitude;
        float adjustedHeight = jumpHeight * gravityRatio;
        
        float requiredVelocity = Mathf.Sqrt(2f * adjustedHeight * gravityMagnitude);
        float totalImpulse = requiredVelocity * rb.mass;
        
        Vector3 launchDir = (networkInputData.MoveDirection + Vector3.up).normalized;
        //rb.AddForce(launchDir * totalImpulse, ForceMode.Impulse);
        rb.AddForce(launchDir * jumpForce, ForceMode.Impulse);

        lastActivityTime = Runner.SimulationTime;

        /*
        // Drain once
        //CurrentStamina = Mathf.Max(0f, CurrentStamina - 3f);

        // Jump logic
        Vector3 launchDir = (networkInputData.MoveDirection + Vector3.up).normalized;
        rb.AddForce(launchDir * jumpForce, ForceMode.Impulse);

        // Reset regen timer
        lastActivityTime = Runner.SimulationTime;
        */
    }
}