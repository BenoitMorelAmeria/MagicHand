using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.Rendering;
using UnityEngine;

public class MagicScroller : MonoBehaviour
{
    [SerializeField] Transform objectToScroll;
    [SerializeField] MagicHandGestures magicHandGestures;


    bool isScrolling = false;
    Vector3 lastHandPosition;


    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
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
        // hand must be flat
        return !isScrolling && magicHandGestures.IsHandFlat;
    }

    public bool IsStopScrollConditionMet()
    {
        // hand must not be flat
        return isScrolling && !magicHandGestures.IsHandFlat;
    }

    public void StartScroll()
    {
        lastHandPosition = magicHandGestures.magicHand.GetCenter();
        isScrolling = true;
    }

    public void StopScroll()
    {
        isScrolling = false;
    }

    public void UpdateScroll()
    {
        if (!isScrolling)
        {
            return;
        }
        Vector3 currentHandPosition = magicHandGestures.magicHand.GetCenter();
        Vector3 delta = currentHandPosition - lastHandPosition;
        objectToScroll.position += new Vector3(delta.x, delta.y, 0);
        lastHandPosition = currentHandPosition;
    }
}
