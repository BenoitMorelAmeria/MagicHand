using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagicTransform : MonoBehaviour
{
    [SerializeField] Transform objectToTransform;
    [SerializeField] MagicHandGestures magicHandGestures;
    [SerializeField] Vector3 palmOrientationToStart = Vector3.down;

    [SerializeField] float flatHandDurationThreshold = 0.5f;
    [SerializeField] float handOrientationThreshold = 0.85f;

    [SerializeField] bool transformInProgress = false;

    private Quaternion initialHandRotation;   // hand orientation when starting
    private Vector3 initialHandCenter;        // hand center when starting
    private Quaternion objectLocalRotation;   // object rotation relative to hand
    private Vector3 objectLocalPosition;      // object position relative to hand

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

        // Use palmNormal directly as the hand "forward" orientation
        initialHandRotation = Quaternion.LookRotation(magicHandGestures.palmNormal, Vector3.up);
        initialHandCenter = magicHandGestures.magicHand.GetCenter();

        // Compute object's relative transform in hand space
        objectLocalPosition = Quaternion.Inverse(initialHandRotation) * (objectToTransform.position - initialHandCenter);
        objectLocalRotation = Quaternion.Inverse(initialHandRotation) * objectToTransform.rotation;
    }

    public void StopTransform()
    {
        transformInProgress = false;
    }

    public void UpdateTransform()
    {
        // Current hand rotation & position
        Quaternion currentHandRotation = Quaternion.LookRotation(magicHandGestures.palmNormal, Vector3.up);
        Vector3 currentHandCenter = magicHandGestures.magicHand.GetCenter();

        // Reapply relative transform
        objectToTransform.position = currentHandCenter + currentHandRotation * objectLocalPosition;
        objectToTransform.rotation = currentHandRotation * objectLocalRotation;
    }
}
