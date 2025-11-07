using System;
using Fusion;
using UnityEngine;

public class NetworkElevator : NetworkBehaviour, ILever
{
    [SerializeField] private NetworkLever lever;
    [SerializeField] private NetworkButton overrideButton;
    [SerializeField] private float speed = 2f;
    [SerializeField] private float topY = 30f;
    [SerializeField] private float bottomY = -0.5f;

    private Rigidbody rb;
    private Transform floorParent;
    private float targetY;
    
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        floorParent = transform.parent;
        bottomY = transform.localPosition.y;

        //lever.ToggleLeverOff();
    }
    
    public override void Spawned()
    {
        targetY = bottomY;
        MoveFloor();
    }

    public override void FixedUpdateNetwork()
    {
        if (!Object.HasStateAuthority) return;
        if (overrideButton.IsButtonPressed) lever.ToggleLeverOff();

        MoveFloor();
    }

    private void MoveFloor()
    {
        // Work in local space
        Vector3 localPos = floorParent.InverseTransformPoint(rb.position);
        localPos.y = Mathf.MoveTowards(localPos.y, targetY, speed * Runner.DeltaTime);
        
        // Back to world space for rb.MovePosition()
        Vector3 worldTarget = floorParent.TransformPoint(localPos);
        rb.MovePosition(worldTarget);
    }

    public void OnLeverToggled(bool state)
    {
        if (!Object.HasStateAuthority) return;

        targetY  = state ? topY : bottomY;;
    }
}