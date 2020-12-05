using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace MHW___Reshade_Injector_Helper
{
    internal static class Program
    {
        private static void Main()
        {
            var injector_p_info = new ProcessStartInfo
            {
                FileName = Path.Combine(Environment.CurrentDirectory, "Reshade Injector", "inject.exe"),
                WorkingDirectory = Path.Combine(Path.Combine(Environment.CurrentDirectory, "Reshade Injector")),
                Arguments = "MonsterHunterWorld.exe"
            };

            Process.Start(injector_p_info);

            Thread.Sleep(3 * 1000);

            Process.Start("steam://run/582010");

            Environment.Exit(1);
        }
    }
}