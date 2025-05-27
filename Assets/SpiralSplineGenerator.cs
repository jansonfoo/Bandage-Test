using UnityEngine;
using UnityEngine.Splines;

[ExecuteAlways]
[RequireComponent(typeof(SplineContainer))]
public class SpiralSplineBuilder : MonoBehaviour
{
    [Header("Spiral Settings")]
    public float radius = 0.5f;
    public float height = 2f;
    public int turns = 5;
    public int pointsPerTurn = 20;

    [Header("Gap Settings")]
    public float gap = 0.02f; // Small gap to avoid clipping

    [Header("Target Cylinder (Optional)")]
    public Transform cylinder; // Use this to match spiral to cylinder height/scale

    public bool regenerateOnUpdate = true;

    private void OnValidate()
    {
        if (regenerateOnUpdate)
            GenerateSpiral();
    }

    [ContextMenu("Generate Spiral")]
    public void GenerateSpiral()
    {
        var splineContainer = GetComponent<SplineContainer>();
        splineContainer.Spline.Clear();

        if (cylinder != null)
        {
            height = cylinder.localScale.y;
            radius = Mathf.Max(cylinder.localScale.x, cylinder.localScale.z) * 0.5f + gap;
        }

        int totalPoints = turns * pointsPerTurn;

        for (int i = 0; i <= totalPoints; i++)
        {
            float t = (float)i / totalPoints;
            float angle = t * turns * 2 * Mathf.PI;

            // Position of knot
            Vector3 pos = new Vector3(
                radius * Mathf.Sin(angle),
                height * t,
                radius * Mathf.Cos(angle)
            );

            // Calculate derivative (tangent) of spiral at t
            // derivative of x = radius * cos(angle) * d(angle)/dt
            // derivative of y = height (linear)
            // derivative of z = -radius * sin(angle) * d(angle)/dt
            // d(angle)/dt = turns * 2 * PI

            float dAngleDt = turns * 2f * Mathf.PI;

            Vector3 tangent = new Vector3(
                radius * Mathf.Cos(angle) * dAngleDt,
                height,
                -radius * Mathf.Sin(angle) * dAngleDt
            ).normalized;

            // Handle length (a fraction of spacing between knots)
            float handleLength = (2 * Mathf.PI * radius) / pointsPerTurn * 0.33f;

            // Set handles along tangent
            Vector3 handleIn = -tangent * handleLength;
            Vector3 handleOut = tangent * handleLength;

            BezierKnot knot = new BezierKnot(
                transform.InverseTransformPoint(pos),
                transform.InverseTransformDirection(handleIn),
                transform.InverseTransformDirection(handleOut)
            );

            splineContainer.Spline.Add(knot);
        }

#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(splineContainer);
#endif
    }
}
