using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Data for a single hand
/// </summary>
public class HandPoseData
{
    public const int KeypointCount = 21;
    // Keypoints using the MediaPipe hand landmark model
    // Unity coordinate systems, in meters.
    // Vector3.zero is the center of the screen, and z axis points towards the screen (z values are usually negative)
    public readonly Vector3[] Keypoints;
    public bool IsPinching = false;

    public HandPoseData()
    {
        Keypoints = new Vector3[KeypointCount];
    }
}

/// <summary>
/// A frame of hand pose data, potentially containing multiple hands.
/// </summary>
public class HandPoseFrameData
{
    public readonly List<HandPoseData> Hands = new List<HandPoseData>();
}