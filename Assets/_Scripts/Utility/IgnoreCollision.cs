using System;
using UnityEngine;

public class IgnoreCollision : MonoBehaviour
{
    [SerializeField] private Collider[] collidersToIgnore;
    private Collider thisCollider;

    private void Awake()
    {
        thisCollider = GetComponent<Collider>();
    }

    private void Start()
    {
        foreach (Collider otherCollider in collidersToIgnore)
        {
            Physics.IgnoreCollision(thisCollider, otherCollider);
        }
    }
}