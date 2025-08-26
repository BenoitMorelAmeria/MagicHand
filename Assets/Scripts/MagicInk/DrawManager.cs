
using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class DrawManager: MonoBehaviour
{
    public enum PointerMode
    {
        Mouse,
        MagicHand,
        INA
    }

    [SerializeField] GameObject pointerMouse;
    [SerializeField] GameObject pointerMagicHand;
    [SerializeField] GameObject pointerINA;
    [SerializeField] MagicHand magicHand; 
    [SerializeField] PointerMode pointerMode = PointerMode.MagicHand;

    [SerializeField] List<InkDrawerBase> drawers = new List<InkDrawerBase>();
    [SerializeField] float brushSize = 0.1f;
    [SerializeField] Color brushColor = Color.blue;
    [SerializeField] float hueStep = 0.1f;

    int _currentDrawerIndex = 0;

    private bool _pinchStateJustChanged = false;
    private bool _currentPinchState = false;

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            if (pointerMode == PointerMode.Mouse)
                pointerMode = PointerMode.MagicHand;
            else if (pointerMode == PointerMode.MagicHand)
                pointerMode = PointerMode.INA;
            else
                pointerMode = PointerMode.Mouse;
        }
        

        pointerMagicHand.SetActive(pointerMode == PointerMode.MagicHand);
        pointerMouse.SetActive(pointerMode == PointerMode.Mouse);
        pointerINA.SetActive(pointerMode == PointerMode.INA);

        magicHand.SetVisible(pointerMode == PointerMode.MagicHand);

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
        if (pointerMode == PointerMode.Mouse)
        {
            return pointerMouse;
        }
        else if (pointerMode == PointerMode.MagicHand)
        {
            return pointerMagicHand;
        }
        else
        {
            return pointerINA;
        }
    }

    private bool WasJustClicked()
    {
        if (pointerMode != PointerMode.Mouse)
        {
            return Input.GetMouseButtonDown(0) || (_pinchStateJustChanged && _currentPinchState);
        } else
        {
            return Input.GetMouseButtonDown(0);
        }
    }

    private bool IsClickPressed()
    {
        if (pointerMode != PointerMode.Mouse)
        {
            return _currentPinchState || Input.GetMouseButton(0);
        }
        else
        {
            return Input.GetMouseButton(0);
        }

    }
}