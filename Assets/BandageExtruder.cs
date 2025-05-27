using UnityEngine;
using UnityEngine.Splines;

public class BandageExtruder : MonoBehaviour
{
    public GameObject bandagePrefab;
    public SplineContainer spline;
    public float segmentSpacing = 0.05f;
    public float wrapSpeed = 0.5f;

    private float currentLength = 0f;
    private float totalLength;

    void Start()
    {
        totalLength = SplineUtility.CalculateLength(spline.Spline, spline.transform.localToWorldMatrix);
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.Space))
        {
            currentLength = Mathf.Min(currentLength + wrapSpeed * Time.deltaTime, totalLength);
            float step = segmentSpacing;

            for (float i = 0; i < currentLength; i += step)
            {
                float t = Mathf.Clamp01(i / totalLength);
                if (!HasSegmentAt(t))
                {
                    PlaceSegment(t);
                }
            }
        }
    }

    void PlaceSegment(float t)
    {
        Vector3 localPos = spline.Spline.EvaluatePosition(t);
        Vector3 worldPos = spline.transform.TransformPoint(localPos);

        Vector3 localTangent = spline.Spline.EvaluateTangent(t);
        Vector3 worldTangent = spline.transform.TransformDirection(localTangent);

        Quaternion rot = Quaternion.LookRotation(worldTangent);

        // Rotate bandage upward so it wraps vertically
        Quaternion offset = Quaternion.Euler(90, 0, 90);
        rot *= offset;

        GameObject segment = Instantiate(bandagePrefab, worldPos, rot, transform);
        segment.name = $"Segment_{t:F2}";
        segment.transform.localScale = new Vector3(0.05f, 0.05f, 0.05f);
    }



    bool HasSegmentAt(float t)
    {
        string name = $"Segment_{t:F2}";
        return transform.Find(name) != null;
    }
}
