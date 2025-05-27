using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class BandageRibbonExtruder : MonoBehaviour
{
    public SplineContainer splineContainer;
    public float wrapSpeed = 0.5f;
    public float bandageWidth = 0.5f;
    public float uvTilingFactor = 5f;  // How many times the texture repeats along the length

    private MeshFilter meshFilter;
    private Mesh bandageMesh;

    private float totalLength;
    private float currentLength;

    private List<Vector3> vertices = new List<Vector3>();
    private List<int> triangles = new List<int>();
    private List<Vector2> uvs = new List<Vector2>();

    void Start()
    {
        meshFilter = GetComponent<MeshFilter>();
        bandageMesh = new Mesh();
        meshFilter.mesh = bandageMesh;

        totalLength = SplineUtility.CalculateLength(splineContainer.Spline, splineContainer.transform.localToWorldMatrix);
        currentLength = 0f;
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.Space))
        {
            currentLength += wrapSpeed * Time.deltaTime;
            if (currentLength > totalLength)
                currentLength = totalLength;

            UpdateMesh(currentLength);
        }
    }

    void UpdateMesh(float length)
    {
        bandageMesh.Clear();
        vertices.Clear();
        triangles.Clear();
        uvs.Clear();

        int segments = Mathf.CeilToInt(length / 0.01f);  // adjust resolution, smaller step for smoother mesh
        float segmentSpacing = length / segments;

        for (int i = 0; i <= segments; i++)
        {
            float distAlongSpline = i * segmentSpacing;
            float t = Mathf.Clamp01(distAlongSpline / totalLength);

            Vector3 localPos = splineContainer.Spline.EvaluatePosition(t);
            Vector3 worldPos = splineContainer.transform.TransformPoint(localPos);

            Vector3 localTangent = splineContainer.Spline.EvaluateTangent(t);
            Vector3 worldTangent = splineContainer.transform.TransformDirection(localTangent).normalized;

            // Calculate base normal perpendicular to tangent (using Vector3.up as reference)
            Vector3 baseNormal = Vector3.ProjectOnPlane(Vector3.up, worldTangent).normalized;

            Vector3 leftPos = worldPos - baseNormal * (bandageWidth * 0.5f);
            Vector3 rightPos = worldPos + baseNormal * (bandageWidth * 0.5f);

            vertices.Add(transform.InverseTransformPoint(leftPos));
            vertices.Add(transform.InverseTransformPoint(rightPos));

            float vCoord = distAlongSpline * uvTilingFactor;
            uvs.Add(new Vector2(0, vCoord));
            uvs.Add(new Vector2(1, vCoord));

            if (i < segments)
            {
                int baseIndex = i * 2;

                triangles.Add(baseIndex);
                triangles.Add(baseIndex + 2);
                triangles.Add(baseIndex + 1);

                triangles.Add(baseIndex + 1);
                triangles.Add(baseIndex + 2);
                triangles.Add(baseIndex + 3);
            }
        }


        bandageMesh.SetVertices(vertices);
        bandageMesh.SetTriangles(triangles, 0);
        bandageMesh.SetUVs(0, uvs);
        bandageMesh.RecalculateNormals();
    }
}
