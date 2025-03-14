using MHW___Reshade_Injector_Helper.Constants;
using MHW___Reshade_Injector_Helper.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace MHW___Reshade_Injector_Helper.Helpers
{
    public static class MHWToolsHelper
    {
        public static bool TestLastGoodAddress(this Process targetProcess, ulong baseAddress, BytePattern SearchTarget)
        {
            var testBytes = new byte[SearchTarget.Bytes.Length];
            ReadProcessMemory(targetProcess.Handle, (IntPtr)baseAddress, testBytes, SearchTarget.Bytes.Length, out int bytesRead);

            return ByteArrayCompare(testBytes, SearchTarget.Bytes); //Use fast compare
        }

        public static unsafe bool ByteArrayCompare(this byte[] b1, byte[] b2)
        {
            if (ReferenceEquals(b1, b2))
            {
                return true;
            }

            if (b1 is null || b2 is null || b1.Length != b2.Length)
            {
                return false;
            }

            fixed (byte* ptr1 = b1, ptr2 = b2)
            {
                return memcmp(ptr1, ptr2, (UIntPtr)(uint)b1.Length) == 0;
            }
        }

        public static byte[] GetBytes(this float num)
        {
            return BitConverter.GetBytes(num);
        }

        public static byte[] GenerateSearchTarget(GraphicsOptionsINI graphicOptions)
        {
            const float widescreenAspectRatio = 2.37f;
            const float normalAspectRatio = 1.777778f;

            //Is it 16:9 or wider screen
            float gameOptionsAspectRatio = graphicOptions.AspectRatio == General_CVs.OFF ? normalAspectRatio : widescreenAspectRatio;

            //Get res XY and aspect Ratio
            int resX = graphicOptions.ResolutionX;
            int resY = graphicOptions.ResolutionY;
            bool isHorizontal = resX / resY > normalAspectRatio;
            int renderResXY = isHorizontal ? resY : resX;
            int otherResXY = isHorizontal ? resX : resY;

            //Find offsets
            var renderResRaw = isHorizontal ? renderResXY * gameOptionsAspectRatio : renderResXY / gameOptionsAspectRatio;
            var renderRes = Convert.ToInt32(Math.Round(renderResRaw));
            var blackBarOffset = (otherResXY - (float)renderRes) / 2f;
            var windowSize = Convert.ToInt32(Math.Floor(otherResXY - blackBarOffset));

            //Hex variables
            var hexBlackBarOffset = BitConverter.GetBytes(Convert.ToInt32(Math.Floor(blackBarOffset)));

            var hexRenderRes = BitConverter.GetBytes(renderRes);

            var hexWindowRenderRes = BitConverter.GetBytes(renderResXY);

            var hexWindowSize = BitConverter.GetBytes(windowSize);

            //Block copies

            var firstBlockCopy = hexBlackBarOffset;
            var secondBlockCopy = isHorizontal ? hexWindowSize : hexWindowRenderRes;
            var thirdBlockCopy = isHorizontal ? hexWindowRenderRes : hexWindowSize;
            var fourthBlockCopy = isHorizontal ? hexRenderRes : hexWindowRenderRes;

            var fifthBlockCopy = isHorizontal ? hexWindowRenderRes : hexRenderRes;

            //Hex Arrays
            byte[] hexBlackBarOffsets = new byte[8];
            byte[] hexWindowSizeBytes = new byte[8];
            byte[] hexRenderResolutionBytes = new byte[8];

            //Copy blocks
            Buffer.BlockCopy(firstBlockCopy, 0, hexBlackBarOffsets, isHorizontal ? 0 : 4, firstBlockCopy.Length);
            Buffer.BlockCopy(secondBlockCopy, 0, hexWindowSizeBytes, 0, secondBlockCopy.Length);
            Buffer.BlockCopy(thirdBlockCopy, 0, hexWindowSizeBytes, 4, thirdBlockCopy.Length);
            Buffer.BlockCopy(fourthBlockCopy, 0, hexRenderResolutionBytes, 0, fourthBlockCopy.Length);
            Buffer.BlockCopy(fifthBlockCopy, 0, hexRenderResolutionBytes, 4, fifthBlockCopy.Length);

            byte[] searchTarget = new byte[24];
            Buffer.BlockCopy(hexBlackBarOffsets, 0, searchTarget, 0, hexBlackBarOffsets.Length);
            Buffer.BlockCopy(hexWindowSizeBytes, 0, searchTarget, 8, hexWindowSizeBytes.Length);
            Buffer.BlockCopy(hexRenderResolutionBytes, 0, searchTarget, 16, hexRenderResolutionBytes.Length);

            return searchTarget;
        }

        public static ulong GetAddressByteArray(Process targetProcess, BytePattern searchTarget, ulong startRange, ulong endRange)
        {
            ulong returnValue = 0;
            ulong currentPointer = startRange;
            ulong lastPointer = currentPointer;
            var searchRegions = new List<(ulong, ulong)>();

            while (currentPointer < endRange)
            {
                //Lets try to parallel this....

                //Want to find memory regions that we can actually access
                //add them to a list of range start/end
                //do upto the core count of parallel searches
                //if we find a match, return the address and stop all other searches with a boolean cancellation variable

                if (VirtualQueryEx(targetProcess.Handle, (IntPtr)currentPointer, out WindowsMemoryHelper.MEMORY_BASIC_INFORMATION64 memoryRegion,
                    (uint)Marshal.SizeOf<WindowsMemoryHelper.MEMORY_BASIC_INFORMATION64>()) > 0
                    && memoryRegion.RegionSize > 0
                    && memoryRegion.State == (uint)WindowsMemoryHelper.RegionPageState.MEM_COMMIT
                    && WindowsMemoryHelper.CheckProtection(searchTarget, memoryRegion.Protect))
                {
                    ulong regionStart = Math.Max(startRange, memoryRegion.BaseAddress);
                    ulong regionEnd = Math.Min(endRange, memoryRegion.BaseAddress + memoryRegion.RegionSize);

                    searchRegions.Add((regionStart, regionEnd));
                }

                lastPointer = currentPointer;

                currentPointer = memoryRegion.RegionSize > 0
                    ? memoryRegion.BaseAddress + memoryRegion.RegionSize
                    : currentPointer + 1;

                if (currentPointer == lastPointer)
                {
                    ErrorLogHelper.Log($"Current pointer ({currentPointer}) = Last Pointer loop");
                    return returnValue;
                }

                if (searchRegions.Count >= 10 || currentPointer >= endRange)
                {
                    //We want to search all regions available, when > 10 regions are found, or current pointer is equal to or greater than the end of range

                    // Get the number of cores available
                    int numCores = Environment.ProcessorCount;

                    // Use Parallel.Linq with maximum concurrency based on core count
                    Parallel.ForEach(searchRegions, new ParallelOptions
                    {
                        MaxDegreeOfParallelism = numCores
                    },
                    searchRegion =>
                    {
                        ulong? result = ReadMemoryRegion(targetProcess, searchTarget, searchRegion.Item1, searchRegion.Item2);

                        if (result.HasValue)
                        {
                            returnValue = result.Value;
                        }
                    });

                    if (returnValue > 0)
                    {
                        return returnValue; //Return the address if we found it
                    }

                    searchRegions.Clear();
                }
            }

            return returnValue; //Return 0 if we didn't find it
        }

        public static byte[][] GenerateNewResolutionArrays(byte[] resX, byte[] resY)
        {
            byte[] primaryResolutionArray = new byte[24];
            byte[] secondaryResolutionArray = new byte[8];

            Buffer.BlockCopy(resX, 0, primaryResolutionArray, 8, resX.Length);
            Buffer.BlockCopy(resY, 0, primaryResolutionArray, 12, resY.Length);
            Buffer.BlockCopy(resX, 0, primaryResolutionArray, 16, resX.Length);
            Buffer.BlockCopy(resY, 0, primaryResolutionArray, 20, resY.Length);

            Buffer.BlockCopy(resX, 0, secondaryResolutionArray, 0, resX.Length);
            Buffer.BlockCopy(resY, 0, secondaryResolutionArray, 4, resY.Length);

            return new[] { primaryResolutionArray, secondaryResolutionArray };
        }

        public static byte[] GenerateUserMasterOffset(float aspectRatio, float userSlider)
        {
            aspectRatio *= 1600;
            aspectRatio -= 2560;
            aspectRatio /= 2;
            aspectRatio *= userSlider;

            return BitConverter.GetBytes(aspectRatio);
        }

        public static void WriteNewResolution(Process targetProcess, ulong baseAddress, byte[][] newResolutionArray)
        {
            //Write new resolution values
            if (!WriteProcessMemory(targetProcess.Handle, baseAddress, newResolutionArray[0], newResolutionArray[0].Length, out int bytesWritten) || bytesWritten != newResolutionArray[0].Length)
            {
                throw new Exception("Failed to write new resolution. Address: " + baseAddress);
            }

            //Write secondary resolution values
            if (!WriteProcessMemory(targetProcess.Handle, baseAddress + 0x23D90, newResolutionArray[1], newResolutionArray[1].Length, out bytesWritten) || bytesWritten != newResolutionArray[1].Length)
            {
                throw new Exception("Failed to write new resolution. Address: " + baseAddress);
            }

            //Break target coords to force rebuild to fix shaders
            if (!WriteProcessMemory(targetProcess.Handle, baseAddress + 0x78, BitConverter.GetBytes(1), 1, out bytesWritten) || bytesWritten != 1)
            {
                throw new Exception("Failed to reset render targets. Address: " + baseAddress);
            }
        }

        public static void WriteSingleArray(Process targetProcess, ulong baseAddress, byte[] newArray)
        {
            //Write new array to address
            if (!WriteProcessMemory(targetProcess.Handle, baseAddress, newArray, newArray.Length, out int bytesWritten))
            {
                throw new Exception("Failed to write new array. Address: " + baseAddress);
            }
        }

        private static ulong? ReadMemoryRegion(Process targetProcess, BytePattern searchTarget, ulong regionStart, ulong regionEnd)
        {
            byte[] regionBytes = new byte[regionEnd - regionStart];
            ReadProcessMemory(targetProcess.Handle, (IntPtr)regionStart, regionBytes, regionBytes.Length, out int bytesRead);

            int locateResult = ByteSearchHelper.Locate(regionBytes, searchTarget.Bytes);
            if (locateResult != -1)
            {
                return regionStart + (ulong)locateResult;
            }

            return null;
        }

        //Answer from stack overflow to improve performance
        //https://stackoverflow.com/a/1445405
        [DllImport("msvcrt.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern unsafe int memcmp(byte* b1, byte* b2, UIntPtr count);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern int VirtualQueryEx(IntPtr hProcess, IntPtr lpAddress, out WindowsMemoryHelper.MEMORY_BASIC_INFORMATION64 lpBuffer, uint dwLength);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, [Out] byte[] lpBuffer, int dwSize, out int lpNumberOfBytesRead);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool WriteProcessMemory(IntPtr hProcess, ulong lpBaseAddress, byte[] lpBuffer, int dwSize, out int lpNumberOfBytesWritten);
    }
}