using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(Collider))]
public class BoxingGloveProjectile : MonoBehaviour
{
    [SerializeField] private float punchForce = 25f;
    [SerializeField] private float upwardPunchBias = 2f;
    
    private Rigidbody rb;
    private Joint joint;
    private bool hasHit = false;

    
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        joint = GetComponent<Joint>();
    }

    public void Launch(Vector3 direction, float launchForce)
    {
        rb.isKinematic = false;
        rb.AddForce(direction * launchForce, ForceMode.Impulse);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (hasHit) return;

        // Apply knockback to any Rigidbody or ragdoll bone
        Rigidbody hitRb = collision.rigidbody;
        if (hitRb != null && hitRb != rb)
        {
            Vector3 knockbackDir = (collision.contacts[0].point - transform.position).normalized;
            knockbackDir.y += upwardPunchBias;
            hitRb.AddForce(knockbackDir.normalized * punchForce, ForceMode.Impulse);
        }

        hasHit = true;

        // Snap the joint on impact so it immediately behaves like loose debris
        ReleaseTether();
    }

    public void ReleaseTether()
    {
        if (joint != null)
        {
            Destroy(joint);
            joint = null;
        }
    }
}