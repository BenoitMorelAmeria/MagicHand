using System.Collections;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class MagicHandGestures : MonoBehaviour
{
    [SerializeField] public MagicHand magicHand;
    [SerializeField] private float flatnessThreshold = 0.001f;
    [SerializeField] private float pointingFrontnessThreshold = 0.7f;
    [SerializeField] private float pointingOthersNonFrontnessThreshold = 0.0f;

    public string handSignednessDebug = "Unknown";

    // is the hand currently float?
    public bool IsHandFlat = false;

    // how long has the hand been flat (in seconds)
    public float flatHandDuration = 0f;

    public Vector3 palmNormal = Vector3.up;
    public Vector3 palmRight = Vector3.right;
    public Vector3 palmForward = Vector3.forward;
    public Quaternion palmRotation = Quaternion.identity;

    public enum Handedness { Left, Right }
    Handedness HandednessDetected = Handedness.Right;

    public List<float> fingerColinearities = new List<float>() { 0f, 0f, 0f, 0f, 0f };
    public List<float> fingerFrontness = new List<float>() { 0f, 0f, 0f, 0f, 0f };

    public bool IndexPointing = false;
    public bool IsVictory = false;
    public bool IsSpiderMan = false;
    public bool IsThumbUp = false;

    // cut gesture: triggered when:
    // - hand is victory pose
    // - index and middle fingers are far enough from each other
    // - then they come close enough again
    [SerializeField] public float cutMinStartingDistance = 0.5f; 
    [SerializeField] public float cutTriggerDistanceThreshold = 0.5f;
    enum CutGestureState { NoVictory, // no victory pose 
        VictoryButTooClose, // victory, but movement hasnt started yet 
        VictoryButTooFar // victory, movement started, waiting for fingers to come close enough
    }
    CutGestureState cutGestureState;

    public event Action OnCutGesture;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!magicHand.IsAvailable())
        {
            IsHandFlat = false;
            return;
        }
        List<Vector3> keypoints = magicHand.GetCurrentKeyPoints();
        float flatness = ComputeFlatness(keypoints);
        IsHandFlat = flatness < flatnessThreshold;
        if (IsHandFlat)
            flatHandDuration += Time.deltaTime;
        else
            flatHandDuration = 0f;
        if (!IsHandFlat)
        {
            HandednessDetected = DetectHandedness(keypoints);
        }
        IsThumbUp = ComputeThumbUp();
        handSignednessDebug = HandednessDetected.ToString();
        palmNormal = GetPalmNormal(keypoints, HandednessDetected);
        palmRight = GetPalmRight(keypoints, HandednessDetected);
        palmForward = Vector3.Cross(palmRight, palmNormal).normalized;
        palmRotation = Quaternion.LookRotation(palmForward, palmNormal);
        UpdateFingersColinearity();
        UpdateFingersFrontness();
        IndexPointing = ComputeIsIndexFingerPointing();
        IsVictory = ComputeIsVictory();
        IsSpiderMan = ComputeIsSpiderMan();
        UpdateCut();
    }

    public void UpdateCut()
    {
        if (!IsVictory) {
            cutGestureState = CutGestureState.NoVictory;
            return;
        }

        if (cutGestureState == CutGestureState.NoVictory)
        {
            cutGestureState = CutGestureState.VictoryButTooClose; 
        }

        float fingerDistance = Vector3.Distance(magicHand.GetKeyPoint(8), magicHand.GetKeyPoint(12));
        if (cutGestureState == CutGestureState.VictoryButTooClose)
        {
            if (fingerDistance > cutMinStartingDistance)
            {
                cutGestureState = CutGestureState.VictoryButTooFar;
            }
        }

        if (cutGestureState == CutGestureState.VictoryButTooFar)
        {
            if (fingerDistance < cutTriggerDistanceThreshold)
            {
                // trigger cut
                OnCutGesture?.Invoke();
                cutGestureState = CutGestureState.VictoryButTooClose;
                Debug.Log("cut");
            }
        }
        //Debug.Log("finger distance: " + fingerDistance + " state: " + cutGestureState);
    }

    public static float ComputeFlatness(List<Vector3> points)
    {
        if (points == null || points.Count < 3)
            return 0f;

        // 1. Compute centroid
        Vector3 centroid = Vector3.zero;
        foreach (var p in points) centroid += p;
        centroid /= points.Count;

        // 2. Build covariance matrix
        float xx = 0, xy = 0, xz = 0;
        float yy = 0, yz = 0, zz = 0;

        foreach (var p in points)
        {
            Vector3 d = p - centroid;
            xx += d.x * d.x;
            xy += d.x * d.y;
            xz += d.x * d.z;
            yy += d.y * d.y;
            yz += d.y * d.z;
            zz += d.z * d.z;
        }

        // covariance matrix (symmetric)
        var cov = new float[3, 3] {
            { xx, xy, xz },
            { xy, yy, yz },
            { xz, yz, zz }
        };

        // 3. Find eigenvalues (we only care about the smallest one).
        // Unity doesn’t have a built-in eigen solver, so:
        // - You can use a small numerical solver (e.g. power iteration) OR
        // - Use a plugin like Math.NET Numerics for proper eigen decomposition.

        // Placeholder: naive "flatness" using determinant / trace
        // (works as a proxy, but eigen decomposition is best).
        float trace = xx + yy + zz;
        float det =
            xx * (yy * zz - yz * yz) -
            xy * (xy * zz - xz * yz) +
            xz * (xy * yz - yy * xz);

        // Flatness score = determinant / (trace^3) as rough approximation
        return det / (trace * trace * trace + Mathf.Epsilon);
    }


    public static Handedness DetectHandedness(List<Vector3> keypoints)
    {
        if (keypoints == null || keypoints.Count < 21)
            return Handedness.Left;

        Vector3 wrist = keypoints[0];
        Vector3 indexBase = keypoints[5];
        Vector3 pinkyBase = keypoints[17];
        Vector3 thumbBase = keypoints[1];

        // Palm normal
        Vector3 palmVector1 = indexBase - wrist;
        Vector3 palmVector2 = pinkyBase - wrist;
        Vector3 palmNormal = Vector3.Cross(palmVector1, palmVector2).normalized;

        // Thumb direction relative to wrist
        Vector3 thumbDir = (thumbBase - wrist).normalized;

        // Dot product
        float dot = Vector3.Dot(palmNormal, thumbDir);

        return dot < 0 ? Handedness.Right : Handedness.Left;
    }

    public static Vector3 GetPalmNormal(List<Vector3> keypoints, Handedness handedness)
    {
        if (keypoints == null || keypoints.Count < 21)
            return Vector3.zero;

        Vector3 wrist = keypoints[0];
        Vector3 indexBase = keypoints[5];  // MCP joint
        Vector3 pinkyBase = keypoints[17]; // MCP joint

        // Compute raw palm normal
        Vector3 v1 = indexBase - wrist;
        Vector3 v2 = pinkyBase - wrist;
        Vector3 normal = Vector3.Cross(v1, v2).normalized;

        // Orient using handedness
        // Cross product order makes it consistent with a right-handed coordinate system,
        // so we just flip for left hand.
        if (handedness == Handedness.Right)
            normal = -normal;

        return normal;
    }

    public static Vector3 GetPalmRight(List<Vector3> keypoints, Handedness handedness)
    {
        if (keypoints == null || keypoints.Count < 21)
            return Vector3.zero;

        Vector3 wrist = keypoints[0];
        Vector3 indexBase = keypoints[5];   // MCP
        Vector3 pinkyBase = keypoints[17];  // MCP

        // Palm normal (same as your function)
        Vector3 v1 = indexBase - wrist;
        Vector3 v2 = pinkyBase - wrist;
        Vector3 palmNormal = Vector3.Cross(v1, v2).normalized;
        if (handedness == Handedness.Right)
            palmNormal = -palmNormal;

        // Raw right vector = indexBase -> pinkyBase
        Vector3 rawRight = (pinkyBase - indexBase).normalized;

        // Project onto palm plane to ensure orthogonality
        Vector3 right = Vector3.ProjectOnPlane(rawRight, palmNormal).normalized;

        // Flip direction so "right" always points toward the thumb
        if (handedness == Handedness.Left)
            right = -right;

        return right;
    }


    public float ComputeColinearity(List<Vector3> vector3s)
    {
        if (vector3s == null || vector3s.Count < 2)
            return 0f;
        Vector3 first = vector3s[0];
        Vector3 last = vector3s[vector3s.Count - 1];
        Vector3 lineDir = (last - first).normalized;
        float totalDistance = 0f;
        for (int i = 1; i < vector3s.Count - 1; i++)
        {
            Vector3 point = vector3s[i];
            Vector3 toPoint = point - first;
            float projectionLength = Vector3.Dot(toPoint, lineDir);
            Vector3 projection = first + projectionLength * lineDir;
            float distance = Vector3.Distance(point, projection);
            totalDistance += distance;
        }
        return totalDistance / (vector3s.Count - 2);
    }

    private void UpdateFingersColinearity()
    {
        for (int i = 0; i < 5; i++)
        {
            List<Vector3> fingerPoints = GetFingerPoints(i);
            float colinearity = ComputeColinearity(fingerPoints);
            fingerColinearities[i] = colinearity;
        }
    }

    private void UpdateFingersFrontness()
    {
        for (int i = 0; i < 5; i++)
        {
            float frontness = ComputeFingerFrontness(i);
            fingerFrontness[i] = frontness;
        }
    }


    private bool ComputeIsIndexFingerPointing()
    {
        if (!magicHand.IsAvailable())
            return false;
        bool pointing = true;
        if (fingerFrontness[1] < pointingFrontnessThreshold)
            pointing = false;
        for (int i = 2; i < 5; i++)
        {
            if (fingerFrontness[i] > pointingOthersNonFrontnessThreshold)
                pointing = false;
        }
        return pointing;
    }

    private bool ComputeIsSpiderMan()
    {
        if (!magicHand.IsAvailable())
            return false;
        if (fingerFrontness[1] < 0.0f)
            return false;
        if (fingerFrontness[2] > 0.0f)
            return false;
        if (fingerFrontness[3] > 0.0f)
            return false;
        if (fingerFrontness[4] < 0.0f)
            return false;
        return true;
    }

    private bool ComputeThumbUp()
    {
        if (!magicHand.IsAvailable())
            return false;
        if (fingerFrontness[0] < 0.75f)
            return false;
        for (int i = 1; i < 5; ++i)
        {
            if (fingerFrontness[i] > -0.2f)
                return false;
        }
        return true;
    }

    private float ComputeFingerFrontness(int fingerIndex)
    {
        Vector3 baseOrientation = magicHand.GetKeyPoint(0) - magicHand.GetKeyPoint(1 + fingerIndex * 4);
        Vector3 fingerOrientation = magicHand.GetKeyPoint(2 + fingerIndex * 4) - magicHand.GetKeyPoint(4 + fingerIndex * 4);
        return Vector3.Dot(baseOrientation.normalized, fingerOrientation.normalized);
    }

    private bool ComputeIsVictory()
    { 

        for (int i = 1; i < 3; ++i)
        {
            if (fingerFrontness[i] < 0.0f)
                return false;
        }
        for (int i = 3; i < 5; ++i)
        {
            if (fingerFrontness[i] > 0.0f)
                return false;
        }
        return true;
    }

    public List<Vector3> GetFingerPoints(int fingerIndex)
    {
        if (fingerIndex < 0 || fingerIndex > 4)
            return null;
        List<Vector3> keypoints = magicHand.GetCurrentKeyPoints();
        if (keypoints == null || keypoints.Count < 21)
            return null;
        int startIdx = 1 + fingerIndex * 4;
        return keypoints.GetRange(startIdx, 4);
    }
}
