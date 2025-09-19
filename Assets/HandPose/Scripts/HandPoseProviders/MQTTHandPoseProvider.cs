using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MQTTHandPoseProvider : MonoBehaviour, IHandPoseProvider
{
    HandPoseFrameData latestFrame = new HandPoseFrameData();

    void Start()
    {
        MqttHandPose.OnKeypointsReceived += UpdateHand;
        MqttHandPose.OnHandPoseDetected += UpdateHandPoseDetected;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void UpdateHand(List<HandKeypoints> inputHands)
    {
        // for now we only support one hand and we take the first one
        if (inputHands.Count > 0)
        {
            latestFrame = new HandPoseFrameData();
            HandPoseData handData = new HandPoseData();
            for (int i = 0; i < handData.Keypoints.Length; i++)
            {
                handData.Keypoints[i] = inputHands[0].Keypoints[i];
            }
            latestFrame.Hands.Add(handData);
        }
    }

    public void UpdateHandPoseDetected(bool detected)
    {
        if (!detected)
        {
            // erase the latest frame
            latestFrame = new HandPoseFrameData();
        }
    }

    public HandPoseFrameData GetLatestHandPoseFrame()
    {
        return latestFrame;
    }
}
