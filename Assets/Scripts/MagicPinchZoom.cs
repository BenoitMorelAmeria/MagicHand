using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagicpINCHZoom : MonoBehaviour
{
    [Header("References")]
    [SerializeField] Transform objectToTranslate;
    [SerializeField] Transform objectToZoom;
    [SerializeField] MagicHandGestures magicHandGestures;

    [Header("Zoom parameters")]
    [SerializeField] float maxDistanceToScreen = 0.15f; // Max distance from hand to screen to start scrolling
    [SerializeField] float fingerDistanceEpsilon = 0.01f;
    [SerializeField] float minFingerAlignmentToZ = 0.5f; // Min alignment of thumb and index finger to Z axis to start zooming

    // current state
    bool isZooming = false;
    Vector3 lastPosition = Vector3.zero;
    float lastDistance = 0.0f; 


    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (IsStartZoomConditionMet())
        {
            StartZoom();
        }
        else if (IsStopZoomConditionMet())
        {
            StopZoom();
        }
        if (isZooming)
        {
            UpdateZoom();
        }
    }

    private bool IsHandInZoomMode()
    {
        if (!magicHandGestures.magicHand.IsAvailable())
        {
            return false;
        }
        float thumbAlignment = GetFingerAlignmentToZ(0);
        float indexAlignment = GetFingerAlignmentToZ(1);
        Debug.Log(thumbAlignment + " " + indexAlignment);    
        return magicHandGestures.magicHand.IsAvailable()
            && thumbAlignment > minFingerAlignmentToZ
            && indexAlignment > minFingerAlignmentToZ
            && GetDistanceToScreen() < maxDistanceToScreen
            //&& magicHandGestures.fingerFrontness[2] < 0.0f
            //&& magicHandGestures.fingerFrontness[3] < 0.0f
            //&& magicHandGestures.fingerFrontness[4] < 0.0f
            ;
    }

    private bool IsStartZoomConditionMet()
    {
        return !isZooming && IsHandInZoomMode();
    }

    private bool IsStopZoomConditionMet()
    {
        return isZooming && !IsHandInZoomMode();
    }

    private void StartZoom()
    {
        isZooming = true;
        lastDistance = GetDistance();
        lastPosition = GetZoomPosition();
    }

    private void StopZoom()
    {
        isZooming = false;
    }

    private void UpdateZoom()
    {

        // scale change is the ratio of the current distance to the last distance between fingers
        float distance = GetDistance();
        Vector3 position = GetZoomPosition();
        float scaleChange = distance / lastDistance;
        Vector3 positionDelta = position - lastPosition;
        // apply position change
        objectToTranslate.position += positionDelta;
        if (distance != 0.0f && lastDistance != 0.0f)
        {
            // apply scale change
            objectToZoom.localScale *= scaleChange;
        }
        lastDistance = distance;
        lastPosition = position;
    }

    private Vector3 GetZoomPosition()
    {
        Vector3 v3 = (magicHandGestures.magicHand.GetKeyPoint(4) + magicHandGestures.magicHand.GetKeyPoint(8)) / 2.0f;
        return new Vector3(v3.x, v3.y, 0.0f);
    }

    private float GetDistance()
    {
        return Mathf.Max(fingerDistanceEpsilon, 
            Vector3.Distance(magicHandGestures.magicHand.GetKeyPoint(4),
                                magicHandGestures.magicHand.GetKeyPoint(8)));
    }

    private float GetFingerAlignmentToZ(int fingerIndex)
    {
        int index1 = fingerIndex * 4 + 1;
        int index2 = fingerIndex * 4 + 4;
        Vector3 fingerDirection = magicHandGestures.magicHand.GetKeyPointDiff(index2, index1).normalized;
        return Mathf.Abs(Vector3.Dot(fingerDirection, Vector3.forward));
    }

    private float GetFingerDistanceToScreen(int fingerIndex)
    {
        return Mathf.Abs(magicHandGestures.magicHand.GetKeyPoint(4 + 4 * fingerIndex).z);
    }

    private float GetDistanceToScreen()
    {
        return Mathf.Max(GetFingerDistanceToScreen(0), GetFingerDistanceToScreen(1));
    }
}
