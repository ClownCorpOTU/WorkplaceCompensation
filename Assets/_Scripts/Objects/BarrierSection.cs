using UnityEngine;

public class BarrierSection : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private Renderer barrierRenderer;

    [Header("Visibility Settings")]
    [SerializeField] private float revealDistance = 5f;
    [SerializeField] private float fadeSpeed = 5f;

    [Header("Debug")]
    [SerializeField] private bool useManualVisibility = false;
    [SerializeField, Range(0f, 1f)] private float visibility = 0f;

    private Material materialInstance;

    private static readonly int VisibilityID = Shader.PropertyToID("_Visibility");

    private void Awake()
    {
        materialInstance = Instantiate(barrierRenderer.material);
        barrierRenderer.material = materialInstance;
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

        if (player == null) return;

        float distance = Vector3.Distance(player.position, transform.position);

        float targetVisibility = Mathf.InverseLerp(revealDistance, 0f, distance);

        visibility = Mathf.Lerp(visibility, targetVisibility, Time.deltaTime * fadeSpeed);

        SetVisibility(visibility);
    }

    private void SetVisibility(float value)
    {
        materialInstance.SetFloat(VisibilityID, value);
    }
}