using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.XR;
using UnityEngine.SpatialTracking; // Make sure you include this!


public class CameraManager : MonoBehaviour
{
    public Camera mainCamera;
    public Transform camerasParentTransform;

    [SerializeField] private Camera defaultMainCamera;
    [SerializeField] private Camera srdScreenMainCamera;
    [SerializeField] private Camera guiCamera;
    [SerializeField] private SpatialManager spatialManager;

    [SerializeField] private GameObject SRDDisplayManager;
    [SerializeField] private GameObject SRDPresence;
    [SerializeField] private GameObject SRDWatcherAnchor;
    [SerializeField] private GameObject SRDLeftEyeAnchor;
    [SerializeField] private GameObject SRDLeftEyeCamera;

    public void PrintDebug()
    {
        /*
        Debug.Log("CameraManager: SRDDisplayManager=" + SRDDisplayManager.transform.localPosition);
        Debug.Log("CameraManager: SRDPresence=" + SRDPresence.transform.localPosition);
        Debug.Log("CameraManager: SRDWatcherAnchor=" + SRDWatcherAnchor.transform.localPosition);
        Debug.Log("CameraManager: SRDLeftEyeAnchor=" + SRDLeftEyeAnchor.transform.localPosition);
        Debug.Log("CameraManager: SRDLeftEyeCamera=" + SRDLeftEyeCamera.transform.localPosition);
        Debug.Log("CameraManager: SRDLeftEyeCamera global=" + SRDLeftEyeCamera.transform.position);
        */
    }

    // Start is called before the first frame update
    public void Start()
    {
        // Set the main camera
        mainCamera = defaultMainCamera;
        if (spatialManager.spatialScreenMode == SpatialManager.SpatialScreenMode.SRDScreen)
        {
            mainCamera = srdScreenMainCamera;
        }

        // Set the main camera position
        if (spatialManager.spatialScreenMode == SpatialManager.SpatialScreenMode.Screen2D)
        {
            mainCamera.transform.localPosition = mainCamera.transform.localPosition + new Vector3(0.0f, 0.0f, -0.8f);
        }

        // Enable/disable cameras
        defaultMainCamera.gameObject.SetActive(spatialManager.spatialScreenMode != SpatialManager.SpatialScreenMode.SRDScreen);

        if (spatialManager.spatialScreenMode == SpatialManager.SpatialScreenMode.SRDScreen)
        {
            // if tracked pose driver is enabled, the GUI moves...
            if (guiCamera == null)
            {
                Debug.LogError("Null gui camera in CameraManager.Start().");
            }

            UnityEngine.SpatialTracking.TrackedPoseDriver trackedPoseDriver = guiCamera.GetComponent<UnityEngine.SpatialTracking.TrackedPoseDriver>();
            if (trackedPoseDriver == null)
            {
                Debug.LogError("Null trackedPoseDriver in CameraManager.Start().");
            }

            trackedPoseDriver.enabled = false;
        }

    }

    // Update is called once per frame
    void Update()
    {
        if (spatialManager.spatialScreenMode == SpatialManager.SpatialScreenMode.SRDScreen)
        {
            PrintDebug();
        }
    }
}
