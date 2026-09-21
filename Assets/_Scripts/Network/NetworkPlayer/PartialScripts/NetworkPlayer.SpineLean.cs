using UnityEngine;

public partial class NetworkPlayer
{
    [Header("Juice - Lean Settings")]
    [SerializeField] private Transform spineTarget;
    [SerializeField] private float maxLeanAngle = 25f; // backward lean
    [SerializeField] private float forwardSnapAngle = 15f; // how far to snap forward
    [SerializeField] private float leanLerpSpeed = 10f; // general lerp speed
    [SerializeField] private float snapSpeed = 25f; // how quickly it snaps forward
    [SerializeField] private float recoverSpeed = 8f; // how quickly it recovers back to 0
    [SerializeField] private float speedThreshold = 0.1f; // movement threshold

    private float currentLeanZ = 0f;
    private bool isSnappingForward = false;
    private float snapTimer = 0f;
    private const float SNAP_DURATION = 0.2f; // how long to hold the forward lean

    private void UpdateSpineLean(float localForwardVelocity)
    {
        if (spineTarget == null) return;

        float clampedSpeed = Mathf.Clamp(localForwardVelocity, 0f, 10f);
        float targetLeanZ;

        if (clampedSpeed > speedThreshold)
        {
            // Actively moving → lean back dynamically
            isSnappingForward = false;
            snapTimer = 0f;

            targetLeanZ = Mathf.Lerp(0f, -maxLeanAngle, clampedSpeed / 10f);
            currentLeanZ = Mathf.Lerp(currentLeanZ, targetLeanZ, Runner.DeltaTime * leanLerpSpeed);
        }
        else
        {
            // If we just stopped and we're not already snapping
            if (!isSnappingForward && Mathf.Abs(currentLeanZ) > 1f)
            {
                isSnappingForward = true;
                snapTimer = 0f;
            }

            if (isSnappingForward)
            {
                // Snap quickly toward forward lean, hold briefly, then return to neutral
                snapTimer += Runner.DeltaTime;

                if (snapTimer < SNAP_DURATION * 0.5f)
                {
                    // First half: snap forward quickly
                    currentLeanZ = Mathf.Lerp(currentLeanZ, forwardSnapAngle, Runner.DeltaTime * snapSpeed);
                }
                else
                {
                    // Second half: return to neutral smoothly
                    currentLeanZ = Mathf.Lerp(currentLeanZ, 0f, Runner.DeltaTime * recoverSpeed);
                    
                    // Reset once close enough to 0
                    if (Mathf.Abs(currentLeanZ) < 0.5f)
                        isSnappingForward = false;
                }
            }
            else
            {
                // Fully stopped and neutral
                currentLeanZ = Mathf.Lerp(currentLeanZ, 0f, Runner.DeltaTime * recoverSpeed);
            }
        }

        // Apply to IK target
        Vector3 euler = spineTarget.localEulerAngles;
        euler.z = currentLeanZ;
        spineTarget.localEulerAngles = euler;
    }
}
