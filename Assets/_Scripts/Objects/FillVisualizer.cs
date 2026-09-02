using UnityEngine;

public class FillVisualizer : MonoBehaviour
{
    [Header("Visual Settings")]
    [SerializeField] private Transform fillPivot;
    [SerializeField] private MeshRenderer fillRend;
    [SerializeField] private Gradient fillGradient;
    
    // Using the Axis enum already defined in NetworkFossilScanner.cs
    [SerializeField] private Axis scalingAxis = Axis.Z;
    
    private Vector3 originalScale;

    private void Awake()
    {
        if (fillPivot != null)
        {
            originalScale = fillPivot.localScale;
        }
    }

    /// <summary>
    /// Updates the visual scale and color of the fill bar.
    /// </summary>
    /// <param name="fillPercent">A value between 0 and 1.</param>
    public void UpdateVisuals(float fillPercent)
    {
        if (fillPivot == null) return;

        // Clamp the percent between 0 and 1, and ensure a minimum scale so lighting doesn't break
        fillPercent = Mathf.Clamp01(fillPercent);
        float scalePercent = Mathf.Max(0.01f, fillPercent);

        // Scale bar on the needed axis
        Vector3 scale = originalScale;

        switch (scalingAxis)
        {
            case Axis.X:
                scale.x *= scalePercent;
                break;
            case Axis.Y:
                scale.y *= scalePercent;
                break;
            case Axis.Z:
                scale.z *= scalePercent;
                break;
        }

        fillPivot.localScale = scale;
        
        // Change color based on the gradient
        if (fillRend != null)
        {
            Color barColor = fillGradient.Evaluate(fillPercent);
            fillRend.material.SetColor("_BaseColor", barColor);
            fillRend.material.SetColor("_EmissionColor", barColor * (2f * fillPercent));
        }
    }
}