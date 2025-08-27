using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagicRotationTranslation : MonoBehaviour
{
    [SerializeField] Transform objectToTransform;
    [SerializeField] MagicHandGestures magicHandGestures;
    [SerializeField] List<Vector3> palmOrientationsToStartTranslation = new List<Vector3>();
    [SerializeField] List<Vector3> palmOrientationsToStartTransform = new List<Vector3>();

    [SerializeField] float flatHandDurationThreshold = 0.5f;
    [SerializeField] float handOrientationThreshold = 0.85f;

    private bool transformInProgress = false;

    private Quaternion initialHandRotation;
    private Vector3 initialHandCenter;
    private Quaternion objectLocalRotation;
    private Vector3 objectLocalPosition;
    private Quaternion savedObjectRotation; // keep original rotation if onlyTranslation


    private bool onlyTranslation = false;


    bool IsOrientationInList(Vector3 palmNormal, List<Vector3> orientationList)
    {
        foreach (var orientation in orientationList)
        {
            if (Vector3.Dot(palmNormal, orientation) > handOrientationThreshold)
                return true;
        }
        return false;
    }

    void Update()
    {
        bool shouldStartTransform = magicHandGestures.IsHandFlat &&
                                    magicHandGestures.flatHandDuration > flatHandDurationThreshold &&
                                    !transformInProgress;


        if (shouldStartTransform)
        {
            if (IsOrientationInList(magicHandGestures.palmNormal, palmOrientationsToStartTransform))
            {
                onlyTranslation = false;
            }
            else if (IsOrientationInList(magicHandGestures.palmNormal, palmOrientationsToStartTranslation))
            {
                onlyTranslation = true;
            }
            else
            {
                shouldStartTransform = false;
            }
        }

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

        Vector3 palmNormal = magicHandGestures.palmNormal.normalized;
        Vector3 handRight = magicHandGestures.palmRight.normalized;
        Vector3 handUp = Vector3.Cross(palmNormal, handRight).normalized;

        initialHandRotation = Quaternion.LookRotation(palmNormal, handUp);
        initialHandCenter = magicHandGestures.magicHand.GetCenter();

        if (onlyTranslation)
        {
            // Don’t involve rotation in the math
            objectLocalPosition = objectToTransform.position - initialHandCenter;
            savedObjectRotation = objectToTransform.rotation;
        }
        else
        {
            objectLocalPosition = Quaternion.Inverse(initialHandRotation) * (objectToTransform.position - initialHandCenter);
            objectLocalRotation = Quaternion.Inverse(initialHandRotation) * objectToTransform.rotation;
        }
    }

    public void UpdateTransform()
    {
        Vector3 palmNormal = magicHandGestures.palmNormal.normalized;
        Vector3 handRight = magicHandGestures.palmRight.normalized;
        Vector3 handUp = Vector3.Cross(palmNormal, handRight).normalized;

        Quaternion currentHandRotation = Quaternion.LookRotation(palmNormal, handUp);
        Vector3 currentHandCenter = magicHandGestures.magicHand.GetCenter();

        if (onlyTranslation)
        {
            objectToTransform.position = currentHandCenter + objectLocalPosition;
            objectToTransform.rotation = savedObjectRotation; // keep original
        }
        else
        {
            objectToTransform.position = currentHandCenter + currentHandRotation * objectLocalPosition;
            objectToTransform.rotation = currentHandRotation * objectLocalRotation;
        }
    }

    public void StopTransform()
    {
        transformInProgress = false;
    }

}
