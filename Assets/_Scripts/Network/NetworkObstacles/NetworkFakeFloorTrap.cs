using Fusion;
using UnityEngine;

public class NetworkFakeFloorTrap : NetworkBehaviour, ITriggerReceiver
{
    [SerializeField] private Rigidbody acidRB;
    [SerializeField] private float speed = 2f;
    [SerializeField] private float topY = 5f;

    private bool hasBeenTriggered;
    private float targetY;

    
    public void OnChildTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) hasBeenTriggered = true;
    }

    public override void FixedUpdateNetwork()
    {
        if (!hasBeenTriggered) return;
        
        // Work in local space
        Vector3 localPos = transform.InverseTransformPoint(acidRB.position);
        localPos.y = Mathf.MoveTowards(localPos.y, topY, speed * Runner.DeltaTime);
        
        // Back to world space for rb.MovePosition()
        Vector3 worldTarget = transform.TransformPoint(localPos);
        acidRB.MovePosition(worldTarget);
    }

    public void OnChildTriggerExit(Collider other)
    {
        // Do nothing
    }
}