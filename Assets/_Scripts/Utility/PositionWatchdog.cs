using UnityEngine;

public class PositionWatchdog : MonoBehaviour
{
    private Vector3 initialPosition;

    void Start()
    {
        initialPosition = transform.position;
    }

    void Update()
    {
        if (Vector3.Distance(transform.position, initialPosition) > 0.1f)
        {
            Debug.LogError($"[WATCHDOG] {gameObject.name} MOVED! From {initialPosition} to {transform.position}");
            
            // This will automatically pause the Unity Editor so you can inspect the scene
            Debug.Break(); 
            
            // Keep the new position as the baseline so it doesn't spam
            initialPosition = transform.position;
        }
    }
}