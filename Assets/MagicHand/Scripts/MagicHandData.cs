using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class MagicHandData
{
    // key points in unity world coordinate system
    public List<Vector3> Keypoints { get; private set; }
    
    // key points in screen space
    public List<Vector3> KeypointsScreenSpace { get; private set; }
    
    public bool enabled = false;

    public bool PinchState { get; private set; }

    public MagicHandData(int keypointCount = 21)
    {
        Keypoints = new List<Vector3>(new Vector3[keypointCount]);
        KeypointsScreenSpace = new List<Vector3>(new Vector3[keypointCount]);
    }

    public void UpdateKeypoints(List<Vector3> newPoints, List<Vector3> pointsScreenSpace)
    {
       
        if (newPoints.Count != Keypoints.Count) return;
        for (int i = 0; i < newPoints.Count; i++)
            Keypoints[i] = newPoints[i];
        for (int i = 0;i < pointsScreenSpace.Count; i++)
            KeypointsScreenSpace[i] = pointsScreenSpace[i];
    }

    public void SetPinchState(bool state) => PinchState = state;

    public Vector3 GetKeypoint(int index) => Keypoints[index];
    public Vector3 GetKeypointScreenSpace(int index) => KeypointsScreenSpace[index];

    public Vector3 GetCenter()
    {
        Vector3 center = Vector3.zero;
        foreach (var p in Keypoints) center += p;
        return Keypoints.Count > 0 ? center / Keypoints.Count : Vector3.zero;
    }

    public bool IsAvailable() => Keypoints.Count == 21 && enabled; // could extend later

    public Vector3 GetKeyPointDiff(int index1, int index2) => Keypoints[index1] - Keypoints[index2];
}