using System;
using System.IO;

namespace MHW___Reshade_Injector_Helper.Helpers
{
    public static class ScreenshotHelper
    {
        public static void MoveCaptures()
        {
            //Check Capture folder for MHW Screenshots and move them to the screenshot folder properly
            var screenshots = Directory.GetFiles(Environment.GetFolderPath(Environment.SpecialFolder.MyVideos), "*Monster*", SearchOption.AllDirectories);

            foreach (var file in screenshots)
            {
                var fileInfo = new FileInfo(file);

                var newPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "MHW ReShade Helper", "screenshots", $"MonsterHunterWorld {fileInfo.CreationTime:yyyy-MM-dd HH-mm-ss-fff}{fileInfo.Extension}");
            }
        }
    }
}