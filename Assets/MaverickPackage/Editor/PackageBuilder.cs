using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Newtonsoft.Json;

namespace Ameria.Maverick
{
    public class PackageBuilder
    {
        private PackageData _packageData;
        private Texture2D _packageImage;

        private static PackageBuilder _instance;

        private static void CreateInstanceIfNeeded()
        {
            if (_instance == null)
            {
                _instance = new PackageBuilder();
            }
        }


        [MenuItem("Build/Maverick/Build Package", priority = 0)]
        public static void BuildPackage()
        {
            CreateInstanceIfNeeded();
            _instance.LoadPackageData();
            _instance.Build();
        }

        private void LoadPackageData()
        {
            var guid = AssetDatabase.FindAssets("t:PackageData", new[] { "Assets/MaverickPackage" }).First();
            var assetPath = AssetDatabase.GUIDToAssetPath(guid);
            _packageData = AssetDatabase.LoadAssetAtPath<PackageData>(assetPath);
        }

        private void Build()
        {
            if (_instance._packageImage != null)
            {
                UnityEngine.Object.Destroy(_instance._packageImage);
            }
            _instance._packageImage = AssetDatabase.LoadAssetAtPath<Texture2D>(_instance._packageData.IconPath);
            _instance.BuildInternal();
        }

        private void BuildInternal()
        {
            var buildDirectory = PackUtils.GetBuildPath();
            FileUtils.EnsureDirectory(buildDirectory);

            // Cleanup build directory
            FileUtils.ClearDirectory(buildDirectory);

            var buildPath = PackUtils.GetBuildSourcePath();

            //PlayerSettings.productName = _packageData.Id;
            var scenes = _packageData.Scenes;
            if (scenes == null || scenes.Length == 0)
            {
                Debug.LogError("No scenes included! Go to package data and add them!");
                return;
            }

            // Build unity scenes
            var editorScenes = new EditorBuildSettingsScene[scenes.Length];
            for (int i = 0; i < scenes.Length; i++)
            {
                editorScenes[i] = new EditorBuildSettingsScene(scenes[i], true);
            }
            BuildPipeline.BuildPlayer(editorScenes,
                buildPath + PlayerSettings.productName + ".exe", BuildTarget.StandaloneWindows64, BuildOptions.None);

            // Copy app icon
            string iconPath = $"{buildDirectory}/{PackageConstants.PACKAGE_ICON_NAME}";
            PackUtils.CopyImage(iconPath, _packageImage);

            // Create mpackage.json file
            var package = new Package
            {
                Schema = PackageConstants.PACKAGE_SCHEMA_APP,
                Id = _packageData.Id,
                Version = _packageData.Version,
                Title = _packageData.Title,
                Icon = PackageConstants.PACKAGE_ICON_NAME,
                Content = $"./{PackUtils.SOURCE}",
                Dependencies = new string[] {},
                Executable = new string[] { $"{PlayerSettings.productName}.exe" },
                Install = new string[] { },
                StartType = Enum.GetName(typeof(StartType), StartType.None),
                RestartBehaviour = Enum.GetName(typeof(RestartBehaviour), RestartBehaviour.Restart)
            };

            using (var fs = File.CreateText(buildDirectory + $"/{PackageConstants.PACKAGE_JSON_FILE}"))
            {
                fs.Write(JsonConvert.SerializeObject(package, Formatting.Indented));
            }

            // Try to execute external pack tool
            if (PackTool.IsInstalled())
            {
                PackTool.BuildPackage(buildDirectory);
            }
            else
            {
                Debug.LogError("Can't build package. Please install pack tool via menu 'Build/Maverick/Install Pack Tool' !");
            }

            // Show in explorer
            EditorUtility.RevealInFinder(buildDirectory);
        }


        [MenuItem("Build/Maverick/Install Pack Tool", priority = 100)]
        public static void InstallPackTool()
        {
            PackTool.Install();
        }

    }
}