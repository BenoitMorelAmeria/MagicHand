
using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class DrawManager: MonoBehaviour
{
    [SerializeField] List<InkDrawerBase> drawers = new List<InkDrawerBase>();
    [SerializeField] Transform pointer;
    [SerializeField] float brushSize = 0.1f;
    [SerializeField] Color brushColor = Color.blue;
    [SerializeField] float hueStep = 0.1f;

    int _currentDrawerIndex = 0;

    public void Update()
    {
        pointer.localScale = Vector3.one * brushSize;

        if (Input.GetMouseButtonDown(0))
        {
            drawers[_currentDrawerIndex].StartNewCurve();
        }
        if (Input.GetMouseButton(0))
        {
            drawers[_currentDrawerIndex].NextPoint(pointer.position, brushColor, brushSize);
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




}