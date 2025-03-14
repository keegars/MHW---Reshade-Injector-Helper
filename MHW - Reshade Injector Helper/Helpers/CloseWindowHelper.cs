using System;
using System.Runtime.InteropServices;

namespace MHW___Reshade_Injector_Helper.Helpers
{
    public static class CloseWindowHelper
    {
        internal const uint SC_CLOSE = 0xF060;
        internal const uint MF_ENABLED = 0x00000000;
        internal const uint MF_GRAYED = 0x00000001;
        internal const uint MF_DISABLED = 0x00000002;
        internal const uint MF_BYCOMMAND = 0x00000000;

        public static void EnableCloseButton(IntPtr handle, bool bEnabled)
        {
            var hSystemMenu = GetSystemMenu(handle, false);
            EnableMenuItem(hSystemMenu, SC_CLOSE, MF_ENABLED | (bEnabled ? MF_ENABLED : MF_GRAYED));
        }

        [DllImport("user32.dll")]
        private static extern bool EnableMenuItem(IntPtr hMenu, uint uIDEnableItem, uint uEnable);

        [DllImport("user32.dll")]
        private static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);
    }
}