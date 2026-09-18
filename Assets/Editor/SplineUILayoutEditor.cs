using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SplineUILayout))]
public class SplineUILayoutEditor : Editor
{
    void OnSceneGUI()
    {
        SplineUILayout layout = (SplineUILayout)target;
        RectTransform rt = layout.GetComponent<RectTransform>();
        if (!rt) return;

        EditorGUI.BeginChangeCheck();

        Vector3 wp0 = Handles.PositionHandle(rt.TransformPoint(layout.p0), Quaternion.identity);
        Vector3 wp1 = Handles.PositionHandle(rt.TransformPoint(layout.p1), Quaternion.identity);
        Vector3 wp2 = Handles.PositionHandle(rt.TransformPoint(layout.p2), Quaternion.identity);
        Vector3 wp3 = Handles.PositionHandle(rt.TransformPoint(layout.p3), Quaternion.identity);

        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(layout, "Modify Spline Layout Points");
            layout.p0 = rt.InverseTransformPoint(wp0);
            layout.p1 = rt.InverseTransformPoint(wp1);
            layout.p2 = rt.InverseTransformPoint(wp2);
            layout.p3 = rt.InverseTransformPoint(wp3);
            layout.UpdateLayout();
        }
    }
}