
using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class DrawManager : MonoBehaviour
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
    [SerializeField] Material pointerMaterial;
    [SerializeField] MagicHand magicHand;
    [SerializeField] MagicHandGestures magicHandGestures;
    [SerializeField] PointerMode pointerMode = PointerMode.MagicHand;

    [SerializeField] List<InkDrawerBase> drawers = new List<InkDrawerBase>();
    [SerializeField] public float brushSize = 0.1f;
    [SerializeField] Color brushColor = Color.blue;
    [SerializeField] float hueStep = 0.1f;

    [SerializeField] AudioClip cutSound;

    [SerializeField] float angleThreshold = 15f;
    float lastAngleChangeTime = 0.0f;

    int _currentDrawerIndex = 0;

    private bool _pinchStateJustChanged = false;
    private bool _currentPinchState = false;
    private bool _wasClickLastFrame = false;
    private Vector3 _prevPos;
    private Vector3 _prevDir;
    private AudioSource _cutAudioSource;

    public void Start()
    {
        Debug.Log("Draw manager start");
        _cutAudioSource = gameObject.AddComponent<AudioSource>();
        _cutAudioSource.clip = cutSound;
        _cutAudioSource.playOnAwake = false;


        magicHandGestures.OnCutGesture += () =>
        {
            Rollback();
        };
    }

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
            lastAngleChangeTime = Time.time + 0.1f;
        }
        if (IsClickPressed())
        {
            Color color = brushColor;
            if (UpdateAndCheckAngleChange(GetPointer3D().transform.position))
            {
                lastAngleChangeTime = Time.time;
                // Sudden direction change detected
                //color = Color.white; // Change color to white on sudden direction change
            }
            drawers[_currentDrawerIndex].NextPoint(GetPointer3D().transform.position, color, brushSize);
            _wasClickLastFrame = true;

        }
        else
        {
            _prevPos = Vector3.zero;
            _prevDir = Vector3.zero;
            if (_wasClickLastFrame)
            {
                if (pointerMode != PointerMode.Mouse)
                    //drawers[_currentDrawerIndex].ClearRecent(rollbackTimeDelta);
                    drawers[_currentDrawerIndex].ClearRecent(Time.time - lastAngleChangeTime);
                Debug.Log("unclick");
            }
            _wasClickLastFrame = false;
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
            Rollback();
        }
        if (Input.GetKeyDown(KeyCode.C))
        {
            Color.RGBToHSV(brushColor, out float h, out float s, out float v);
            h += hueStep;
            if (h > 1f) h -= 1f; // wrap around
            brushColor = Color.HSVToRGB(h, s, v);
        }


        pointerMaterial.color = brushColor;
        // set emission color
        pointerMaterial.SetColor("_EmissionColor", brushColor * 2.0f);
    }

    public float GetHue()
    {
        Color.RGBToHSV(brushColor, out float h, out float s, out float v);
        return h;
    }

    public void SetHue(float hue)
    {
        Color.RGBToHSV(brushColor, out float h, out float s, out float v);
        brushColor = Color.HSVToRGB(hue, s, v);
    }

    private void Rollback()
    {

        if (_cutAudioSource != null)
        {
            _cutAudioSource.Play();
        }
        drawers[_currentDrawerIndex].Rollback();
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

    private bool UpdateAndCheckAngleChange(Vector3 currentPos)
    {
        if (currentPos == Vector3.zero || currentPos == _prevPos)
            return false;

        Vector3 dir = (currentPos - _prevPos).normalized;
        bool res = false;
        Debug.Log(_prevDir + " " + dir);
        if (_prevPos != Vector3.zero)
        {
            if (_prevDir != Vector3.zero)
            {
                // Check angle between directions
                float angle = Vector3.Angle(_prevDir, dir);
                if (angle > angleThreshold)
                {
                    res = true;
                    Debug.Log($"Sudden direction change detected: {angle:F1}°");
                    //OnDirectionChange(angle, currentPos);
                }
            }

            _prevDir = dir;
        }
        _prevPos = currentPos;

        return res;
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