using UnityEngine;

[ExecuteAlways]
public class SplineUILayout : MonoBehaviour
{
    [Header("Curve Control Points (Local Positions)")]
    public Vector2 p0 = new Vector2(-100, 300);
    public Vector2 p1 = new Vector2(50, 150);
    public Vector2 p2 = new Vector2(50, -150);
    public Vector2 p3 = new Vector2(-100, -300);

    [Header("Options")]
    public bool alignRotation = false;
    [Range(0f, 1f)] public float startT = 0f;
    [Range(0f, 1f)] public float endT = 1f;

    // Standard Bezier evaluation uses 't' (0 to 1), but 't' does NOT travel at a constant speed.
    // Points move faster where handles are stretched and slower where they are compressed.
    // We pre-sample the curve into a Look-Up Table (LUT) to map real pixel distance back to 't'.
    private const int LUT_SAMPLES = 100;
    private float[] arcLengths = new float[LUT_SAMPLES + 1];
    private float totalLength;

    
    void Update()
    {
        UpdateLayout();
    }

    public void UpdateLayout()
    {
        // Count only active children so hidden/disabled menu buttons don't leave empty gaps
        int childCount = 0;
        for (int i = 0; i < transform.childCount; i++)
        {
            if (transform.GetChild(i).gameObject.activeSelf)
                childCount++;
        }
        if (childCount == 0) return;

        // Recalculate distance points along the curve before placing elements
        BuildArcLengthLUT();

        // Start anchoring all active children
        int activeIndex = 0;
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            if (!child.gameObject.activeSelf) continue;

            if (child is RectTransform rect)
            {
                // Calculate where this button belongs as a percentage (0.0 to 1.0) along total path length
                float targetNormalizedDist = (childCount == 1) 
                    ? startT 
                    : Mathf.Lerp(startT, endT, (float)activeIndex / (childCount - 1));

                // Convert desired physical distance into the non-linear Bezier parameter 't'
                float t = GetTFromDistance(targetNormalizedDist * totalLength);
                Vector2 curvePos = EvaluateBezier(t);
                rect.localPosition = new Vector3(curvePos.x, curvePos.y, 0f);

                // Calculate the curve's slope at 't' to orient the button along the path direction
                if (alignRotation)
                {
                    Vector2 tangent = EvaluateTangent(t);
                    float angle = Mathf.Atan2(tangent.y, tangent.x) * Mathf.Rad2Deg;
                    rect.localRotation = Quaternion.Euler(0, 0, angle);
                }
            }
            activeIndex++;
        }
    }
    

    /// <summary>
    /// Slices the curve into 100 small linear segments to approximate cumulative curve length.
    /// arcLengths[i] stores total distance from P0 up to sample i.
    /// </summary>
    void BuildArcLengthLUT()
    {
        arcLengths[0] = 0f;
        Vector2 prev = EvaluateBezier(0f);
        totalLength = 0f;

        for (int i = 1; i <= LUT_SAMPLES; i++)
        {
            float t = (float)i / LUT_SAMPLES;
            Vector2 curr = EvaluateBezier(t);
            totalLength += Vector2.Distance(prev, curr);
            arcLengths[i] = totalLength;
            prev = curr;
        }
    }

    /// <summary>
    /// Uses binary search across the distance table to find which segment contains targetDistance,
    /// then linearly interpolates between those two samples to find the exact 't' value.
    /// </summary>
    float GetTFromDistance(float targetDistance)
    {
        if (targetDistance <= 0f) return 0f;
        if (targetDistance >= totalLength) return 1f;

        // Binary search to find the segment index containing our target distance
        int low = 0;
        int high = LUT_SAMPLES;
        int index = 0;

        while (low < high)
        {
            int mid = (low + high) / 2;
            if (arcLengths[mid] < targetDistance)
            {
                index = mid;
                low = mid + 1;
            }
            else
            {
                high = mid;
            }
        }

        // Figure out how far along we are inside this specific segment (0.0 to 1.0)
        float segmentStart = arcLengths[index];
        float segmentLength = arcLengths[index + 1] - segmentStart;
        float segmentFraction = (segmentLength > 0.0001f) ? (targetDistance - segmentStart) / segmentLength : 0f;

        // Map that segment fraction back into global 't' parameter space
        float tStart = (float)index / LUT_SAMPLES;
        float tEnd = (float)(index + 1) / LUT_SAMPLES;

        return Mathf.Lerp(tStart, tEnd, segmentFraction);
    }

    /// <summary>
    /// Standard cubic Bernstein polynomial: B(t) = (1-t)³P0 + 3(1-t)²tP1 + 3(1-t)t²P2 + t³P3
    /// </summary>
    Vector2 EvaluateBezier(float t)
    {
        float u = 1f - t;
        return (u * u * u * p0) +
               (3f * u * u * t * p1) +
               (3f * u * t * t * p2) +
               (t * t * t * p3);
    }

    /// <summary>
    /// First derivative of the cubic curve, returning the forward velocity/tangent vector at 't'.
    /// </summary>
    Vector2 EvaluateTangent(float t)
    {
        float u = 1f - t;
        return (3f * u * u * (p1 - p0)) +
               (6f * u * t * (p2 - p1)) +
               (3f * t * t * (p3 - p2));
    }

    
    void OnDrawGizmosSelected()
    {
        RectTransform rt = GetComponent<RectTransform>();
        if (!rt) return;

        Gizmos.color = Color.cyan;
        Vector3 prevPoint = rt.TransformPoint(EvaluateBezier(0f));

        for (int i = 1; i <= 30; i++)
        {
            float t = i / 30f;
            Vector3 currPoint = rt.TransformPoint(EvaluateBezier(t));
            Gizmos.DrawLine(prevPoint, currPoint);
            prevPoint = currPoint;
        }
    }
}