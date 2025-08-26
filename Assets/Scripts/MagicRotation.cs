using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagicRotation : MonoBehaviour
{
    [SerializeField] Transform objectToTransform;
    [SerializeField] MagicHandGestures magicHandGestures;
    [SerializeField] Vector3 palmOrientationToStart = Vector3.down;

    // how long the hand should stay flat to trigger the translation
    [SerializeField] float flatHandDurationThreshold = 0.5f;
    // how aligned the hand should be with the camera right or left vectors to trigger the translation
    [SerializeField] float handOrientationThreshold = 0.85f;

    [SerializeField] bool transformInProgress = false;
    [SerializeField] bool rotateAroundHand = false;

    private Vector3 lastPalmOrientation;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        bool shouldStartRotation = magicHandGestures.IsHandFlat && magicHandGestures.flatHandDuration > flatHandDurationThreshold;
        shouldStartRotation &= Vector3.Dot(magicHandGestures.palmNormal, palmOrientationToStart) > handOrientationThreshold;
        shouldStartRotation &= !transformInProgress;
        if (shouldStartRotation)
        {
            StartRotation();
        }
        bool shouldStopRotation = !magicHandGestures.IsHandFlat && transformInProgress;
        shouldStopRotation |= !magicHandGestures.magicHand.IsAvailable();
        if (shouldStopRotation)
        {
            StopRotation();
        }
        if (transformInProgress)
        {
            UpdateRotation();
        }
    }

    public void StartRotation()
    {
        transformInProgress = true;
        lastPalmOrientation = magicHandGestures.palmNormal;
    }

    public void StopRotation()
    {
        transformInProgress = false;
    }

    public void UpdateRotation()
    {
        Vector3 rotationCenter = magicHandGestures.magicHand.GetCenter();
        if (!rotateAroundHand)
        {
            rotationCenter = Vector3.zero;
        }
        Vector3 palmOrientation = magicHandGestures.palmNormal;
        Quaternion rotation = Quaternion.FromToRotation(lastPalmOrientation, palmOrientation);


        // Rotate position around center
        Vector3 dir = objectToTransform.transform.position - rotationCenter;       // offset from pivot
        dir = rotation * dir;                                 // rotate the offset
        objectToTransform.transform.position = rotationCenter + dir;               // new position

        // Rotate object itself
        objectToTransform.transform.rotation = rotation * objectToTransform.transform.rotation;
        lastPalmOrientation = magicHandGestures.palmNormal;

    }
}
