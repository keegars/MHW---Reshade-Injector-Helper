using System;
using System.Diagnostics;

namespace MHW___Reshade_Injector_Helper.Helpers
{
    public static class ProcessHelper
    {
        public static Process[] GetProcess(string executableName)
        {
            var process = Process.GetProcessesByName(executableName);
            return process.Length < 1 ? throw new Exception("Failed to find executable. Process not running.") : process;
        }

        public static void Kill(string processName)
        {
            foreach (var process in Process.GetProcessesByName(processName))
            {
                process.Kill();
            }
        }

        public static bool Exists(string processName)
        {
            return Process.GetProcessesByName(processName).Length > 0;
        }
    }
}