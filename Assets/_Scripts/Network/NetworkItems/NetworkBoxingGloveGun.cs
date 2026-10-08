using System;
using System.Collections;
using Fusion;
using Unity.Cinemachine;
using UnityEngine;

public class NetworkBoxingGloveGun : NetworkBehaviour
{
    [Header("Aim Settings")]
    [SerializeField] private float aimingFoV = 50f;
    [SerializeField] private float zoomSpeed = 12f;

    [Header("Glove References & Physics")]
    [SerializeField] private Rigidbody gunBody;
    [SerializeField] private BoxingGloveProjectile glove;
    [SerializeField] private Transform launchPoint;
    [SerializeField] private float launchImpulse = 35f;
    [SerializeField] private float maxDistance = 4.5f;

    [Header("Drop & Cleanup")]
    [SerializeField] private float dropDelay = 0.35f;
    [SerializeField] private float despawnDelay = 3.0f;

    private CinemachineCamera cam;
    private float defaultFoV;
    private bool isAiming = false;
    private bool hasFired = false;
    
    
    private void OnEnable()
    {
        if (cam == null)
            cam = FindFirstObjectByType<CinemachineCamera>();

        if (defaultFoV == 0f)
            defaultFoV = cam.Lens.FieldOfView;
    }
    
    public override void FixedUpdateNetwork()
    {
        // Network input code goes here
    }

    private void Update()
    {
        if (cam == null) return;
        
        cam.Lens.FieldOfView = Mathf.Lerp(cam.Lens.FieldOfView, aimingFoV, Time.deltaTime * zoomSpeed);
        
        if (Input.GetKeyDown(KeyCode.G)) FireGlove();
    }

    private void OnDisable()
    {
        if (cam != null)
            cam.Lens.FieldOfView = defaultFoV;
    }

    public void FireGlove()
    {
        if (hasFired) return;
        hasFired = true;

        // 1. Set up the Spring / Joint limit to hold max reach distance
        ConfigurableJoint joint = glove.gameObject.AddComponent<ConfigurableJoint>();
        joint.connectedBody = gunBody;
        joint.autoConfigureConnectedAnchor = false;
        joint.anchor = Vector3.zero;
        joint.connectedAnchor = gunBody.transform.InverseTransformPoint(launchPoint.position);

        // Allow free extension up to maxDistance, then catch hard
        joint.xMotion = ConfigurableJointMotion.Limited;
        joint.yMotion = ConfigurableJointMotion.Limited;
        joint.zMotion = ConfigurableJointMotion.Limited;
        
        SoftJointLimit limit = new SoftJointLimit { limit = maxDistance };
        joint.linearLimit = limit;

        // Add soft spring restitution at the apex
        SoftJointLimitSpring spring = new SoftJointLimitSpring { spring = 400f, damper = 25f };
        joint.linearLimitSpring = spring;

        // 2. Fire outward
        glove.Launch(launchPoint.forward, launchImpulse);

        // 3. Drop from player hands and clean up
        StartCoroutine(DropAndDespawnRoutine());
    }

    private IEnumerator DropAndDespawnRoutine()
    {
        yield return new WaitForSeconds(dropDelay);

        // Release the spring tether
        glove.ReleaseTether();

        // Detach weapon from player hands and enable physics on the gun body
        transform.SetParent(null);
        if (gunBody != null)
        {
            gunBody.isKinematic = false;
            gunBody.useGravity = true;
            gunBody.AddForce(Vector3.down * 2f + launchPoint.forward * 1.5f, ForceMode.Impulse);
        }

        yield return new WaitForSeconds(despawnDelay);

        if (Object.HasStateAuthority)
            Runner.Despawn(Object);
        else if (Runner == null)
            Destroy(gameObject);
    }
}