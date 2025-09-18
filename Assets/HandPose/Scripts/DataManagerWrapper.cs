using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Runtime.InteropServices;
using UnityEngine;

public class DataManagerWrapper : MonoBehaviour
{
    [DllImport("HandPose")]
    private static extern IntPtr createDataManager();

    [DllImport("HandPose")]
    private static extern void destroyDataManager(IntPtr dataManager);

    [DllImport("HandPose")]
    private static extern void setNumCamerasDataManager(IntPtr dataManager, int numCameras);

    [DllImport("HandPose")]
    private static extern void setImageDataManager(IntPtr dataManager, IntPtr img, int width, int height, int camIdx, int irIdx);

    [DllImport("HandPose")]
    private static extern void setDepthImageDataManager(IntPtr dataManager, IntPtr depthImg, int depthWidth, int depthHeight, int camIdx);

    IntPtr mDataManager;

    void Start()
    {
        mDataManager = createDataManager();
        if (mDataManager == IntPtr.Zero)
        {
            Debug.LogError("Failed to create Data Manager.");
        }
    }

    public IntPtr GetDataManager ()
    {
        return mDataManager;
    }

    public void SetNumCameras(int numCameras)
    {
        if (mDataManager != IntPtr.Zero)
        {
            setNumCamerasDataManager(mDataManager, numCameras);
        }
        else
        {
            Debug.LogError("Data Manager is not initialized.");
        }
    }

    public void SetImage(IntPtr img, int width, int height, int camIdx, int irIdx)
    {
        if (mDataManager != IntPtr.Zero)
        {
            setImageDataManager(mDataManager, img, width, height, camIdx, irIdx);
        }
        else
        {
            Debug.LogError("Data Manager is not initialized.");
        }
    }

    public void SetDepthImage(IntPtr depthImg, int depthWidth, int depthHeight, int camIdx)
    {
        if (mDataManager != IntPtr.Zero)
        {
            setDepthImageDataManager(mDataManager, depthImg, depthWidth, depthHeight, camIdx);
        }
        else
        {
            Debug.LogError("Data Manager is not initialized.");
        }
    }

    void OnDestroy()
    {
        if (mDataManager != IntPtr.Zero)
        {
            destroyDataManager(mDataManager);
            mDataManager = IntPtr.Zero;
        }
    }
}
