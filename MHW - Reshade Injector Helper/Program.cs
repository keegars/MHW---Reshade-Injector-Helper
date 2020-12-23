using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;

namespace MHW___Reshade_Injector_Helper
{
    internal static class Program
    {
        private static void Main()
        {
            try
            {
                var injectorPath = ExtractResource("inject.exe", Environment.CurrentDirectory);
                ExtractResource("ReShade.ini", Environment.CurrentDirectory);
                ExtractResource("ReShade64.dll", Environment.CurrentDirectory);

                var injectorInfo = new ProcessStartInfo
                {
                    FileName = injectorPath,
                    WorkingDirectory = Environment.CurrentDirectory,
                    Arguments = "MonsterHunterWorld.exe"
                };

                Process.Start(injectorInfo);

                Thread.Sleep(3 * 1000);

                Process.Start("steam://run/582010");

                Environment.Exit(1);
            }
            catch (Exception ex)
            {
                File.AppendAllText(Path.Combine(Environment.CurrentDirectory, "ErrorLog.txt"), $"{Environment.NewLine} {DateTime.Now:dd/MM/yyyy HH:mm:ss} {Environment.NewLine}  {ex.Source} {ex.Message} -  {ex.InnerException} - {ex.StackTrace}");
            }
        }

        private static string ExtractResource(string name, string destinationPath)
        {
            var destination = Path.Combine(destinationPath, name);

            if (File.Exists(destination))
            {
                return destination;
            }

            var currentAssembly = Assembly.GetExecutingAssembly();
           
            using (var resourceStream = currentAssembly.GetManifestResourceStream($"MHW___Reshade_Injector_Helper.{name}"))
            using (var fileStream = File.Create(destination))
            {
                resourceStream.Seek(0, SeekOrigin.Begin);
                resourceStream.CopyTo(fileStream);
            }

            return destination;
        }
    }
}