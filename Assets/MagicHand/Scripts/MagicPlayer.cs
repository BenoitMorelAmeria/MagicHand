using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class MagicPlayer : MonoBehaviour
{
    [SerializeField] MagicHand magicHand;
    [SerializeField] string fileName = "recordedHandData.json";
    [SerializeField] bool autoPlayOnStart = false;

    private HandRecording recording;
    private float playbackStartTime;
    private int currentFrame = 0;
    private bool isPlaying = false;
    private bool paused = false;

    void Start()
    {
        LoadRecording();
        if (autoPlayOnStart)
            Play();
    }

    void Update()
    {
        // Check for pause/resume keys
        if (Input.GetKeyDown(KeyCode.P))
        {
            paused = true;
            Debug.Log("Playback paused.");
        }
        else if (Input.GetKeyDown(KeyCode.Q))
        {
            paused = false;
            // Restart playback from beginning
            Play();
        }

        if (!isPlaying || paused || recording == null || recording.frames.Count == 0)
            return;

        float elapsed = Time.time - playbackStartTime;

        // Advance frames if time has passed
        while (currentFrame < recording.frames.Count &&
               recording.frames[currentFrame].time <= elapsed)
        {
            HandFrameData frame = recording.frames[currentFrame];

            if (frame.available)
            {
                magicHand.SetVisible(true);
                magicHand.UpdateHand(frame.keypoints);
                magicHand.SetHandPoseEnabled(true);
            }
            else
            {
                magicHand.SetVisible(false);
            }

            currentFrame++;
        }

        // End of playback
        if (currentFrame >= recording.frames.Count)
        {
            isPlaying = false;
            Debug.Log("Playback finished.");
        }
    }

    public void Play()
    {
        if (recording == null || recording.frames.Count == 0)
        {
            Debug.LogWarning("No recording loaded to play.");
            return;
        }
        magicHand.gameObject.SetActive(true);

        currentFrame = 0;
        playbackStartTime = Time.time;
        isPlaying = true;
        paused = false;
        Debug.Log("Playback started.");
    }

    private void LoadRecording()
    {
        string json = File.ReadAllText(fileName);
        recording = JsonUtility.FromJson<HandRecording>(json);

        Debug.Log($"Recording loaded: {recording.frames.Count} frames from {fileName}");
    }
}
