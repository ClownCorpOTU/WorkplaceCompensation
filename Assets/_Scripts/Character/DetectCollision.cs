using UnityEngine;

/// <summary>
/// This script is responsible for detecting collisions and activating ragdoll when hit (right now only on head and spine)
/// </summary>
public class DetectCollision : MonoBehaviour
{
    [SerializeField] private float knockoutThreshold = 15f;
    [SerializeField] private float knockbackForceMultiplier = 0.5f;
    [SerializeField] private float maxKnockbackForce = 30f;
    
    private NetworkPlayer networkPlayer;
    private Rigidbody rb;
    private ContactPoint[] contactPoints = new ContactPoint[5];
    
    private void Awake()
    {
        networkPlayer = GetComponentInParent<NetworkPlayer>();
        rb = GetComponent<Rigidbody>();
    }
    
    private void OnCollisionEnter(Collision collision)
    {
        if (!networkPlayer.HasStateAuthority) return;
        if (!networkPlayer.IsActiveRagdoll) return;
        
        if (!collision.gameObject.CompareTag("CauseDamage")) return;
        if (collision.collider.transform.root == networkPlayer.transform) return;
        
        // Get contact points
        int numberOfContacts = collision.GetContacts(contactPoints);

        for (int i = 0; i < numberOfContacts; i++)
        {
            ContactPoint contactPoint = contactPoints[i];
            
            // Get the contact impulse
            Vector3 contactImpulse = contactPoint.impulse / Time.fixedDeltaTime;
            if (contactImpulse.magnitude < knockoutThreshold) continue;
            
            // Player knockout (Ragdoll)
            networkPlayer.OnPlayerBodyPartHit();
            
            // Send the player up a bit so they don't stop due to friction immediately
            Vector3 forceDirection = (contactImpulse + Vector3.up) * knockbackForceMultiplier;
            
            // Limit the force
            forceDirection = Vector3.ClampMagnitude(forceDirection, maxKnockbackForce);
            
            // Knock the player back
            rb.AddForce(forceDirection, ForceMode.Impulse);
        }
    }
}