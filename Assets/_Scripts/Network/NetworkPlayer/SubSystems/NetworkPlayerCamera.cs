using Fusion;
using Unity.Cinemachine;
using UnityEngine;

/// <summary>
/// This script takes care of the camera setup, as well as providing the camera-relative world direction.
/// It relies on NetworkPlayer to feed it the input data for now.
/// </summary>
public class NetworkPlayerCamera : MonoBehaviour
{
    [SerializeField] private GameObject cameraContainerPrefab;
    [SerializeField] private Transform camFollow;
    
    private Camera cam;
    private CinemachineCamera cinemachineCamera;
    private CinemachineBrain cinemachineBrain;
    private GameObject localCameraInstance;
    private CinemachineInputAxisController inputController;

    public void SetupCamera(bool hasInputAuthority)
    {
        // Spawn camera
        localCameraInstance = Instantiate(cameraContainerPrefab, Vector3.zero, Quaternion.identity);

        cam = localCameraInstance.GetComponentInChildren<Camera>();
        cinemachineCamera = localCameraInstance.GetComponentInChildren<CinemachineCamera>();
        cinemachineBrain = localCameraInstance.GetComponentInChildren<CinemachineBrain>();
        inputController = cinemachineCamera.GetComponent<CinemachineInputAxisController>();

        cinemachineCamera.Follow = camFollow;
        cinemachineCamera.LookAt = camFollow;
        
        // Update sensitivity
        UpdateSensitivity();
        
        // Enable audio listener
        var audioListener = cam.GetComponent<AudioListener>();
        
        // Enable listener for the local player
        if (audioListener != null) audioListener.enabled = hasInputAuthority;
    }

    public void ComputeCameraRelativeWorldDirection(bool hasInputAuthority, ref NetworkInputData data)
    {
        if (hasInputAuthority && cam != null)
        {
            Vector3 camForward = cam.transform.forward;
            camForward.y = 0f;
            camForward.Normalize();

            Vector3 camRight = cam.transform.right;
            camRight.y = 0f;
            camRight.Normalize();

            Vector3 worldMove = camRight * data.RawInput.x + camForward * data.RawInput.y;
            data.MoveDirection = (worldMove.sqrMagnitude > 0.0001f) ? worldMove.normalized : Vector3.zero;
        }
        else
        {
            data.MoveDirection = Vector3.zero;
        }
    }

    public void UpdateSensitivity()
    {
        if (cinemachineCamera == null) return;

        float value = PlayerPrefs.GetFloat("MouseSensitivity", 1.0f);
        
        if (inputController != null)
        {
            inputController.Controllers[0].Input.Gain = value;
            inputController.Controllers[1].Input.Gain = value;
        }
    }

    public void Render(float localAlpha)
    {
        if (cinemachineBrain != null) cinemachineBrain.ManualUpdate();
        if (cinemachineCamera != null) cinemachineCamera.UpdateCameraState(Vector3.up, localAlpha);
    }

    public void DespawnCamera()
    {
        if (localCameraInstance != null)
        {
            Destroy(localCameraInstance);
            localCameraInstance = null;
        }
    }
}