using UnityEngine;

public class BarrierSection : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Renderer barrierRenderer;

    [Header("Visibility Settings")]
    [SerializeField] private float revealDistance = 5f;
    [SerializeField] private float fadeSpeed = 5f;

    [Header("Debug")]
    [SerializeField] private bool useManualVisibility = false;
    [SerializeField, Range(0f, 1f)] private float visibility = 0f;

    private Transform player;
    private Material materialInstance;
    private static readonly int VisibilityID = Shader.PropertyToID("_Visibility");
    private Collider barrierCollider;
    
    private void Awake()
    {
        materialInstance = Instantiate(barrierRenderer.material);
        barrierRenderer.material = materialInstance;
        barrierCollider = GetComponent<Collider>();
    }

    public void InitializeBarrierSections(Transform localPlayer)
    {
        player = localPlayer;
    }

    private void Update()
    {
        if (useManualVisibility)
        {
            //Use inspector slider directly
            SetVisibility(visibility);
            return;
        }

        if (player == null || barrierCollider == null) return;

        // This finds the point on the barrier's edge closest to the player
        Vector3 closestPointOnBarrier = barrierCollider.ClosestPoint(player.position);
        float distance = Vector3.Distance(player.position, closestPointOnBarrier);

        // Distance logic should work now since we're no longer using the pivot of the barrier (which was higher up)
        float targetVisibility = Mathf.InverseLerp(revealDistance, 0f, distance);
        visibility = Mathf.Lerp(visibility, targetVisibility, Time.deltaTime * fadeSpeed);
    
        SetVisibility(visibility);
    }

    private void SetVisibility(float value)
    {
        materialInstance.SetFloat(VisibilityID, value);
    }
}