using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagicTranslation : MonoBehaviour
{
    [SerializeField] Transform objectToTranslate;
    [SerializeField] MagicHandGestures magicHandGestures;

    // how long the hand should stay flat to trigger the translation
    [SerializeField] float flatHandDurationThreshold = 0.5f;
    // how aligned the hand should be with the camera right or left vectors to trigger the translation
    [SerializeField] float handOrientationThreshold = 0.85f;

    [SerializeField] bool translatingInProgress = false;
    Vector3 lastPointerPos = Vector3.zero;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        bool shouldStartTranslation = magicHandGestures.IsHandFlat && magicHandGestures.flatHandDuration > flatHandDurationThreshold;
        shouldStartTranslation &= Mathf.Abs(Vector3.Dot(magicHandGestures.palmNormal, Vector3.right)) > handOrientationThreshold;
        shouldStartTranslation &= !translatingInProgress;
        if (shouldStartTranslation)
        {
            StartTranslation();
        }
        bool shouldStopTranslation = !magicHandGestures.IsHandFlat && translatingInProgress;
        if (shouldStopTranslation)
        {
            StopTranslation();
        }
        if (translatingInProgress)
        {
            UpdateTranslation();
        }
    }

    void StartTranslation()
    {
        translatingInProgress = true;
        lastPointerPos = GetPointerPosition();
    }

    void StopTranslation()
    {
        translatingInProgress = false;
    }

    private void UpdateTranslation()
    {
        if (translatingInProgress)
        {
            Vector3 currentPointerPos = GetPointerPosition();
            Vector3 delta = currentPointerPos - lastPointerPos;
            objectToTranslate.position += delta;
            lastPointerPos = currentPointerPos;
        }

    }
    private Vector3 GetPointerPosition()
    {
        return magicHandGestures.magicHand.GetCenter();
    }

}
