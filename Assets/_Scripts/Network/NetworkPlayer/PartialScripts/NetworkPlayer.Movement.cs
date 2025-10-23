using Fusion;
using UnityEngine;

public partial class NetworkPlayer
{
    [Header("Movement")]
    [SerializeField] private float maxSpeed = 4f;
    [SerializeField] private float acceleration = 30f;
    [SerializeField] private float rotationAngle = 300f;
    [SerializeField] private float jumpForce = 20f;
    
    private Vector2 moveInputVector = Vector2.zero;
    public Vector2 MoveInputVector => moveInputVector;
    private bool isJumpButtonPressed = false;
    private TickTimer jumpBuffer;
    
    
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
                audioManager.Play("Walk");
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
        if (networkInputData.IsJumpPressed)
            jumpBuffer = TickTimer.CreateFromSeconds(Runner, 0.15f); // 150ms coyote time

        if (isGrounded && jumpBuffer.IsRunning)
        {
            jumpBuffer = TickTimer.None;
            
            animatedModel.Play("Isis_Jump");
            Vector3 launchDir = (networkInputData.MoveDirection + Vector3.up).normalized;
            rb.AddForce(launchDir * jumpForce, ForceMode.Impulse);
        }
    }
}