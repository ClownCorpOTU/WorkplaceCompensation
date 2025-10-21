using UnityEngine;

public partial class NetworkPlayer
{
    private bool isGrounded = false;
    private RaycastHit[] raycastHits = new RaycastHit[10];
    
    
    private void GravityAndGrounding()
    {
        isGrounded = false;
        
        // Check if we are grounded
        int numberOfHits = Physics.SphereCastNonAlloc(rb.position, 0.1f, -Vector3.up, raycastHits, 0.1f);
        for (int i = 0; i < numberOfHits; i++)
        {
            if (raycastHits[i].transform.root == transform) continue; // Ignore self hits

            isGrounded = true;
            break;
        }
        
        // Add extra gravity so character feels less floaty
        if (!isGrounded) rb.AddForce(Vector3.down * 25f);
    }
}