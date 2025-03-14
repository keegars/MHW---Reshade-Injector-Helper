using System;

namespace MHW___Reshade_Injector_Helper.Constants
{
    public static class General_CVs
    {
        //Save limit
        public const int SaveLimit = 100;

        //Exe names
        public const string InjectExe = "inject.exe";

        //User Input Keys
        public const ConsoleKey DESIRED_INPUT = ConsoleKey.Enter;
        public const ConsoleKey ARPATCH_INPUT = ConsoleKey.Backspace;
        //Off/On
        public const string OFF = "Off";
        public const string ON = "On";
        //Timers
        public static readonly TimeSpan SaveTime = new TimeSpan(0, 15, 0); // 15 minutes
        public static readonly TimeSpan WaitTime = new TimeSpan(0, 0, 10); // 10 seconds
    }
}