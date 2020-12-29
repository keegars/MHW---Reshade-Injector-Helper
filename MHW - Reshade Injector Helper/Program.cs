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
            try
            {
                var injectorInfo = new ProcessStartInfo
                {
                    FileName = Path.Combine(Environment.CurrentDirectory, "inject.exe"),
                    WorkingDirectory = Environment.CurrentDirectory,
                    Arguments = "MonsterHunterWorld.exe"
                };

                Process.Start(injectorInfo);

                Thread.Sleep(3 * 1000);

                Process.Start("steam://run/582010");
            }
            catch (Exception ex)
            {
                File.AppendAllText(Path.Combine(Environment.CurrentDirectory, "ErrorLog.txt"), $"{Environment.NewLine} {DateTime.Now:dd/MM/yyyy HH:mm:ss} {Environment.NewLine}  {ex.Source} {ex.Message} -  {ex.InnerException} - {ex.StackTrace}");
            }
        }
    }
}