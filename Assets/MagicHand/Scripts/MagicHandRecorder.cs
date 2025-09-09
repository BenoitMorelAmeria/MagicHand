using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[Serializable]
public class HandFrameData
{
    public float time;          // seconds since recording start
    public bool available;      // is the hand available
    public List<Vector3> keypoints; // hand keypoints (if available)
}

[Serializable]
public class HandRecording
{
    public List<HandFrameData> frames = new List<HandFrameData>();
}

public class MagicHandRecorder : MonoBehaviour
{
    [SerializeField] MagicHand magicHand;
    [SerializeField] string filePath = "Assets/recordedHandData.json";

    private HandRecording recording;
    private float startTime;
    private bool isRecording = false;

    void Start()
    {
        recording = new HandRecording();
        startTime = Time.time;
        isRecording = true; // auto-start recording
    }

    void Update()
    {
        if (!isRecording) return;

        HandFrameData frame = new HandFrameData();
        frame.time = Time.time - startTime;
        frame.available = magicHand.IsAvailable();

        if (frame.available)
        {
            frame.keypoints = new List<Vector3>(magicHand.GetCurrentKeyPoints());
        }
        else
        {
            frame.keypoints = new List<Vector3>(); // empty if not available
        }

        recording.frames.Add(frame);

        // Example: Stop & Save with key press
        if (Input.GetKeyDown(KeyCode.S))
        {
            SaveRecording();
            Debug.Log("Recording saved to " + filePath);
        }
    }

    void SaveRecording()
    {
        string json = JsonUtility.ToJson(recording, true);
        File.WriteAllText(filePath, json);
    }
}
