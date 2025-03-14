using MadMilkman.Ini;
using MHW___Reshade_Injector_Helper.Constants;
using MHW___Reshade_Injector_Helper.Helpers;
using MHW___Reshade_Injector_Helper.Models;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Threading;
using System.Threading.Tasks;

namespace MHW___Reshade_Injector_Helper
{
    internal static class Program
    {
        private static ConsoleKeyInfo USER_INPUT;

        private static async Task Main()
        {
            try
            {
                //Disable close button
                CloseWindowHelper.EnableCloseButton(Process.GetCurrentProcess().MainWindowHandle, false);

                //Read INI file for application settings
                SettingsIni settings;
                var iniFileInfo = new FileInfo(Path.Combine(Environment.CurrentDirectory, $"{Process.GetCurrentProcess().ProcessName}.ini"));

                //Check to make sure it exists, if not, create default settings
                if (!iniFileInfo.Exists)
                {
                    using (var iniFS = iniFileInfo.Create())
                    {
                        //Create main section
                        var tmpSettings = new IniFile();
                        var mainSection = tmpSettings.Sections.Add("Main");

                        //Add Resolution of main display
                        var mydisplayResolution = new ManagementObjectSearcher("SELECT CurrentHorizontalResolution, CurrentVerticalResolution FROM Win32_VideoController");
                        foreach (ManagementObject record in mydisplayResolution.Get())
                        {
                            var resX = record["CurrentHorizontalResolution"];
                            var resY = record["CurrentVerticalResolution"];

                            mainSection.Keys.Add("ResolutionX", resX.ToString());
                            mainSection.Keys.Add("ResolutionY", resY.ToString());

                            //We only want the first one
                            break;
                        }

                        //Application Name
                        Console.WriteLine("Please select the application you wish to inject into");
                        var applicationFileName = OpenFileDialogHelper.ShowOpenDialog();

                        var exists = File.Exists(applicationFileName);

                        var applicationFileInfo = new FileInfo(applicationFileName);

                        mainSection.Keys.Add("ApplicationFilePath", applicationFileInfo.DirectoryName);
                        mainSection.Keys.Add("ApplicationName", Path.GetFileNameWithoutExtension(applicationFileInfo.Name));

                        //Get Steam Id from acf, in steamapps folder
                        string appId = string.Empty;

                        if (applicationFileInfo.FullName.Contains("steamapps"))
                        {
                            var steamAppFolderName = applicationFileInfo.FullName.Substring(0, applicationFileInfo.FullName.IndexOf("steamapps") + 10);
                            var acfFiles = Directory.GetFiles(steamAppFolderName, "*.acf", SearchOption.TopDirectoryOnly);

                            foreach (var acfFile in acfFiles)
                            {
                                var contents = File.ReadAllLines(acfFile);
                                var installDirLine = contents.FirstOrDefault(z => z.Contains("installdir") && z.Contains(applicationFileInfo.Directory.Name));

                                if (installDirLine != null)
                                {
                                    var appIdLine = contents.FirstOrDefault(z => z.Contains("appid"));
                                    appId = string.Concat(appIdLine.Where(z => char.IsDigit(z)));

                                    break;
                                }
                            }
                        }

                        mainSection.Keys.Add("SteamAppId", appId);

                        //Set steam data path
                        mainSection.Keys.Add("SteamDataPath", @"C:\Program Files (x86)\Steam\userdata");

                        //Set address ranges
                        mainSection.Keys.Add("AddressRangeStart", "0");
                        mainSection.Keys.Add("AddressRangeEnd", "130000000");
                        mainSection.Keys.Add("HUDAddressRangeStart", "0");
                        mainSection.Keys.Add("HUDAddressRangeEnd", "130000000");

                        tmpSettings.Save(iniFS);
                    }
                }

                //Read ini file
                settings = new SettingsIni(iniFileInfo.FullName);

                //Kill any existing processes
                ProcessHelper.Kill(settings.ApplicationName);

                await Task.Delay(1 * 1000);

                //Launch injector
                var injectorInfo = new ProcessStartInfo
                {
                    FileName = Path.Combine(Environment.CurrentDirectory, "inject.exe"),
                    WorkingDirectory = Environment.CurrentDirectory,
                    Arguments = $"{settings.ApplicationName}.exe",
                    UseShellExecute = false
                };

                Process.Start(injectorInfo);

                //Launch Game
                Thread.Sleep(2 * 1000);

                Process.Start($"steam://run/{settings.SteamAppId}");

                Thread.CurrentThread.Priority = ThreadPriority.BelowNormal;

                //Schedule backup of saves
                var backupTaskCancellationSource = new CancellationTokenSource();
                var backupTask = Task.Run(() => SaveManagerHelper.ScheduledBackupSaveAsync(settings, backupTaskCancellationSource.Token));

                Thread.Sleep(1 * 1000);

                Console.WriteLine("Applying patch after 30 seconds");

                if (!Debugger.IsAttached)
                {
                    var arPatchTask = Task.Run(() =>
                    {
                        Thread.Sleep(30 * 1000);
                        new AspectRatioPatcher().ApplyPatch(settings);
                        Console.WriteLine("Patch applied");
                    });
                }
               
                //Await user to press desired input(ENTER at present)
                Console.WriteLine("Please press ENTER to exit ");

                do
                {
                    do
                    {
                        USER_INPUT = Console.ReadKey();
                    } while (USER_INPUT.Key != General_CVs.DESIRED_INPUT && USER_INPUT.Key != General_CVs.ARPATCH_INPUT);

                    //Reapply patch
                    if (USER_INPUT.Key == General_CVs.ARPATCH_INPUT)
                    {
                        new AspectRatioPatcher().ApplyPatch(settings);
                    }
                } while (USER_INPUT.Key != General_CVs.DESIRED_INPUT);

                //Wait for backup task to exit, just in case it is executing
                backupTaskCancellationSource.Cancel();
                backupTask.Wait();

                //Wait 10 seconds for game to finish hopefully writing any data, then kill game and kill inject
                if (ProcessHelper.Exists(settings.ApplicationName))
                {
                    Console.WriteLine($"Waiting 10 seconds for {settings.ApplicationName} to finish up writing data...");

                    await Task.Delay(10 * 1000);
                    ProcessHelper.Kill(settings.ApplicationName);
                }

                ProcessHelper.Kill("inject");

                //Check Capture folder for MHW Screenshots and move them to the screenshot folder properly
                ScreenshotHelper.MoveCaptures();
            }
            catch (Exception ex)
            {
                ErrorLogHelper.Log(ex);
            }
        }
    }
}