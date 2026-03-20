using UnityEngine;
using Fusion;
using System.Collections.Generic;
using UnityEngine.Serialization;

public class WorkplaceWindFan : NetworkBehaviour
{
    [SerializeField] private float windPower = 12f;
    [SerializeField] private LayerMask affectedLayers;

    // We track the 'Root' to avoid double-counting, and a list of RBs to apply force to.
    private Dictionary<Transform, Rigidbody[]> _activeEntities = new Dictionary<Transform, Rigidbody[]>();

    public override void FixedUpdateNetwork()
    {
        if (!Object.HasStateAuthority) return;

        foreach (var entity in _activeEntities)
        {
            foreach (Rigidbody rb in entity.Value)
            {
                if (rb == null) continue;

                rb.AddForce(Vector3.up * windPower, ForceMode.Acceleration);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // 1. Check if the layer matches (Players, Props, etc.)
        if (((1 << other.gameObject.layer) & affectedLayers) != 0)
        {
            // 2. Find the highest parent (The "Root")
            Transform root = other.transform.root;

            // 3. If we aren't tracking this entity yet, grab ALL its rigidbodies
            if (!_activeEntities.ContainsKey(root))
            {
                Rigidbody[] allBones = root.GetComponentsInChildren<Rigidbody>();
                _activeEntities.Add(root, allBones);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Transform root = other.transform.root;
        if (_activeEntities.ContainsKey(root))
        {
            _activeEntities.Remove(root);
        }
    }
}