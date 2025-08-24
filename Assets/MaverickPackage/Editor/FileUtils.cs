using System;
using System.IO;
using System.Security.Cryptography;

namespace Ameria.Maverick
{
    public static class FileUtils
    {
        public static void CopyDirectory(string sourceDirName, string destDirName, bool copySubDirs, bool overwrite)
        {
            var dir = new DirectoryInfo(sourceDirName);
            var dirs = dir.GetDirectories();

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
            }

            CopyFiles(sourceDirName, destDirName, overwrite);

            if (copySubDirs)
            {
                foreach (var subdir in dirs)
                {
                    var temppath = Path.Combine(destDirName, subdir.Name);
                    CopyDirectory(subdir.FullName, temppath, copySubDirs, overwrite);
                }
            }
        }

        public static void CopyFiles(string sourceDirName, string destDirName, bool overwrite)
        {
            if (!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
            }

            var sourceDir = new DirectoryInfo(sourceDirName);

            var files = sourceDir.GetFiles();
            foreach (var file in files)
            {
                if (file.Name.Contains(".meta"))
                {
                    continue;
                }

                var temppath = Path.Combine(destDirName, file.Name);
                file.CopyTo(temppath, overwrite);
            }
        }

        public static void ClearDirectory(string dirName, bool needToDeleteFolder = false)
        {
            var directoryInfo = new DirectoryInfo(dirName);
            var dirs = directoryInfo.GetDirectories();

            if (!directoryInfo.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + dirName);
            }

            ClearFiles(dirName);

            foreach (var subdir in dirs)
            {
                var temppath = Path.Combine(dirName, subdir.Name);
                ClearDirectory(temppath);
                Directory.Delete(temppath);
            }

            if (needToDeleteFolder)
            {
                Directory.Delete(dirName);
            }
        }

        public static void ClearFiles(string dirName)
        {
            if (!Directory.Exists(dirName))
            {
                return;
            }

            var sourceDir = new DirectoryInfo(dirName);

            var files = sourceDir.GetFiles();
            foreach (var file in files)
            {
                file.Delete();
            }
        }

        public static void EnsureDirectory(string path)
        {
            if (path.Contains("file:///"))
            {
                path = path.Replace("file:///", string.Empty);
            }

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        public static string GetMd5HashFromFile(string fileName)
        {
            using (var fileStream = File.OpenRead(fileName))
            {
                var md5 = new MD5CryptoServiceProvider();
                byte[] fileData = new byte[fileStream.Length];
                fileStream.Read(fileData, 0, (int)fileStream.Length);
                byte[] checkSum = md5.ComputeHash(fileData);
                var result = BitConverter.ToString(checkSum).Replace("-", string.Empty);
                return result.ToLower();
            }
        }

        public static string[] GetDirectoryNames(string targetDir)
        {
            var result = Directory.GetDirectories(targetDir);
            for (var i = 0; i < result.Length; i++)
            {
                result[i] = result[i].Split('\\')[1];
            }

            return result;
        }
    }
}