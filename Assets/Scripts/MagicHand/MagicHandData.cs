using System.Collections.Generic;
using UnityEngine;

public class MagicHandData
{
    public List<Vector3> Keypoints { get; private set; }
    [SerializeField] public List<Vector2Int> jointPairs = new List<Vector2Int>();

    public bool PinchState { get; private set; }

    public MagicHandData(int keypointCount = 21)
    {
        Keypoints = new List<Vector3>(new Vector3[keypointCount]);
    }

    public void UpdateKeypoints(List<Vector3> newPoints)
    {
        if (newPoints.Count != Keypoints.Count) return;
        for (int i = 0; i < newPoints.Count; i++)
            Keypoints[i] = newPoints[i];
    }

    public void SetPinchState(bool state) => PinchState = state;

    public Vector3 GetKeypoint(int index) => Keypoints[index];

    public Vector3 GetCenter()
    {
        Vector3 center = Vector3.zero;
        foreach (var p in Keypoints) center += p;
        return Keypoints.Count > 0 ? center / Keypoints.Count : Vector3.zero;
    }

    public bool IsAvailable() => Keypoints.Count == 21; // could extend later

    public Vector3 GetKeyPointDiff(int index1, int index2) => Keypoints[index1] - Keypoints[index2];
}