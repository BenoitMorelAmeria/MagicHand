using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Management;
using UnityEngine.XR;
using SRD.Sample.UI2DView;

/// <summary>
/// This class manages the spatial mode of the application
/// Spatial mode means that a 3D screen is detected and that we want to use it
/// It affects the initial position of the main camera and the rendering of the canvases
/// (we dont use 2D menus in spatial mode)
/// </summary>
public class SpatialManager : MonoBehaviour
{
    public static SpatialManager Instance { get; private set; } // Singleton instance

    public void Awake()
    {
        if (Instance == null)
        {
            Instance = this; // Set the singleton instance
        }
        else
        {
            Destroy(gameObject); // Destroy duplicate instances
            return;
        }

    }

    [SerializeField] private SRD.Core.SRDManager _srdManager;
    [SerializeField] protected bool initialEnableSpatialMode;
    [SerializeField] protected List<Canvas> _canvases;
    [SerializeField] protected Camera _mainCamera;


    public enum SpatialScreenMode
    {
        Screen2D, // normal screen, no 3D effect
        SpatialScreen, // ACER screen
        SRDScreen // Sony screen
    }

    public SpatialScreenMode spatialScreenMode = SpatialScreenMode.Screen2D;

    /// <summary>
    /// Check if a spatial screen is detected
    /// </summary>
    /// <returns></returns>
    static public bool IsSpatialScreenDetected()
    {
        if (XRGeneralSettings.Instance != null && XRGeneralSettings.Instance.Manager.isInitializationComplete)
        {
            // Check if an XR device is active
            if (XRSettings.isDeviceActive)
            {
                return true;
            }
        }
        return false;
    }

    // Start is called before the first frame update
    public void Start()
    {
        if (_srdManager != null && _srdManager.isActiveAndEnabled)
        {
            spatialScreenMode = SpatialScreenMode.SRDScreen;
        }
        else if (IsSpatialScreenDetected())
        {
            spatialScreenMode = SpatialScreenMode.SpatialScreen;
        }
        else
        {
            spatialScreenMode = SpatialScreenMode.Screen2D;
        }
        Debug.Log("Spatial screen mode: " + spatialScreenMode);
        SetCursorVisibility(!Is3DScreen());
        foreach (Canvas canvas in _canvases)
        {
            canvas.renderMode = Is3DScreen() ? RenderMode.ScreenSpaceCamera : RenderMode.ScreenSpaceOverlay;
            //canvas.renderMode = (spatialScreenMode == SpatialScreenMode.SpatialScreen) ? RenderMode.ScreenSpaceCamera : RenderMode.ScreenSpaceOverlay;
        }
    }

    // Update is called once per frame
    void Update()
    {
    }

    public bool Is3DScreen()
    {
        return spatialScreenMode == SpatialScreenMode.SpatialScreen || spatialScreenMode == SpatialScreenMode.SRDScreen;
    }

    void SetCursorVisibility(bool visible)
    {
        Cursor.visible = visible;
    }
    public void SwitchCursorVisibility()
    {
        Cursor.visible = !Cursor.visible;
    }

}
