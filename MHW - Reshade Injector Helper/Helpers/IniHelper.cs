using MadMilkman.Ini;
using MHW___Reshade_Injector_Helper.Constants;
using System;

namespace MHW___Reshade_Injector_Helper.Helpers
{
    public static class IniHelper
    {
        public static T GetValue<T>(this IniFile iniFile, string key, string section = Ini_CVs.GRAPHIC_OPTIONS_SECTION)
        {
            return iniFile?.Sections[section]?.Keys[key]?.Value == null ?
                default :
                (T)Convert.ChangeType(iniFile.Sections[section].Keys[key].Value, typeof(T));
        }
    }
}