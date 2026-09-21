using UnityEngine;

public class LocalUIBillboard : MonoBehaviour
{
    [SerializeField] private AnimationCurve rotationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private float dampingSpeed = 5f;
    
    private Transform localCamera;
    private float currentCurveTime;
    private Quaternion targetRotation;
    
    private void Start()
    {
        if (Camera.main != null)
            localCamera = Camera.main.transform;
    }
    
    private void LateUpdate()
    {
        if (localCamera == null) return;
        
        // 1. Calculate the ideal rotation
        Vector3 direction = transform.position - localCamera.position;
        Quaternion targetRotation = Quaternion.LookRotation(direction);

        // 2. Check if the target has changed significantly
        if (Quaternion.Angle(transform.rotation, targetRotation) > 0.1f)
        {
            // We need to rotate since we are moving
            currentCurveTime += Time.deltaTime * dampingSpeed;
        }
        else
        {
            // We have arrive at the target, so we can reset the curve
            currentCurveTime = 0f;
        }

        // 3. Clamp the time so we don't go past the end of the graph
        currentCurveTime = Mathf.Clamp01(currentCurveTime);

        // 4. Evaluate and apply
        float curveValue = rotationCurve.Evaluate(currentCurveTime);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, curveValue);
    }
}