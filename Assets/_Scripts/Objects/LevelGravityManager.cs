using UnityEngine;

public class LevelGravityManager : MonoBehaviour
{
    [SerializeField] private Vector3 levelGravity = new Vector3(0, -3.71f, 0);
    private Vector3 defaultGravity;

    void Awake()
    {
        // Store the original gravity so we can reset it later
        defaultGravity = Physics.gravity;
        
        // Set the new gravity for this scene
        Physics.gravity = levelGravity;
    }

    void OnDestroy()
    {
        // Reset to default when leaving the scene
        Physics.gravity = defaultGravity;
    }
}