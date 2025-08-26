using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagicRotationTranslation : MonoBehaviour
{
    [SerializeField] Transform objectToTransform;
    [SerializeField] MagicHandGestures magicHandGestures;
    [SerializeField] Vector3 palmOrientationToStart = Vector3.down;

    [SerializeField] float flatHandDurationThreshold = 0.5f;
    [SerializeField] float handOrientationThreshold = 0.85f;

    private bool transformInProgress = false;

    private Quaternion initialHandRotation;
    private Vector3 initialHandCenter;

    private Quaternion objectLocalRotation;
    private Vector3 objectLocalPosition;

    void Update()
    {
        bool shouldStartTransform = magicHandGestures.IsHandFlat &&
                                    magicHandGestures.flatHandDuration > flatHandDurationThreshold &&
                                    Vector3.Dot(magicHandGestures.palmNormal, palmOrientationToStart) > handOrientationThreshold &&
                                    !transformInProgress;

        if (shouldStartTransform)
        {
            StartTransform();
        }

        bool shouldStopTransform = !magicHandGestures.IsHandFlat && transformInProgress;
        shouldStopTransform |= !magicHandGestures.magicHand.IsAvailable();

        if (shouldStopTransform)
        {
            StopTransform();
        }

        if (transformInProgress)
        {
            UpdateTransform();
        }
    }

    public void StartTransform()
    {
        transformInProgress = true;

        // Build a consistent hand orientation from tracking data
        // palmNormal = forward, handRight = secondary axis
        Vector3 palmNormal = magicHandGestures.palmNormal.normalized;
        Vector3 handRight = magicHandGestures.palmRight.normalized; // <-- your API should give you this
        Vector3 handUp = Vector3.Cross(palmNormal, handRight).normalized;

        initialHandRotation = Quaternion.LookRotation(palmNormal, handUp);
        initialHandCenter = magicHandGestures.magicHand.GetCenter();

        // Object's pose relative to the hand
        objectLocalPosition = Quaternion.Inverse(initialHandRotation) * (objectToTransform.position - initialHandCenter);
        objectLocalRotation = Quaternion.Inverse(initialHandRotation) * objectToTransform.rotation;
    }

    public void StopTransform()
    {
        transformInProgress = false;
    }

    public void UpdateTransform()
    {
        // Rebuild the same hand orientation consistently
        Vector3 palmNormal = magicHandGestures.palmNormal.normalized;
        Vector3 handRight = magicHandGestures.palmRight.normalized;
        Vector3 handUp = Vector3.Cross(palmNormal, handRight).normalized;

        Quaternion currentHandRotation = Quaternion.LookRotation(palmNormal, handUp);
        Vector3 currentHandCenter = magicHandGestures.magicHand.GetCenter();

        // Transform object back into world space
        objectToTransform.position = currentHandCenter + currentHandRotation * objectLocalPosition;
        objectToTransform.rotation = currentHandRotation * objectLocalRotation;
    }
}
