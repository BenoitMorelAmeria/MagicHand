using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class CalibrationWrapper : MonoBehaviour
{
    [DllImport("HandPose")]
    private static extern IntPtr createCameraStructs();

    [DllImport("HandPose")]
    private static extern void destroyCameraStructs(IntPtr cameraStructs);

    [DllImport("HandPose")]
    private static extern void setNumCameras(IntPtr cameraStructs, uint numCameras);

    [DllImport("HandPose")]
    private static extern void setIntrinsicsCameraStructs(IntPtr cameraStructs, int camIdx, float fx, float fy, float ppx, float ppy, uint width, uint height, float depthUnits);

    [DllImport("HandPose")]
    private static extern void setPoseCameraStructs(IntPtr cameraStructs, int camIdx, int infraredIdx, float[] R, float[] t);

    private IntPtr mCameraStructs;

    // Start is called before the first frame update
    void Start()
    {
        mCameraStructs = createCameraStructs();

        if (mCameraStructs == IntPtr.Zero)
        {
            Debug.LogError("Failed to create Camera Structs.");
        }
    }

    public IntPtr GetCameraStructs()
    {
        return mCameraStructs;
    }

    public void SetNumCameras(uint numCameras)
    {
        if (mCameraStructs != IntPtr.Zero)
        {
            setNumCameras(mCameraStructs, numCameras);
        }
    }

    public void SetIntrinsics(int camIdx, float fx, float fy, float ppx, float ppy, uint width, uint height, float depthUnits)
    {
        if (mCameraStructs != IntPtr.Zero)
        {
            setIntrinsicsCameraStructs(mCameraStructs, camIdx, fx, fy, ppx, ppy, width, height, depthUnits);
        }
    }

    public void SetPose(int camIdx, int infraredIdx, float[] R, float[] t)
    {
        if (mCameraStructs != IntPtr.Zero)
        {
            setPoseCameraStructs(mCameraStructs, camIdx, infraredIdx, R, t);
        }
    }

    void OnDestroy()
    {
        if (mCameraStructs != IntPtr.Zero)
        {
            destroyCameraStructs(mCameraStructs);
            mCameraStructs = IntPtr.Zero;
        }
    }
}
