using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagicTranslation : MonoBehaviour
{
    [SerializeField] Transform objectToTransform;
    [SerializeField] MagicHandGestures magicHandGestures;
    [SerializeField] Vector3 palmOrientationToStart = Vector3.right;

    // how long the hand should stay flat to trigger the translation
    [SerializeField] float flatHandDurationThreshold = 0.5f;
    // how aligned the hand should be with the camera right or left vectors to trigger the translation
    [SerializeField] float handOrientationThreshold = 0.85f;

    [SerializeField] bool transformInProgress = false;
    Vector3 lastPointerPos = Vector3.zero;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        bool shouldStartTranslation = magicHandGestures.IsHandFlat && magicHandGestures.flatHandDuration > flatHandDurationThreshold;
        shouldStartTranslation &= Mathf.Abs(Vector3.Dot(magicHandGestures.palmNormal, palmOrientationToStart)) > handOrientationThreshold;
        shouldStartTranslation &= !transformInProgress;
        if (shouldStartTranslation)
        {
            StartTranslation();
        }
        bool shouldStopTranslation = !magicHandGestures.IsHandFlat && transformInProgress;
        shouldStopTranslation |= !magicHandGestures.magicHand.IsAvailable();
        if (shouldStopTranslation)
        {
            StopTranslation();
        }
        if (transformInProgress)
        {
            UpdateTranslation();
        }
    }

    void StartTranslation()
    {
        transformInProgress = true;
        lastPointerPos = GetPointerPosition();
    }

    void StopTranslation()
    {
        transformInProgress = false;
    }

    private void UpdateTranslation()
    {
        if (transformInProgress)
        {
            Vector3 currentPointerPos = GetPointerPosition();
            Vector3 delta = currentPointerPos - lastPointerPos;
            objectToTransform.position += delta;
            lastPointerPos = currentPointerPos;
        }

    }
    private Vector3 GetPointerPosition()
    {
        return magicHandGestures.magicHand.GetCenter();
    }

}
