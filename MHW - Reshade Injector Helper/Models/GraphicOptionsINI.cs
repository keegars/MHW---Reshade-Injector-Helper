using MadMilkman.Ini;
using MHW___Reshade_Injector_Helper.Constants;
using MHW___Reshade_Injector_Helper.Helpers;
using System;
using System.Linq;

namespace MHW___Reshade_Injector_Helper.Models
{
    public class GraphicsOptionsINI
    {
        public GraphicsOptionsINI(string path)
        {
            GraphicOptionsIni.Load(path);

            var message = "Ini File Loaded.";

            if (string.IsNullOrWhiteSpace(AspectRatio) || UltraWideModeLayout == -1)
            {
                message = "ERROR: graphics_option.ini does not have an option for aspect ratio. Please toggle the aspect ratio setting in-game and restart application";

                ErrorLogHelper.Log(message);
                throw new Exception(message);
            }

            Console.WriteLine(message);
        }

        public IniFile GraphicOptionsIni { get; set; } = new IniFile();

        public string AspectRatio => GraphicOptionsIni?.GetValue<string>(Ini_CVs.ASPECT_RATIO);

        public int ResolutionX => Resolution[0];

        public int ResolutionY => Resolution[1];

        public bool DLSS => GraphicOptionsIni?.GetValue<string>(Ini_CVs.NVIDIA_DLSS) != General_CVs.OFF;

        public bool D3D12 => GraphicOptionsIni?.GetValue<string>(Ini_CVs.DIRECT_X_12_ENABLE) != General_CVs.OFF;

        public int UltraWideModeLayout => GraphicOptionsIni?.GetValue<int>(Ini_CVs.ULTRAWIDE_MODE_UI_LAYOUT) ?? -1;

        private int[] Resolution => GraphicOptionsIni?.GetValue<string>(Ini_CVs.RESOLUTION)?.Split('x').Select(z => int.Parse(z)).ToArray();
    }
}