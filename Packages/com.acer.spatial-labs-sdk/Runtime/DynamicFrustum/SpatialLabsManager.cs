using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR;
using System.Runtime.InteropServices;
using System.Text;
using System;
using Unity.XR.Acer;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

namespace Unity.XR.Acer
{
	public class SpatialLabsManager
	{
		private const string ExtLib = "AcerSpatialLabsDevice";
		
		[DllImport(ExtLib, EntryPoint = "disableSpatialLabsLens")]
		private static extern void disableSpatialLabsLens();
		
		[DllImport(ExtLib, EntryPoint = "enableSpatialLabsLens")]
		private static extern void enableSpatialLabsLens();
		
		[DllImport(ExtLib, EntryPoint = "hideSpatialLabsWindow")]
		private static extern void hideSpatialLabsWindow();
		
		[DllImport(ExtLib, EntryPoint = "showSpatialLabsWindow")]
		private static extern void showSpatialLabsWindow();
		
		[DllImport(ExtLib, EntryPoint = "getSpatialLabsMonitorSize")]
		private static extern Vector2 getSpatialLabsMonitorSize();
		
		private static bool isAppFocused = true; // Define focus state of the Application

		// Called before the Application starts
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
		private static void Initialize()
		{
			Debug.Log("FocusDetectorManager initialized!");
			// Subscribe to the focusChanged event of the Application
			Application.focusChanged += OnApplicationFocusChanged;
			
			// Subscribe to the quitting event of the Application
            Application.quitting += OnApplicationQuitting;
			
			Vector2 slMonitorSize = getSpatialLabsMonitorSize();
			Debug.Log("Size of SpatialLabs Display: x=" + slMonitorSize.x + ", y=" + slMonitorSize.y);
			DynamicFrustumFeature.RealScreenSize = slMonitorSize;
		}

		private static void OnApplicationFocusChanged(bool hasFocus)
		{
		#if UNITY_EDITOR
			if(EditorApplication.isPlaying)
		#endif
			{
				isAppFocused = hasFocus;
				if (isAppFocused)
				{
					// The Application gains focus
					ShowSpatialLabsDisplay();
					EnableSpatialLabsLens();
					Debug.Log("Application focused!");
				}
				else
				{
					// The Application loses focus
					HideSpatialLabsDisplay();
					DisableSpatialLabsLens();
					Debug.Log("Application unfocused!");
				}
			}
		}
		
		// Called when the Application starts
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void OnGameStart()
        {
			Debug.Log("SpatialLabs started!");
        }
		
		// Called before the application quits
        private static void OnApplicationQuitting()
        {
			Debug.Log("SpatialLabs quitting!");
        }
		
		
		// Publis static APIs for Stereoscopic 3D
		public static void DisableSpatialLabsLens()
		{
			disableSpatialLabsLens();
		}
		
		public static void EnableSpatialLabsLens()
		{
			enableSpatialLabsLens();
		}
		
		public static void ShowSpatialLabsDisplay()
		{
			showSpatialLabsWindow();
		}
		
		public static void HideSpatialLabsDisplay()
		{
			hideSpatialLabsWindow();
		}
		
		public static void SetSpatialLabsScale(float scale)
		{
			DynamicFrustumFeature.SpatialLabsScale = scale;
		}
		
		public static float GetSpatialLabsScale()
		{
			return DynamicFrustumFeature.SpatialLabsScale;
		}
	}
}
