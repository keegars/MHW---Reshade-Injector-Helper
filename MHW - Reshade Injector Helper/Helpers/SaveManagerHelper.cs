using MHW___Reshade_Injector_Helper.Constants;
using MHW___Reshade_Injector_Helper.Models;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MHW___Reshade_Injector_Helper.Helpers
{
    public static class SaveManagerHelper
    {
        public static async Task ScheduledBackupSaveAsync(SettingsIni settings, CancellationToken token)
        {
            var totalWaitTime = new TimeSpan();

            do
            {
                //Clear console and backup the save data
                Console.Clear();

                BackupSave(settings.SteamDataPath, settings.SteamAppId);

                Console.WriteLine();
                Console.WriteLine("Awaiting User Input, press ENTER to quit OR press BACKSPACE to reapply aspect ratio patch...");

                //Loop until the desired input is pressed or total wait time is exceeded
                do
                {
                    await Task.Delay(General_CVs.WaitTime);
                    totalWaitTime.Add(General_CVs.WaitTime);
                } while (totalWaitTime < General_CVs.SaveTime && !token.IsCancellationRequested);

                //Reset wait time
                totalWaitTime = new TimeSpan();
            } while (!token.IsCancellationRequested);
        }

        private static void BackupSave(string steamDataFolder, int appId)
        {
            var saveFolderLocation = Path.Combine(Environment.CurrentDirectory, "Save Backup Folder");

            if (!Directory.Exists(saveFolderLocation))
            {
                Directory.CreateDirectory(saveFolderLocation);
            }

            var saveFolders = Directory.GetDirectories(saveFolderLocation);
            if (saveFolders.Length >= General_CVs.SaveLimit)
            {
                var weakestFolder = saveFolders.OrderBy(z => z).First();
                Directory.Delete(weakestFolder, true);
            }

            var gameFolders = Directory.GetDirectories(steamDataFolder, appId.ToString(), SearchOption.AllDirectories).Where(z => !z.Contains(@"\760\"));

            foreach (var folder in gameFolders)
            {
                var tmpName = folder.Substring(steamDataFolder.Length + 1, folder.Length - steamDataFolder.Length - 1).Replace(@"\", "_");
                var newFolderName = Path.Combine(saveFolderLocation, tmpName);

                CopyAll(folder, newFolderName, true);
            }
        }

        private static void CopyAll(string sourceDirectory, string targetDirectory, bool isRootFolder = false)
        {
            if (isRootFolder)
            {
                targetDirectory += $"_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}";
            }

            var source = new DirectoryInfo(sourceDirectory);
            var target = new DirectoryInfo(targetDirectory);

            Console.WriteLine($"{DateTime.Now:dd_MM_yyyy HH:mm:ss} - Copying from {sourceDirectory} to {targetDirectory}");

            Directory.CreateDirectory(target.FullName);

            // Copy each file into the new directory.
            foreach (var fi in source.GetFiles())
            {
                fi.CopyTo(Path.Combine(target.FullName, fi.Name), true);
            }

            // Copy each subdirectory using recursion.
            foreach (var diSourceSubDir in source.GetDirectories())
            {
                var nextTargetSubDir = target.CreateSubdirectory(diSourceSubDir.Name);
                CopyAll(diSourceSubDir.FullName, nextTargetSubDir.FullName);
            }

            if (isRootFolder)
            {
                Console.WriteLine($"{DateTime.Now:dd-mm-yyyy HH:mm:ss} - Completed");
            }
        }
    }
}