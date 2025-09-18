using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class PosePipelineWrapper : MonoBehaviour
{
    [DllImport("HandPose")]
    private static extern IntPtr createPosePipeline();

    [DllImport("HandPose")]
    private static extern void destroyPosePipeline(IntPtr pipeline);

    [DllImport("HandPose")]
    private static extern void initializePosePipeline(IntPtr pipeline, string configPath);

    [DllImport("HandPose")]
    private static extern bool isInitializedPosePipeline(IntPtr pipeline);

    [DllImport("HandPose")]
    private static extern void setCalibrationFromStructsPosePipeline(IntPtr pipeline, IntPtr cameraStructs);

    [DllImport("HandPose")]
    private static extern bool isCalibratedPosePipeline(IntPtr pipeline);

    [DllImport("HandPose")]
    private static extern void executePosePipeline(IntPtr pipeline, IntPtr dataManager, bool debug);

    [DllImport("HandPose")]
    private static extern void executePosePipelineWithDepth(IntPtr pipeline, IntPtr dataManager, bool debug);

    [DllImport("HandPose")]
    private static extern int getNumDetectionsPosePipeline(IntPtr pipeline);

    /*
     __declspec(dllexport) const float* getDetectionsPosePipeline(const PosePipeline* pipeline, int index);

	__declspec(dllexport) int getNumKeyPointsPosePipeline(const PosePipeline* pipeline);

	__declspec(dllexport) const float* getKeyPointsPosePipeline(const PosePipeline* pipeline, int index);
     
     */

    [DllImport("HandPose")]
    private static extern int getNumKeyPoints3dPosePipeline(IntPtr pipeline);

    [DllImport("HandPose")]
    private static extern IntPtr getKeyPoints3dPosePipeline(IntPtr pipeline, int index);

    [DllImport("HandPose")]
    private static extern int getMaxNumHandsPosePipeline(IntPtr pipeline);

    private IntPtr mPosePipeline;

    // Start is called before the first frame update
    void Start()
    {
        mPosePipeline = createPosePipeline();

        if (mPosePipeline == IntPtr.Zero)
        {
            Debug.LogError("Failed to create Pose Pipeline.");
        }
    }

    public void InitializePosePipeline(string configPath)
    {
        if (mPosePipeline != IntPtr.Zero)
        {
            initializePosePipeline(mPosePipeline, configPath);
        }
    }

    public bool IsInitialized()
    {
        if (mPosePipeline != IntPtr.Zero)
        {
            return isInitializedPosePipeline(mPosePipeline);
        }
        return false;
    }

    public void SetCalibrationFromStructs(IntPtr cameraStructs)
    {
        if (mPosePipeline != IntPtr.Zero && cameraStructs != IntPtr.Zero)
        {
            setCalibrationFromStructsPosePipeline(mPosePipeline, cameraStructs);
        }
    }

    public bool IsCalibrated()
    {
        if (mPosePipeline != IntPtr.Zero)
        {
            return isCalibratedPosePipeline(mPosePipeline);
        }
        return false;
    }

    public void Execute(IntPtr dataManager, bool debug = false)
    {
        if (mPosePipeline != IntPtr.Zero && dataManager != IntPtr.Zero)
        {
            executePosePipeline(mPosePipeline, dataManager, debug);
        }
    }

    public void ExecuteWithDepth(IntPtr dataManager, bool debug = false)
    {
        if (mPosePipeline != IntPtr.Zero && dataManager != IntPtr.Zero)
        {
            executePosePipelineWithDepth(mPosePipeline, dataManager, debug);
        }
    }

    public int GetNumDetections()
    {
        if (mPosePipeline != IntPtr.Zero)
        {
            return getNumDetectionsPosePipeline(mPosePipeline);
        }
        return 0;
    }

    public int GetNumKeyPoints3d()
    {
        if (mPosePipeline != IntPtr.Zero)
        {
            return getNumKeyPoints3dPosePipeline(mPosePipeline);
        }
        return 0;
    }


    public float[] GetKeyPoints3d(int index)
    {
        if (mPosePipeline != IntPtr.Zero)
        {
            if (index >= 0 && index < GetNumKeyPoints3d())
            {
                IntPtr ptr = getKeyPoints3dPosePipeline(mPosePipeline, index);
                if (ptr != IntPtr.Zero)
                {
                    float[] keypoints = new float[21 * 4]; // Assuming 21 keypoints with (x, y, z) each
                    Marshal.Copy(ptr, keypoints, 0, keypoints.Length);
                    return keypoints;
                }
            }
            
        }
        return null;
    }

    public int GetMaxNumHands()
    {
        if (mPosePipeline != IntPtr.Zero)
        {
            return getMaxNumHandsPosePipeline(mPosePipeline);
        }
        return 0;
    }


    void OnDestroy()
    {
        if (mPosePipeline != IntPtr.Zero)
        {
            destroyPosePipeline(mPosePipeline);
            mPosePipeline = IntPtr.Zero;
        }
    }
}
