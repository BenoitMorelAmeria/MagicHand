using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagicScroller : MonoBehaviour
{
    [Header("References")]
    [SerializeField] Transform objectToScroll;
    [SerializeField] MagicHandGestures magicHandGestures;

    [Header("Activation conditions")]
    [SerializeField] bool requireHandFlat = true; // Require hand to be flat to start scrolling
    [SerializeField] float maxDistanceToScreen = 0.25f; // Max distance from hand to screen if requireHandFlat is true

    [SerializeField] float maxFingerDistanceToScreen = 0.1f; // Max distance between finger and
    [SerializeField] int numberOfFingersToIntersect = 4; // Number of fingers required to be opened and to intersect the plane


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

    private bool CheckCondition()
    {
        if (!magicHandGestures.magicHand.IsAvailable())
        {
            return false;
        }
        if (requireHandFlat)
        {
            return magicHandGestures.IsHandFlat
           && Mathf.Abs(magicHandGestures.magicHand.GetCenter().z) < maxDistanceToScreen;
        }
        else
        {
            // count the number of opened fingers close enough
            int count = 0;
            for (int i = 0; i < 5; ++i)
            {
                if (magicHandGestures.fingerFrontness[i] > 0.0f // finger is opened
                    && Mathf.Abs(magicHandGestures.magicHand.Data.GetKeypointScreenSpace(4 * i + 4).z) < maxFingerDistanceToScreen) // fingertip is close enough to the screen
                {
                    count++;
                }
            }
            return count >= numberOfFingersToIntersect;
        }
    }

    public bool IsStartScrollConditionMet()
    {
        if (isScrolling)
        {
            return false;
        }
        // Hand must be flat
        return CheckCondition();
    }

    public bool IsStopScrollConditionMet()
    {

        if (!isScrolling)
        {
            return false;
        }
        return !CheckCondition();
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
