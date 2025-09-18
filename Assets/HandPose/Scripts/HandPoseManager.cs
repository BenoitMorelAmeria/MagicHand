using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Ameria.SpatialHand;
using Intel.RealSense;

public class HandPoseManager : MonoBehaviour
{
    public RsDeviceInfo mRsDeviceInfo;
    public DataManagerWrapper mDataManager;
    public PosePipelineWrapper mPosePipeline;
    public CalibrationInfo mCalibrationInfo;
    public CalibrationWrapper mCalibrationWrapper;

    // textures from devices
    public List<MyTextureRenderer> depthRenderer;
    public List<MyTextureRenderer> ir1Renderer;
    public List<MyTextureRenderer> ir2Renderer;

    public bool ShowDebugOpenCV = false;
    public bool enablePinchDetection = false;

    private string[] ConfigFilePaths =
    {
        @"C:\ProgramData\Ina\Data\HandPoseUnity\PipelineConfig.json",
        @"C:\ProgramData\Ina\Data\HandPose\PipelineConfig.json"
    };

    // spheres for 3d keypoints
    public GameObject handPrefab;
    private int maxHands = 0;
    private List<HandSpheres> hands = new List<HandSpheres>();

    // Start is called before the first frame update
    void Start()
    {
    }

    public int GetHandsCount()
    {
        return mPosePipeline.GetNumKeyPoints3d();
    }

    public Vector3 GetKeypointPosition(int handIndex, int keypointIndex)
    {
        if (handIndex < hands.Count)
        {
            var hand = hands[handIndex];
            if (keypointIndex < hand.SphereCount)
            {
                var sphere = hand.GetSphere(keypointIndex);
                return sphere.transform.localPosition;
            }
        }
        return Vector3.zero;
    }

    // Update is called once per frame
    void Update()
    {
        if (!mPosePipeline.IsInitialized())
        {
            // set config
            for (int i = 0; i < ConfigFilePaths.Length; i++)
            {
                if (File.Exists(ConfigFilePaths[i]))
                {
                    Debug.Log($"Using configuration file {ConfigFilePaths[i]}");
                    mPosePipeline.InitializePosePipeline(ConfigFilePaths[i]);
                    break;
                }
            }
                        
            if (mPosePipeline.IsInitialized())
            {
                maxHands = mPosePipeline.GetMaxNumHands();
                for (int i = 0; i < maxHands; i++)
                {
                    var newHand = Instantiate(handPrefab);
                    newHand.transform.parent = handPrefab.transform.parent;
                    newHand.transform.localPosition = Vector3.zero;
                    newHand.transform.localEulerAngles = Vector3.zero;
                    hands.Add(newHand.GetComponent<HandSpheres>());
                }
            }
        }

        if (!mRsDeviceInfo.EnsureInit()) return;

        if (!mPosePipeline.IsCalibrated())
        {
            // set num cameras and intrinsics
            int numCameras = mRsDeviceInfo.getNumDevices();
            mCalibrationWrapper.SetNumCameras((uint)numCameras);
            mDataManager.SetNumCameras(numCameras);

            mCalibrationInfo.ReadData();
            for (int i = 0; i < numCameras; i++)
            {
                Intrinsics intrinsics = mRsDeviceInfo.getIntrinsics(i);
                var depthUnits = mRsDeviceInfo.getDepthUnits(i);
                mCalibrationWrapper.SetIntrinsics(i, intrinsics.fx, intrinsics.fy, intrinsics.ppx, intrinsics.ppy, (uint)intrinsics.width, (uint)intrinsics.height, depthUnits);

                // compute and set extrinsics
                var baseline = mRsDeviceInfo.getExtrinsics(i); // ir1 to ir2
                var cameras = mCalibrationInfo.cameras;                
                foreach (var camera in cameras)
                {
                    var serialNumber = camera.SerialNumber;                    
                    if (serialNumber == mRsDeviceInfo.getSerialNumber(i))
                    {
                        for (int j = 0; j < 2; j++)
                        {
                            Matrix4x4 cameraTransform = camera.Rotation;
                            cameraTransform.SetColumn(3, camera.Translation); // Set translation column
                            cameraTransform.SetRow(3, new Vector4(0, 0, 0, 1)); // Set last row

                            Matrix4x4 transformOut = cameraTransform;
                            if (j == 1)
                            {
                                transformOut = InverseRigid(baseline * InverseRigid(cameraTransform));
                            }
                            float[] translationArrayIR = new float[] { transformOut.m03, transformOut.m13, transformOut.m23 };
                            float[] rotationArrayIR = new float[] {
                            transformOut.m00, transformOut.m01, transformOut.m02,
                            transformOut.m10, transformOut.m11, transformOut.m12,
                            transformOut.m20, transformOut.m21, transformOut.m22
                            };

                            mCalibrationWrapper.SetPose(i, j, rotationArrayIR, translationArrayIR); // camera i, infrared j
                        }                        
                    }
                }
                
            }

            mPosePipeline.SetCalibrationFromStructs(mCalibrationWrapper.GetCameraStructs());
        }


        if (mPosePipeline.IsInitialized() && mPosePipeline.IsCalibrated())
        {
            bool allBufferAvailable = true;
            int numCameras = mRsDeviceInfo.getNumDevices();
            for (int i = 0; i < numCameras; i++)
            {
                var ir1Buffer = ir1Renderer[i].GetTexture();
                var ir2Buffer = ir2Renderer[i].GetTexture();

                if (ir1Buffer.pointer != System.IntPtr.Zero)
                {
                    mDataManager.SetImage(ir1Buffer.pointer, ir1Buffer.width, ir1Buffer.height, i, 0);
                    mDataManager.SetImage(ir2Buffer.pointer, ir2Buffer.width, ir2Buffer.height, i, 1);
                }
                else
                {
                    allBufferAvailable = false;
                    break;
                }

                var depthBuffer = depthRenderer[i].GetTexture();
                if (depthBuffer.pointer != System.IntPtr.Zero)
                {
                    mDataManager.SetDepthImage(depthBuffer.pointer, depthBuffer.width, depthBuffer.height, i);
                }
                else
                {
                    allBufferAvailable = false;
                    break;
                }
            }         
            if (allBufferAvailable)
            {
                mPosePipeline.ExecuteWithDepth(mDataManager.GetDataManager(), ShowDebugOpenCV);

                int numKeypoints3d = mPosePipeline.GetNumKeyPoints3d();
                Debug.Log("3D Keypoints: " + numKeypoints3d);

                for (int i = 0; i < maxHands; i++)
                {
                    var hand = hands[i];
                    if (i < numKeypoints3d)
                    {
                        var keypoints3d = mPosePipeline.GetKeyPoints3d(i);
                        if (keypoints3d != null)
                        {
                            for (int j = 0; j < keypoints3d.Length / 4; j++)
                            {
                                if (j < hand.SphereCount)
                                {
                                    var kpx = keypoints3d[j * 4 + 0];
                                    var kpy = keypoints3d[j * 4 + 1];
                                    var kpz = keypoints3d[j * 4 + 2];
                                    var sphere = hand.GetSphere(j);
                                    sphere.transform.localPosition = new Vector3(kpx, kpy, kpz);
                                    
                                }
                            }

                            if (enablePinchDetection)
                            {
                                if (detectPinch(keypoints3d, 0.03f, 0.1f, out Vector3 pinchCenter))
                                {
                                    Debug.Log("Pinch detected at " + pinchCenter);
                                }
                            }
                            
                        }
                    }
                    else
                    {
                        if (hand == null)
                        {
                            Debug.LogError("HAND ERROR");   
                        }
                        for (int j = 0; j < hand.SphereCount; j++)
                        {
                            var sphere = hand.GetSphere(j);
                            sphere.transform.localPosition = new Vector3(999, 999, 999);
                        }
                    }
                    
                }
            }


        }
    }

    private static Matrix4x4 InverseRigid(Matrix4x4 m)
    {
        // Extract rotation (upper 3x3)
        Matrix4x4 R = Matrix4x4.identity;
        R.m00 = m.m00; R.m01 = m.m01; R.m02 = m.m02;
        R.m10 = m.m10; R.m11 = m.m11; R.m12 = m.m12;
        R.m20 = m.m20; R.m21 = m.m21; R.m22 = m.m22;

        // Extract translation
        Vector3 t = new Vector3(m.m03, m.m13, m.m23);

        // Rotation inverse = transpose
        Matrix4x4 Rinv = R.transpose;

        // Compute -R^T * t
        Vector3 tinv = -(Rinv.MultiplyVector(t));

        // Build final matrix
        Matrix4x4 result = Matrix4x4.identity;
        result.m00 = Rinv.m00; result.m01 = Rinv.m01; result.m02 = Rinv.m02;
        result.m10 = Rinv.m10; result.m11 = Rinv.m11; result.m12 = Rinv.m12;
        result.m20 = Rinv.m20; result.m21 = Rinv.m21; result.m22 = Rinv.m22;

        result.m03 = tinv.x;
        result.m13 = tinv.y;
        result.m23 = tinv.z;

        return result;
    }

    private bool detectPinch(float[] keypoints, float pinchThreshold, float kptThreshold, out Vector3 pinchCenter)
    {
        pinchCenter = Vector3.zero;
        bool isPinch = false;

        int thumbTipIndex = 4;
        int indexTipIndex = 8;
        var thumbTip = new Vector3(keypoints[thumbTipIndex * 4 + 0], keypoints[thumbTipIndex * 4 + 1], keypoints[thumbTipIndex * 4 + 2]);
        var thumbScore = keypoints[thumbTipIndex * 4 + 3];
        var indexTip = new Vector3(keypoints[indexTipIndex * 4 + 0], keypoints[indexTipIndex * 4 + 1], keypoints[indexTipIndex * 4 + 2]);
        var indexScore = keypoints[indexTipIndex * 4 + 3];

        float distance = Vector3.Distance(thumbTip, indexTip);
        if (distance < pinchThreshold && thumbScore > kptThreshold && indexScore > kptThreshold)
        {
            pinchCenter = (thumbTip + indexTip) / 2;
            isPinch = true;
        }
        return isPinch;
    }

}
