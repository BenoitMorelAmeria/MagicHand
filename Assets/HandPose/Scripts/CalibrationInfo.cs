using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Newtonsoft.Json.Linq;

namespace Ameria.SpatialHand
{
    public class CalibrationInfo : MonoBehaviour
    {
        private string[] CalibrationFilePaths =
            {
                @"C:\ProgramData\Ina\Data\SH\calib_param_v1.json", //default SH calibration file
                @"C:\ProgramData\Ina\Data\calib_param_v1.json"     //fallback calibration file without SH subfolder
            };

        public static CalibrationInfo Instance { get; private set; }

        public string ScreenName { get; private set; }
        public float ScreenWidth { get; private set; }
        public float ScreenHeight { get; private set; }
        public ScreenOrientation ScreenOrientation { get; private set; }

        public class CameraInfo
        {
            public string SerialNumber { get; internal set; }
            public Vector3 Translation { get; internal set; }
            public Matrix4x4 Rotation { get; internal set; }
        }

        public List<CameraInfo> cameras = new List<CameraInfo>();


        private void Awake()
        {
            Instance = this;
        }

        public void ReadData()
        {
            string path = "";
            for (int i = 0; i < CalibrationFilePaths.Length; i++)
            {
                if (File.Exists(CalibrationFilePaths[i]))
                {
                    Debug.Log($"Using calibration file {CalibrationFilePaths[i]}");
                    path = CalibrationFilePaths[i];
                    break;
                }
            }
            if (string.IsNullOrEmpty(path))
            {
                var message = $"The calibration file doesn't exist or wasn't found at path: {path} \n Please check your calibration!";
                Debug.LogError(message);
                // AlertManager.ShowError(message);
                return; // we can't continue
            }

            Debug.Log("Read calibration data...");
            var json = File.ReadAllText(path);
            var data = JObject.Parse(json);

            // parse screen name
            ScreenName = data["screen_struct"]["monitor_name"].Value<string>();

            // parse screen width and height
            ScreenWidth = data["screen_struct"]["screen_size"]["0"].Value<float>();
            ScreenHeight = data["screen_struct"]["screen_size"]["1"].Value<float>();

            // parse screen orientation
            string orientation = data["screen_struct"]["screen_orientation"].Value<string>();
            switch (orientation)
            {
                case string value when value.Equals("PORTRAIT", StringComparison.OrdinalIgnoreCase):
                    ScreenOrientation = ScreenOrientation.Portrait;
                    break;
                case string value when value.Equals("LANDSCAPE", StringComparison.OrdinalIgnoreCase):
                    ScreenOrientation = ScreenOrientation.LandscapeLeft;
                    break;
            }

            // parse cameras
            int numberOfCameras = data["camera_structs"].Count();
            Debug.Log($"Calibration file contains {numberOfCameras} cameras.");

            for (int cameraId = 0; cameraId < numberOfCameras; cameraId++)
            {
                string serial = data["camera_structs"][cameraId]["serial"].Value<string>();
                var cameraPose = data["camera_structs"][cameraId]["camera_pose"];

                var translation = new Vector3();
                var rotation = new Matrix4x4();
                for (int i = 0; i < 3; i++)
                {
                    for (int j = 0; j < 3; j++)
                    {
                        rotation[i, j] = cameraPose["R"][i.ToString() + j.ToString()].Value<float>();
                    }
                    translation[i] = cameraPose["t"][i.ToString()].Value<float>();
                }
                rotation[3, 3] = 1f;

                CameraInfo cameraInfo = new CameraInfo();
                cameraInfo.SerialNumber = serial;
                cameraInfo.Translation = translation;
                cameraInfo.Rotation = rotation;

                cameras.Add(cameraInfo);
            }

        }

        public override string ToString()
        {
            return $"Screen Info:" +
                $"\nName: {ScreenName}" +
                $"\nSize (m): {ScreenWidth} x {ScreenHeight}" +
                $"\nOrientation: {ScreenOrientation.ToString()}";
        }
    }
}