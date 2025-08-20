using AOT;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.XR.OpenXR.Features;
using System;
using System.Linq;
using UnityEngine.Rendering;
using UnityEngine.XR;
using System.Text;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.XR.OpenXR.Features;
#endif

using System.Collections.Generic;

namespace Unity.XR.Acer
{
#if UNITY_EDITOR
    [OpenXRFeature(UiName = "Acer: Dynamic Frustum",
        BuildTargetGroups = new[] {BuildTargetGroup.Standalone,},
        Company = "Acer",
        Desc = "",
        DocumentationLink = "https://spatiallabs.acer.com/developer",
        OpenxrExtensionStrings = "",
        Version = "0.0.3",
        FeatureId = FeatureId)]
#endif
	public class DynamicFrustumFeature : OpenXRFeature
    {				
		public const string FeatureId = "com.acer.spatil-labs-sdk.feature.camera.dynamic-frustum";

        private const string ExtLib = "AcerSpatialLabs";

        [Header("Field of View")]
		[Tooltip("Specifies the field of view axis")]
        public Camera.FieldOfViewAxis FieldOfViewAxis = Camera.FieldOfViewAxis.Vertical;
		
        [Tooltip("Field of view value used to calculate camera frustum when UseDynamicFov is not set")]
        [Range(0.01f, 179.99f)]		
        public float FieldOfView = 15.0f;

        [Header("Dynamic Field of View")] 
        [Tooltip("Adjust field of view depending on eye position")]
        public bool UseDynamicFov = true;
		
		[Tooltip("This value is multiplied by each scene’s ‘SpatialLabs Scale’ value to allow applying a uniform scale to all scenes in the project")]
        [Min(0.01f)]
        public float WorldScaleFactor = 1.0f;
		public static float SpatialLabsScale = 1.0f;
		private float WorldScale = 1.0f;
			
        private float fixedFovDistance => FieldOfViewAxis switch
        {
            Camera.FieldOfViewAxis.Horizontal => screenSizeHalf.x,
            Camera.FieldOfViewAxis.Vertical => screenSizeHalf.y,
            _ => screenSizeHalf.x,
        } / Mathf.Tan(FieldOfView * Mathf.Deg2Rad * 0.5f);
		
		[SerializeField]
		[Tooltip("The IPD parameter has been superseded by StereoSeparation, and it is scheduled for deprecation in the future")]
		private float _IPD = 0.063f;
		public float IPD
		{
			get { return _IPD; }
			private set { _IPD = 0.063f; }
		}
		public static float FixedIpd = 0.063f;
		
		// New parameter introduced in version 0.0.3
		[Tooltip("Modify the level of Stereoscopic 3D according to the tracked interpupillary distance")]
		[Range(0.0f, 1.0f)]
		public float StereoSeparation = 1.0f;

        [Header("Head Position")] 
        [Tooltip("Set fixed head position instead of head-tracking")]
        public bool UseFixedHeadPos = false;

        [Tooltip("Fixed head position value when UseFixedHeadPos is set")]
        public Vector3 FixedHeadPos = new(0.0f, 0.0f, -0.6f);
		

        [Header("Virtual Screen Settings")] 
		[Tooltip("ScaledSize represents that the virtual screen within 3D scenes will fill the SpatialLabs display; OriginalSize represents that the size of the virtual screen within 3D scenes will be written by the current SpatialLabs display")]
		public scalingMode ScalingMode = scalingMode.ScaledSize;
		
		[Tooltip("Define the size of the virtual screen within 3D scenes")]
        public Vector2 ScreenSize = new(0.344f, 0.193f); // ASV15-1BP(0.34422f, 0.19362f)
		public static Vector2 RealScreenSize = new(0.344f, 0.193f);
		private float spaceScaling => ScalingMode switch
		{
			scalingMode.ScaledSize => ScreenSize.y / RealScreenSize.y,
			scalingMode.OriginalSize => 1.0f,
			_ => 1.0f,
		};
		
        private Vector2 screenSizeHalf => ScalingMode switch
		{
			scalingMode.ScaledSize => ScreenSize * 0.5f,
			scalingMode.OriginalSize => RealScreenSize * 0.5f,
			_ => ScreenSize * 0.5f,
		};
		
		[Tooltip("The frustum gizmo represents a virtual screen and viewpoint within 3D scenes")]
		public bool ShowFrustumGizmo = true;
		
        private static readonly XRNode[] EyeNodes = {XRNode.LeftEye, XRNode.RightEye};
		

        protected override IntPtr HookGetInstanceProcAddr(IntPtr func)
        {
            Internal_SetCallback(OnMessage);
            return intercept_xrCreateSession_xrGetInstanceProcAddr(func);
        }

        protected override bool OnInstanceCreate(ulong xrInstance)
        {
            // Initialize IPD
			FixedIpd = IPD;
			SetPluginParameters(FixedIpd);

            RenderPipelineManager.beginCameraRendering -= BeginCameraRenderingSrp;
            RenderPipelineManager.beginCameraRendering += BeginCameraRenderingSrp;

            Camera.onPreCull -= BeginCameraRenderingBirp;
            Camera.onPreCull += BeginCameraRenderingBirp;

            return true;
        }

        private void BeginCameraRenderingSrp(ScriptableRenderContext context, Camera camera)
        {
            SetPluginParameters(FixedIpd);
        }

        private void BeginCameraRenderingBirp(Camera camera)
        {
            SetPluginParameters(FixedIpd);
        }
		
        private void SetPluginParameters(float ipd)
        {
            {
                WorldScale = WorldScaleFactor * SpatialLabsScale;
				
				var nodes = UnityEngine.Pool.ListPool<XRNodeState>.Get();
                InputTracking.GetNodeStates(nodes);

                var centerEyeNodeState = nodes.FirstOrDefault(x => x.nodeType == XRNode.CenterEye);
                if (centerEyeNodeState.tracked &&
                    centerEyeNodeState.TryGetPosition(out var eyeCenterPosition) &&
                    centerEyeNodeState.TryGetRotation(out var eyeCenterRotation))
                {
					if (centerEyeNodeState.TryGetVelocity(out var centerEyeVelocity))
                    {
                        eyeCenterPosition += centerEyeVelocity;
                    }

                    eyeCenterRotation = Quaternion.identity;

                    for (var iEye = 0; iEye < EyeNodes.Length; iEye++)
                    { 		
						var eyeRotation = Quaternion.identity;						
						
						var eyePosition = Internal_GetEyePosition(iEye);
						Vector3 redirectPosition = new Vector3(1.0f, 1.0f, -1.0f); 
						eyePosition = Vector3.Scale(eyePosition, redirectPosition);
						eyePosition = LerpWithoutClamp( eyeCenterPosition, eyePosition, StereoSeparation );
						
						if (UseFixedHeadPos)
						{
						   eyePosition = eyePosition - eyeCenterPosition + FixedHeadPos;
						}
						
						// Consider space scaling between virtual window and real screen size
						eyePosition *= spaceScaling;
						
						if (!UseDynamicFov)
						{
							// Restrict FOV to a specific visible range
							eyePosition = Internal_ConstrainedFOV(eyePosition, fixedFovDistance);
						}
						
                        UpdateEyePose(iEye, eyeRotation, eyePosition * WorldScale);
                        UpdateEyeFov(iEye, eyePosition, screenSizeHalf);
                    }
                }
                else
                {
                    eyeCenterPosition = Vector3.back * fixedFovDistance;

                    for (var iEye = 0; iEye < EyeNodes.Length; iEye++)
                    {
                        UpdateEyePose(iEye, Quaternion.identity, eyeCenterPosition);
                        UpdateEyeFov(iEye, eyeCenterPosition, screenSizeHalf);
                    }
                }

                UnityEngine.Pool.ListPool<XRNodeState>.Release(nodes);
            }

            Internal_SetUseApplicationFov(true);
        }

        private static void UpdateEyeFov(int iEye, Vector3 eyePosition, Vector2 screenSizeHalf)
        {
			Internal_SetFov(iEye, eyePosition, screenSizeHalf);
        }

        private static void UpdateEyePose(int iEye, Quaternion poseRotation, Vector3 posePosition)
        {
            Internal_SetPose(iEye,
                poseRotation.x, poseRotation.y, poseRotation.z, poseRotation.w,
                posePosition.x, posePosition.y, -posePosition.z);
        }

        private delegate void OnMessageDelegate(string message);

        [MonoPInvokeCallback(typeof(OnMessageDelegate))]
        private static void OnMessage(string message)
        {
            if (message == null)
                return;

            Debug.Log(message);
        }

        private delegate void ReceiveMessageDelegate(string message);

        private delegate void UpdateFromPluginDelegate();

        [DllImport(ExtLib, EntryPoint = "script_set_callback")]
        static extern void Internal_SetCallback(ReceiveMessageDelegate callback);

        [DllImport(ExtLib, EntryPoint = "script_set_updateCallback")]
        static extern void Internal_SetUpdateCallback(UpdateFromPluginDelegate callback);

        [DllImport(ExtLib, EntryPoint = "script_intercept_xrCreateSession_xrGetInstanceProcAddr")]
        private static extern IntPtr intercept_xrCreateSession_xrGetInstanceProcAddr(IntPtr func);

        [DllImport(ExtLib, EntryPoint = "script_set_useApplicationFov")]
        private static extern void Internal_SetUseApplicationFov(bool value);
		
		[DllImport(ExtLib, EntryPoint = "script_set_fov")]
        private static extern void Internal_SetFov(
            int viewIndex, Vector3 eyePosition, Vector2 screenSizeHalf);

        [DllImport(ExtLib, EntryPoint = "script_set_pose")]
        private static extern void Internal_SetPose(
            int viewIndex, float orientationX, float orientationY, float orientationZ, float orientationW,
            float positionX, float positionY, float positionZ);
			
		[DllImport(ExtLib, EntryPoint = "script_constrainedFOV")]
		private static extern Vector3 Internal_ConstrainedFOV(
			Vector3 eyePosition, float focalLength);
		
		[DllImport(ExtLib, EntryPoint = "script_get_eye_position")]
		private static extern Vector3 Internal_GetEyePosition(
			int viewIndex);
		
		public enum scalingMode
		{
			ScaledSize,
			OriginalSize,
		}
		
		private Vector3 LerpWithoutClamp(Vector3 A, Vector3 B, float t)
		{
			return A + (B-A)*t;
		}
		
		// Lock fixed IPD for deprecation in the future
		private void OnValidate()
		{
			IPD = _IPD;
		}
    }
}