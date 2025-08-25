
using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class DrawManager: MonoBehaviour
{
    [SerializeField] GameObject pointerMouse;
    [SerializeField] GameObject pointerMagicHand;
    [SerializeField] MagicHand magicHand; 
    [SerializeField] bool UseMagicHand = true;

    [SerializeField] List<InkDrawerBase> drawers = new List<InkDrawerBase>();
    [SerializeField] float brushSize = 0.1f;
    [SerializeField] Color brushColor = Color.blue;
    [SerializeField] float hueStep = 0.1f;

    int _currentDrawerIndex = 0;

    private bool _pinchStateJustChanged = false;
    private bool _currentPinchState = false;

    public void Update()
    {

        pointerMagicHand.SetActive(UseMagicHand);
        pointerMouse.SetActive(!UseMagicHand);

        bool pinchState = magicHand.IsAvailable() && magicHand.GetPinchState();
        if (pinchState != _currentPinchState)
        {
            _pinchStateJustChanged = true;
            _currentPinchState = pinchState;
        }
        else
        {
            _pinchStateJustChanged = false;
        }
        Debug.Log("Pinch state: " + pinchState);

        GetPointer3D().transform.localScale = Vector3.one * brushSize;

        if (WasJustClicked())
        {
            drawers[_currentDrawerIndex].StartNewCurve();
        }
        if (IsClickPressed())
        {
            drawers[_currentDrawerIndex].NextPoint(GetPointer3D().transform.position, brushColor, brushSize);
        }
        if (Input.GetKeyDown(KeyCode.P))
        {
            brushSize *= 1.2f;
        }
        if (Input.GetKeyDown(KeyCode.M))
        {
            brushSize /= 1.2f;
        }
        if (Input.GetKeyDown(KeyCode.Z))
        {
            Debug.Log("z pressed");
            drawers[_currentDrawerIndex].Rollback();
        }
        if (Input.GetKeyDown(KeyCode.C))
        {
            Color.RGBToHSV(brushColor, out float h, out float s, out float v);
            h += hueStep;
            if (h > 1f) h -= 1f; // wrap around
            brushColor = Color.HSVToRGB(h, s, v);
        }
    }

    private GameObject GetPointer3D()
    {
        if (UseMagicHand)
        {
            return pointerMagicHand;
        }
        else
        {
            return pointerMouse;
        }
    }

    private bool WasJustClicked()
    {
        if (UseMagicHand)
        {
            return _pinchStateJustChanged && _currentPinchState;
        } else
        {
            return Input.GetMouseButtonDown(0);
        }
    }

    private bool IsClickPressed()
    {
        if (UseMagicHand)
        {
            return _currentPinchState;
        }
        else
        {
            return Input.GetMouseButton(0);
        }

    }
}