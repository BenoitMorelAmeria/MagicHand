using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BrushColorController : MonoBehaviour
{
    [SerializeField] MagicHandGestures magicHandGestures;
    [SerializeField] DrawManager drawManager;
    [SerializeField] Vector3 thumbOrientationToStart = Vector3.right;
    [SerializeField] float thumbOrientationThreshold = 0.8f;

    public float hueChangeSpeed = 0.1f;



    private bool _isChanging = false;
    private float _startingHue = 0.0f;
    private float _startThumbY;

    static float Wrap01(float value)
    {
        value = value % 1f;       // modulo 1
        if (value < 0f) value += 1f;
        return value;
    }

    void Update()
    {
        if (!magicHandGestures.magicHand.IsAvailable() || !magicHandGestures.IsThumbUp)
        {
            _isChanging = false;
            return;
        }

        // Thumb vector (tip - base)
        Vector3 thumbOrientation = magicHandGestures.magicHand.GetKeyPoint(4) - magicHandGestures.magicHand.GetKeyPoint(1);
        thumbOrientation.Normalize();
        float dot = Mathf.Abs(Vector3.Dot(thumbOrientation, thumbOrientationToStart));
        if (dot < thumbOrientationThreshold)
        {
            _isChanging = false;
            return;
        }

        Vector3 thumbTip = magicHandGestures.magicHand.GetKeyPoint(4);
        Vector3 thumbBase = magicHandGestures.magicHand.GetKeyPoint(1); // base of thumb
        float thumbY = thumbTip.y; // horizontal offset

        if (!_isChanging)
        {
            _isChanging = true;
            _startThumbY = thumbY; // store reference X
            _startingHue = drawManager.GetHue();
        }
        else
        {
            float deltaY = thumbY - _startThumbY;
            float hue = Wrap01(_startingHue + deltaY * hueChangeSpeed);
            drawManager.SetHue(hue);
        }
    }
}
