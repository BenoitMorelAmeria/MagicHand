using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagicScroller : MonoBehaviour
{
    [Header("References")]
    [SerializeField] Transform objectToScroll;
    [SerializeField] MagicHandGestures magicHandGestures;

    [Header("Activation conditions")]
    [SerializeField] float maxDistanceToScreen = 0.2f; // Max distance from hand to screen to start scrolling


    [Header("Inertia Settings")]
    [SerializeField] float inertiaDuration = 1.0f;   // How long inertia lasts
    [SerializeField] float deceleration = 5.0f;      // Higher = stops faster
    [SerializeField] bool useInertia = true;         // Toggle inertia on/off

    bool isScrolling = false;
    Vector3 lastHandPosition;
    Vector3 velocity;              // Current scrolling velocity
    float inertiaTimer = 0f;

    void Update()
    {
        if (IsStartScrollConditionMet())
        {
            StartScroll();
        }
        else if (IsStopScrollConditionMet())
        {
            StopScroll();
        }

        UpdateScroll();
    }

    public bool IsStartScrollConditionMet()
    {
        if (isScrolling)
        {
            return false;
        }
        // Hand must be flat
        return magicHandGestures.magicHand.IsAvailable()
            && magicHandGestures.IsHandFlat
            && Mathf.Abs(magicHandGestures.magicHand.GetCenter().z) < maxDistanceToScreen;
    }

    public bool IsStopScrollConditionMet()
    {

        if (!isScrolling)
        {
            return false;
        }
        return !magicHandGestures.magicHand.IsAvailable()
            || !magicHandGestures.IsHandFlat
            || Mathf.Abs(magicHandGestures.magicHand.GetCenter().z) >= maxDistanceToScreen;
    }

    public void StartScroll()
    {
        lastHandPosition = magicHandGestures.magicHand.GetCenter();
        velocity = Vector3.zero; // reset velocity
        isScrolling = true;
        Debug.Log("start scroll");
    }

    public void StopScroll()
    {
        Debug.Log("stop  scroll");
        isScrolling = false;
        inertiaTimer = 0f; // start inertia timer
    }

    public void UpdateScroll()
    {
        if (isScrolling)
        {
            // Hand-driven scrolling
            Vector3 currentHandPosition = magicHandGestures.magicHand.GetCenter();
            Vector3 delta = currentHandPosition - lastHandPosition;
            objectToScroll.position += new Vector3(delta.x, delta.y, 0);

            // Track velocity for inertia
            velocity = new Vector3(delta.x, delta.y, 0) / Time.deltaTime;

            lastHandPosition = currentHandPosition;
        }
        else if (useInertia && velocity.magnitude > 0.01f && inertiaTimer < inertiaDuration)
        {
            // Apply inertia
            objectToScroll.position += velocity * Time.deltaTime;

            // Apply deceleration
            velocity = Vector3.Lerp(velocity, Vector3.zero, deceleration * Time.deltaTime);

            inertiaTimer += Time.deltaTime;
        }
    }
}
