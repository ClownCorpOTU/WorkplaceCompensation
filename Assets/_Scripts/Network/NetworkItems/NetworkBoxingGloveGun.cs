using System;
using Fusion;
using Unity.Cinemachine;
using UnityEngine;

public class NetworkBoxingGloveGun : NetworkBehaviour
{
    [SerializeField] private float aimingFoV = 50f;
    [SerializeField] private float zoomSpeed = 10f;
    [SerializeField] private float launchForce = 10f;
    
    private Rigidbody rb;
    
    private CinemachineCamera cam;
    private float originalFoV = 0f;
    
    private void OnEnable()
    {
        if (cam == null)
            cam = FindFirstObjectByType<CinemachineCamera>();

        if (originalFoV == 0f)
            originalFoV = cam.Lens.FieldOfView;
        
        if (rb == null)
            rb = GetComponent<Rigidbody>();
        
        rb.isKinematic = true;
    }

    private void Update()
    {
        cam.Lens.FieldOfView = Mathf.Lerp(cam.Lens.FieldOfView, aimingFoV, Time.deltaTime * zoomSpeed);
        
        if (Input.GetKeyDown(KeyCode.G)) UseBoxingGloveGun();
    }

    private void OnDisable()
    {
        if (cam != null)
            cam.Lens.FieldOfView = originalFoV;
    }

    private void UseBoxingGloveGun()
    {
        rb.isKinematic = false;
        rb.AddForce(transform.up * launchForce, ForceMode.Impulse); // Using up due to the model's base rotation
    }
}