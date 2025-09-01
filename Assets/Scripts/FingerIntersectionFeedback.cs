using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FingerIntersectionFeedback : MonoBehaviour
{
    [SerializeField] MagicHandGestures magicHandGestures;
    [SerializeField] GameObject feedbackPrefab;
    [SerializeField] int fingerIndex = 0; // 0 = thumb, 1 = index, 2 = middle, 3 = ring, 4 = pinky, <0 = all fingers
    [SerializeField] float zIntersection = 0;
    [SerializeField] bool onlyIfFingerOpen = false;

    List<Vector2Int> joinsToCheck = new List<Vector2Int>();

    List<GameObject> feedbacks = new List<GameObject>();

    void Start()
    {
        if (fingerIndex >= 0 && fingerIndex < 5)
        {
            joinsToCheck.Add(new Vector2Int(4 * fingerIndex + 1, 4 * fingerIndex + 2));
            joinsToCheck.Add(new Vector2Int(4 * fingerIndex + 2, 4 * fingerIndex + 3));
            joinsToCheck.Add(new Vector2Int(4 * fingerIndex + 3, 4 * fingerIndex + 4));
        }
        else
        {
            foreach (var jp in magicHandGestures.magicHand.jointPairs)
            {
                joinsToCheck.Add(jp);
            }
        }

        for (int i = 0; i < joinsToCheck.Count; ++i)
        {
            feedbacks.Add(Instantiate(feedbackPrefab, transform));
        }
    }

    // Update is called once per frame
    public void Update()
    {    
        if (magicHandGestures == null)
        {
            Debug.Log("null gestures");
            return;
        }
        if (magicHandGestures.magicHand == null)
        {
            Debug.Log("null magic");
            return;
        }
        bool cancelRendering = !magicHandGestures.magicHand.IsAvailable();
        if (!cancelRendering && onlyIfFingerOpen && 0 <= fingerIndex && fingerIndex < 5)
        {
            cancelRendering |= magicHandGestures.fingerFrontness[fingerIndex] < 0.0f;
        }

        if (cancelRendering)
        {
            foreach (var fb in feedbacks)
                fb.SetActive(false);
            return;
        }


        MagicHandData data = magicHandGestures.magicHand.Data;
        foreach (var join in joinsToCheck)
        {
            bool doesIntersect = false;
            Vector3 intersection = Vector3.zero;
            Vector3 p1 = data.GetKeypoint(join.x);
            Vector3 p2 = data.GetKeypoint(join.y);
            doesIntersect |= IntersectZPlane(p1, p2, zIntersection, out intersection);
            GameObject feedback = feedbacks[joinsToCheck.IndexOf(join)];
            feedback.SetActive(doesIntersect);
            if (doesIntersect)
            {
                feedback.transform.position = intersection;
            }
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
