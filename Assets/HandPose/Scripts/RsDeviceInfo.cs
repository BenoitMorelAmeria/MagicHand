using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Intel.RealSense;
using System;

public class RsDeviceInfo : MonoBehaviour
{
    public List<RsDevice> mDevices = new List<RsDevice>();

    private List<Matrix4x4> mExtrinsicsMat = new List<Matrix4x4>();
    private List<Intrinsics> mIntrinsics = new List<Intrinsics>();
    private List<float> mDepthUnits = new List<float>();
    private bool _isInit = false;

    private void Clear()
    {
        mExtrinsicsMat.Clear();
        mIntrinsics.Clear();
        mDepthUnits.Clear();
    }

    public bool EnsureInit()
    {
        if (_isInit) return true;

        try
        {
            for (int i = 0; i < mDevices.Count; i++)
            {
                var device = mDevices[i];

                var ir1Stream = device.ActiveProfile.GetStream(Stream.Infrared, 1);
                var ir2Stream = device.ActiveProfile.GetStream(Stream.Infrared, 2);
                var depthStream = device.ActiveProfile.GetStream(Stream.Depth);

                mIntrinsics.Add(ir1Stream.As<VideoStreamProfile>().GetIntrinsics());
                mDepthUnits.Add(0.001f);

                var ir2Extrinsics = ir1Stream.GetExtrinsicsTo(ir2Stream);
                Matrix4x4 extrinsicsMat = new Matrix4x4();
                extrinsicsMat.m00 = ir2Extrinsics.rotation[0];
                extrinsicsMat.m01 = ir2Extrinsics.rotation[1];
                extrinsicsMat.m02 = ir2Extrinsics.rotation[2];
                extrinsicsMat.m10 = ir2Extrinsics.rotation[3];
                extrinsicsMat.m11 = ir2Extrinsics.rotation[4];
                extrinsicsMat.m12 = ir2Extrinsics.rotation[5];
                extrinsicsMat.m20 = ir2Extrinsics.rotation[6];
                extrinsicsMat.m21 = ir2Extrinsics.rotation[7];
                extrinsicsMat.m22 = ir2Extrinsics.rotation[8];

                // Set translation
                extrinsicsMat.m03 = ir2Extrinsics.translation[0];
                extrinsicsMat.m13 = ir2Extrinsics.translation[1];
                extrinsicsMat.m23 = ir2Extrinsics.translation[2];

                // Last row
                extrinsicsMat.m30 = 0f;
                extrinsicsMat.m31 = 0f;
                extrinsicsMat.m32 = 0f;
                extrinsicsMat.m33 = 1f;
                mExtrinsicsMat.Add(extrinsicsMat);
            }

            _isInit = true;
            Debug.Log("RsDeviceInfo got camera info");

            return true;
        }
        catch (Exception)
        {
            Clear();

            return false;
        }
    }

    public bool isInit => _isInit;

    public string getSerialNumber(int deviceIdx)
    {
        return mDevices[deviceIdx].DeviceConfiguration.RequestedSerialNumber;
    }

    public int getNumDevices()
    {
        return mDevices.Count;
    }

    public Intrinsics getIntrinsics(int deviceIdx)
    {
        return mIntrinsics[deviceIdx];
    }

    public float getDepthUnits(int deviceIdx)
    {
        return mDepthUnits[deviceIdx];
    }

    public Matrix4x4 getExtrinsics(int deviceIdx)
    {
        return mExtrinsicsMat[deviceIdx];
    }
}
