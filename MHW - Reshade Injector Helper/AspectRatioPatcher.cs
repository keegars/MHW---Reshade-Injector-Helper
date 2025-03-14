using MHW___Reshade_Injector_Helper.Constants;
using MHW___Reshade_Injector_Helper.Helpers;
using MHW___Reshade_Injector_Helper.Models;
using System;
using System.Diagnostics;
using System.IO;

namespace MHW___Reshade_Injector_Helper
{
    public class AspectRatioPatcher
    {
        public void ApplyPatch(SettingsIni settings)
        {
            Console.WriteLine("Applying Patch.");

            var graphicsOptionsIni = new GraphicsOptionsINI(Path.Combine(settings.ApplicationFilePath, "graphics_option.ini"));

            if (graphicsOptionsIni.D3D12 || graphicsOptionsIni.DLSS)
            {
                Console.WriteLine("ERROR: DirectX 12 and/or DLSS detected. These are not supported. Please disable them and restart the game.");
                return;
            }

            //if (graphicsOptionsIni.AspectRatio == General_CVs.OFF)
            //{
            //    Console.WriteLine("ERROR: Aspect Ratio mode is set to 16:9. Please set to 21:9 in the game options.");
            //    return;
            //}

            byte[] searchTarget = MHWToolsHelper.GenerateSearchTarget(graphicsOptionsIni);
            var searchTargetConfig = new BytePatternConfig(searchTarget, ulong.Parse(settings.AddressRangeStart), ulong.Parse(settings.AddressRangeEnd), WindowsMemoryHelper.RegionPageProtection.PAGE_READWRITE);
            var searchTargetFinal = new BytePattern(searchTargetConfig);

            Process[] gameProcess;
            try
            {
                gameProcess = ProcessHelper.GetProcess(settings.ApplicationName);
                foreach (Process proc in gameProcess)
                {
                    if (proc.MainWindowTitle.Contains("MONSTER HUNTER: WORLD"))
                    {
                        gameProcess[0] = proc;
                        Console.WriteLine("Found MH:W");
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorLogHelper.Log("WINDOWS ERROR: GetProcessByName critical error.", ex);
                return;
            }

            ulong baseAddress;
            try
            {
                baseAddress = MHWToolsHelper.GetAddressByteArray(gameProcess[0], searchTargetFinal, searchTargetFinal.AddressRange.Start, searchTargetFinal.AddressRange.End);
                if (baseAddress == 0)
                {
                    baseAddress = MHWToolsHelper.GetAddressByteArray(gameProcess[0], searchTargetFinal, 0, 1152921504606846975);
                    if (baseAddress == 0)
                    {
                        var error = $"Search target = {searchTargetFinal.Bytes} graphics_option.ini | Resolution = " + graphicsOptionsIni.ResolutionX.ToString() + "x" + graphicsOptionsIni.ResolutionY.ToString() + " Aspect Ratio = " + graphicsOptionsIni.AspectRatio + " Address range | " + settings.AddressRangeStart + " ==> " + settings.AddressRangeEnd;
                        ErrorLogHelper.Log(error);
                        return;
                    }
                }

                Console.WriteLine("Found base address for resolution...");

                settings.SetAddressRangeStart(Convert.ToUInt64(baseAddress - 1000000).ToString());
                settings.SetAddressRangeEnd(Convert.ToUInt64(baseAddress + 1000000).ToString());
                settings.SaveIni();

                var newResolutionArrays = MHWToolsHelper.GenerateNewResolutionArrays(BitConverter.GetBytes(settings.ResolutionX), BitConverter.GetBytes(settings.ResolutionY));
                try
                {
                    MHWToolsHelper.WriteNewResolution(gameProcess[0], baseAddress, newResolutionArrays);
                    Console.WriteLine("Resolution applied.");
                }
                catch (Exception ex)
                {
                    ErrorLogHelper.Log("ERROR: Failed to write to memory for new Resolution arrays.", ex);
                    return;
                }        

                byte[] hudAspectRatioSearchTargetArray = new byte[] { 0x1F, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x04, 0x00, 0x00, 0x00, 0x03, 0x00, 0x00, 0x00, 0x33, 0x33, 0x73, 0x3F, 0x00, 0x00, 0x80, 0x3E, 0x02, 0x00, 0x00, 0x00, 0x02, 0x00, 0x00, 0x00 };

                var hudAspectRatioSearchConfig = new BytePatternConfig(hudAspectRatioSearchTargetArray, ulong.Parse(settings.HUDAddressRangeStart), ulong.Parse(settings.HUDAddressRangeEnd), WindowsMemoryHelper.RegionPageProtection.PAGE_READWRITE);
                var hudAspectRatioSearchFinal = new BytePattern(hudAspectRatioSearchConfig);

                baseAddress = MHWToolsHelper.GetAddressByteArray(gameProcess[0], hudAspectRatioSearchFinal, hudAspectRatioSearchFinal.AddressRange.Start, hudAspectRatioSearchFinal.AddressRange.End);
                if (baseAddress == 0)
                {
                    baseAddress = MHWToolsHelper.GetAddressByteArray(gameProcess[0], hudAspectRatioSearchFinal, 0, 1152921504606846975);
                    if (baseAddress == 0)
                    {
                        var error = $"Search target = {hudAspectRatioSearchFinal.Bytes} graphics_option.ini | Resolution = " + graphicsOptionsIni.ResolutionX.ToString() + "x" + graphicsOptionsIni.ResolutionY.ToString() + " Aspect Ratio = " + graphicsOptionsIni.AspectRatio + " Address range | " + hudAspectRatioSearchFinal.AddressRange.Start + " ==> " + hudAspectRatioSearchFinal.AddressRange.End;
                        ErrorLogHelper.Log(error);
                        return;
                    }
                }

                Console.WriteLine("Found base address for HUD...");

                settings.SetHUDAddressRangeStart(Convert.ToUInt64(baseAddress - 1000000).ToString());
                settings.SetHUDAddressRangeEnd(Convert.ToUInt64(baseAddress + 1000000).ToString());
                settings.SaveIni();

                var newHudAspect = (float)settings.ResolutionX / settings.ResolutionY;
                byte[] newHudAspectRatio = BitConverter.GetBytes(newHudAspect);

                try
                {
                    MHWToolsHelper.WriteSingleArray(gameProcess[0], baseAddress + 40, newHudAspectRatio);
                    Console.WriteLine("HUD Aspect Ratio Applied.");
                }
                catch (Exception ex)
                {
                    ErrorLogHelper.Log("ERROR: Failed to write to memory for new HUD aspect ratio.", ex);
                    return;
                }

                baseAddress += 983178308;
                byte[] newUserMasterOffset = MHWToolsHelper.GenerateUserMasterOffset(newHudAspect, graphicsOptionsIni.UltraWideModeLayout / 100);
                var userSliderValue = BitConverter.GetBytes(graphicsOptionsIni.UltraWideModeLayout / 100);
                try
                {
                    MHWToolsHelper.WriteSingleArray(gameProcess[0], baseAddress, new byte[] { 0x00, 0x00, 0x00, 0x00 });
                    MHWToolsHelper.WriteSingleArray(gameProcess[0], baseAddress + 4, new byte[] { 0x00, 0x00, 0x00, 0x00 });
                    MHWToolsHelper.WriteSingleArray(gameProcess[0], baseAddress + 8, new byte[] { 0x00, 0x00, 0x00, 0x00 });

                    MHWToolsHelper.WriteSingleArray(gameProcess[0], baseAddress, userSliderValue);
                    MHWToolsHelper.WriteSingleArray(gameProcess[0], baseAddress + 4, newUserMasterOffset);
                    MHWToolsHelper.WriteSingleArray(gameProcess[0], baseAddress + 8, newUserMasterOffset);

                    Console.WriteLine("Ultra Wide Mode Layout applied");
                }
                catch (Exception ex)
                {
                    ErrorLogHelper.Log("ERROR: Failed to write to memory for userSlider.", ex);
                }
            }
            catch (Exception ex)
            {
                ErrorLogHelper.Log("ERROR: Testing/finding base address.", ex);
            }
        }
    }
}