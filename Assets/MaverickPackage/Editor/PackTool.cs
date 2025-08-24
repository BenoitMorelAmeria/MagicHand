using System.Diagnostics;

namespace Ameria.Maverick
{
    public static class PackTool
    {
        public static bool Install()
        {
            return ExecuteProcess("dotnet", "tool install --global Maverick.Pack.Tool", "");
        }

        public static bool Uninstall()
        {
            return ExecuteProcess("dotnet", "tool uninstall --global Maverick.Pack.Tool", "");
        }

        public static bool IsInstalled() 
        {
            return ExecuteProcess("maverick-pack", "--version", "");
        }

        public static bool BuildPackage(string path)
        {
            return ExecuteProcess("maverick-pack", "build", path);
        }


        private static bool ExecuteProcess(string name, string arguments, string path)
        {
            Process process = new Process();
            process.StartInfo.FileName = name;
            process.StartInfo.Arguments = arguments;
            process.StartInfo.WorkingDirectory = path;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.CreateNoWindow = true;

            // Standard Output handler
            process.OutputDataReceived += (sender, args) =>
            {
                if (!string.IsNullOrEmpty(args.Data))
                {
                    UnityEngine.Debug.Log($"Process output: {args.Data}");
                }
            };

            // Standard Error handler
            process.ErrorDataReceived += (sender, args) =>
            {
                if (!string.IsNullOrEmpty(args.Data))
                {
                    UnityEngine.Debug.LogError($"Process error: {args.Data}");
                }
            };

            try
            {
                // Start the process
                process.Start();

                // Begin reading output asynchronously
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                // Wait for the process to exit
                process.WaitForExit();

                // Handle the exit code (0 = success, non-zero = error)
                int exitCode = process.ExitCode;
                if (exitCode == 0)
                {
                    UnityEngine.Debug.Log("Process executed successfully.");
                    return true;
                }
                else
                {
                    UnityEngine.Debug.LogError($"Process returned error code: {exitCode}");
                    return false;
                }
            }
            catch (System.Exception ex)
            {
                UnityEngine.Debug.LogError($"Error executing process: {ex.Message}");
                return false;
            }
            finally
            {
                // Cleanup
                process.Close();
            }
        }

    }
}