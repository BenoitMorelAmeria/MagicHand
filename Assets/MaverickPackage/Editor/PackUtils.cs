using System;
using System.IO;
using UnityEngine;

namespace Ameria.Maverick
{
    public static class PackUtils
    {
        public const string BUILD = "Build/";
        public const string SOURCE = "Source/";

        public static string GetBuildPath()
        {
            return $"{Application.dataPath.Replace("/Assets", "/")}{BUILD}";
        }

        public static string GetBuildSourcePath(string fileName = "")
        {
            return $"{Application.dataPath.Replace("/Assets", "/")}{BUILD}{SOURCE}{fileName}";
        }

        public static void CleanBuildDirectory()
        {
            var buildPath = GetBuildPath();
            FileUtils.EnsureDirectory(buildPath);
            FileUtils.ClearDirectory(buildPath);
        }

        public static void CopyImage(Texture2D texture2D)
        {
            CopyImage(GetBuildPath(), texture2D);
        }

        public static void CopyImage(string path, Texture2D texture2D)
        {
            var bytes = texture2D.EncodeToPNG();
            File.WriteAllBytes(path, bytes);
        }

    }
}