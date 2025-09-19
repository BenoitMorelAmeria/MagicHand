using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IHandPoseProvider
{
    /// <summary>
    /// Returns the latest hand pose frame data
    /// </summary>
    /// If no hand is detected, the returned frame will have zero hands.
    HandPoseFrameData GetLatestHandPoseFrame();
}
