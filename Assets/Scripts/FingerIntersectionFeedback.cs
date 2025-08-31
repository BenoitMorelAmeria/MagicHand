using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FingerIntersectionFeedback : MonoBehaviour
{
    [SerializeField] MagicHand magicHand;
    [SerializeField] GameObject feedbackPrefab;
    [SerializeField] int fingerIndex = 0;
    [SerializeField] float zIntersection = 0;

    GameObject feedback;

    void Start()
    {
        feedback = Instantiate(feedbackPrefab);
    }

    // Update is called once per frame
    public void Update()
    {      
        MagicHandData data = magicHand.Data;
        bool doesIntersect = false;
        Vector3 intersection = Vector3.zero;
        for (int i = 0; i < 3; ++i)
        {
            Vector3 p1 = data.GetKeypoint(4 * fingerIndex + i + 1);
            Vector3 p2 = data.GetKeypoint(4 * fingerIndex + i + 2);
            doesIntersect |= IntersectZPlane(p1, p2, zIntersection, out intersection);
            if (doesIntersect) break;
        }
        feedback.SetActive(doesIntersect);
        if (doesIntersect)
        {
            Debug.Log("does intersect");
            feedback.transform.position = intersection;
        }

    }


    /// <summary>
    /// Checks if the segment [p1, p2] intersects the plane z = z0.
    /// If yes, returns true and outputs the intersection point.
    /// </summary>
    public static bool IntersectZPlane(Vector3 p1, Vector3 p2, float z0, out Vector3 intersection)
    {
        intersection = Vector3.zero;

        float z1 = p1.z;
        float z2 = p2.z;

        // Case 1: both points on the plane -> take p1 as intersection
        if (Mathf.Approximately(z1, z0) && Mathf.Approximately(z2, z0))
        {
            intersection = p1;
            return true;
        }

        // Case 2: segment crosses the plane?
        if ((z1 - z0) * (z2 - z0) > 0)
        {
            // both on same side, no intersection
            return false;
        }

        // Case 3: compute interpolation factor t
        float t = (z0 - z1) / (z2 - z1); // fraction along segment
        intersection = Vector3.Lerp(p1, p2, t);
        return true;
    }


}
