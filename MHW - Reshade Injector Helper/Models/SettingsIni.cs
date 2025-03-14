using MadMilkman.Ini;
using MHW___Reshade_Injector_Helper.Helpers;

namespace MHW___Reshade_Injector_Helper.Models
{
    public class SettingsIni
    {
        public SettingsIni(string path)
        {
            _path = path;
            IniFile.Load(path);
        }

        public IniFile IniFile { get; set; } = new IniFile();

        public int ResolutionX => IniFile?.GetValue<int>("ResolutionX", "Main") ?? -1;

        public int ResolutionY => IniFile?.GetValue<int>("ResolutionY", "Main") ?? -1;

        public string ApplicationFilePath => IniFile?.GetValue<string>("ApplicationFilePath", "Main");

        public string ApplicationName => IniFile?.GetValue<string>("ApplicationName", "Main");

        public int SteamAppId => IniFile?.GetValue<int>("SteamAppId", "Main") ?? -1;

        public string SteamDataPath => IniFile?.GetValue<string>("SteamDataPath", "Main");

        public string AddressRangeStart => IniFile?.GetValue<string>("AddressRangeStart", "Main");

        public string AddressRangeEnd => IniFile?.GetValue<string>("AddressRangeEnd", "Main");

        public string HUDAddressRangeStart => IniFile?.GetValue<string>("HUDAddressRangeStart", "Main");

        public string HUDAddressRangeEnd => IniFile?.GetValue<string>("HUDAddressRangeEnd", "Main");

        private string _path { get; set; }

        public void SetAddressRangeStart(string value)
        {
            IniFile.Sections["Main"].Keys["AddressRangeStart"].Value = value;
        }

        public void SetAddressRangeEnd(string value)
        {
            IniFile.Sections["Main"].Keys["AddressRangeEnd"].Value = value;
        }

        public void SetHUDAddressRangeStart(string value)
        {
            IniFile.Sections["Main"].Keys["HUDAddressRangeStart"].Value = value;
        }

        public void SetHUDAddressRangeEnd(string value)
        {
            IniFile.Sections["Main"].Keys["HUDAddressRangeEnd"].Value = value;
        }

        public void SaveIni()
        {
            IniFile.Save(_path);
        }
    }
}