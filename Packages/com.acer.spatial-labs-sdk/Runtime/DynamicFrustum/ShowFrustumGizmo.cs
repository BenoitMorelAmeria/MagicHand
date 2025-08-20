using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.XR.OpenXR.Features;
#endif

namespace Unity.XR.Acer
{
#if UNITY_EDITOR
	[InitializeOnLoad]
	public class ShowFrustumGizmo
    {
		private const string ExtLib = "AcerSpatialLabsDevice";
		private static DynamicFrustumFeature dynamicFrustumFeature;
		
		[DllImport(ExtLib, EntryPoint = "getSpatialLabsMonitorSize")]
		private static extern Vector2 getSpatialLabsMonitorSize();
		
		private static Vector2 slMonitorSize = new(0.344f, 0.193f); // ASV15-1BP(0.34422f, 0.19362f)
		
		static ShowFrustumGizmo()
		{	
			// Get details of DynamicFrutumFeature
			dynamicFrustumFeature = LoadDynamicFrustumFeature();
			
			// Parsing SpatialLabs Pro Device
			if (!EditorApplication.isPlayingOrWillChangePlaymode)
			{
				slMonitorSize = getSpatialLabsMonitorSize();
				Debug.Log("[Editor] Size of SpatialLabs Display: x=" + slMonitorSize.x + ", y=" + slMonitorSize.y);
			}
			SceneView.duringSceneGui += OnSceneGUI;
		}
		
		static void OnSceneGUI(SceneView sceneView)
		{		
			if (!EditorApplication.isPlaying && dynamicFrustumFeature != null)
			{
				float worldScaleFactor = dynamicFrustumFeature.WorldScaleFactor;
				Vector3 fixedHeadPos = dynamicFrustumFeature.FixedHeadPos;
				Vector2 screenSize = dynamicFrustumFeature.ScreenSize;
				bool showFrustumGizmo = dynamicFrustumFeature.ShowFrustumGizmo;
				
				if( dynamicFrustumFeature.ScalingMode == DynamicFrustumFeature.scalingMode.OriginalSize )
				{
					screenSize = slMonitorSize;
				}
				
				Camera[] cameras = GameObject.FindObjectsOfType<Camera>();

				if (showFrustumGizmo)
				{
					foreach (Camera camera in cameras)
					{					
						MonoBehaviour[] scripts = camera.GetComponents<MonoBehaviour>();

						foreach (MonoBehaviour script in scripts)
						{
							if (script.GetType().Name.Contains("TrackedPoseDriver"))
							{
								DrawGizmoAtCamera(camera, screenSize.x, screenSize.y, fixedHeadPos, worldScaleFactor);
							}
						}
					}
				}
			}
		}
		
		static void DrawGizmoAtCamera(Camera camera, float length, float width, Vector3 fixedHeadPos, float worldScaleFactor)
		{
			Vector3 cameraPosition = camera.transform.position;
			Quaternion cameraRotation = camera.transform.rotation;
			Vector3 cameraGlobalScale = camera.transform.lossyScale;
			
			// Set Blue-Color of Rectangle
			Handles.color = Color.blue;

			// Four vertices of Rectangle
			Vector3 topLeft = cameraPosition + cameraRotation * (Vector3.left * length * 0.5f * cameraGlobalScale.x + Vector3.up * width * 0.5f * cameraGlobalScale.y) * worldScaleFactor;
			Vector3 topRight = cameraPosition + cameraRotation * (Vector3.right * length * 0.5f * cameraGlobalScale.x + Vector3.up * width * 0.5f * cameraGlobalScale.y) * worldScaleFactor;
			Vector3 bottomLeft = cameraPosition + cameraRotation * (Vector3.left * length * 0.5f * cameraGlobalScale.x + Vector3.down * width * 0.5f * cameraGlobalScale.y) * worldScaleFactor;
			Vector3 bottomRight = cameraPosition + cameraRotation * (Vector3.right * length * 0.5f * cameraGlobalScale.x + Vector3.down * width * 0.5f * cameraGlobalScale.y) * worldScaleFactor;

			// Draw closed line of Rectangle
			Handles.DrawAAPolyLine(10f, topLeft, topRight, bottomRight, bottomLeft, topLeft);
			
			// Set Red-Color of Pyramid
			Handles.color = Color.red;
			
			// Vertice of Viewpoint
			Vector3 headPosition = cameraPosition + cameraRotation * fixedHeadPos * worldScaleFactor * cameraGlobalScale.z;
			
			// Draw closed line of Pyramid
			Handles.DrawAAPolyLine(10f, topLeft, headPosition);
			Handles.DrawAAPolyLine(10f, topRight, headPosition);
			Handles.DrawAAPolyLine(10f, bottomRight, headPosition);
			Handles.DrawAAPolyLine(10f, bottomLeft, headPosition);
		}
		
		static DynamicFrustumFeature LoadDynamicFrustumFeature()
		{
			string assetPath = "Assets/XR/Settings/OpenXR Package Settings.asset";
			DynamicFrustumFeature dynamicFrustumFeature = AssetDatabase.LoadAssetAtPath<DynamicFrustumFeature>(assetPath);
			
			if (dynamicFrustumFeature == null)
			{
				Debug.LogError("Lose defaut asset of OpenXR Package Settings: " + assetPath + "\r\nPlease *RESTART* the editor.");
			}
			else
			{
				Debug.Log("DynamicFrustumFeature has been assigned!");
			}

			return dynamicFrustumFeature;
		}
    }
#endif
}
