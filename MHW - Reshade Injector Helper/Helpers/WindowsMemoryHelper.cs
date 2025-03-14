using MHW___Reshade_Injector_Helper.Models;
using System;

namespace MHW___Reshade_Injector_Helper.Helpers
{
    public class WindowsMemoryHelper
    {
        public static RegionPageProtection[] ProtectionExclusions { get; } = new[]
        {
            RegionPageProtection.PAGE_GUARD,
            RegionPageProtection.PAGE_NOACCESS
        };

        public static bool CheckProtection(BytePattern pattern, uint flags)
        {
            var protectionFlags = (RegionPageProtection)flags;

            foreach (var protectionExclusion in ProtectionExclusions)
            {
                if (protectionFlags.HasFlag(protectionExclusion))
                {
                    return false;
                }
            }

            foreach (var protectionOrInclusive in pattern.Config.PageProtections)
            {
                if (protectionFlags.HasFlag(protectionOrInclusive))
                {
                    return true;
                }
            }

            return false;
        }

        public struct MEMORY_BASIC_INFORMATION64
        {
            public ulong BaseAddress;
            public ulong AllocationBase;
            public uint AllocationProtect;
            public uint __alignment1;
            public ulong RegionSize;
            public uint State;
            public uint Protect;
            public uint Type;
            public uint __alignment2;
        }

        [Flags]
        public enum RegionPageProtection : uint
        {
            // Disables all access to the committed region of pages. An attempt to read from, write to, or execute the committed region results in an access violation. This flag is not supported by
            // the CreateFileMapping function.
            PAGE_NOACCESS = 0x01,

            // Enables read-only access to the committed region of pages. An attempt to write to the committed region results in an access violation. If Data Execution Prevention is enabled, an
            // attempt to execute code in the committed region results in an access violation.
            PAGE_READONLY = 0x02,

            // Enables read-only or read/write access to the committed region of pages. If Data Execution Prevention is enabled, attempting to execute code in the committed region results in an access violation.
            PAGE_READWRITE = 0x04,

            // Enables read-only or copy-on-write access to a mapped view of a file mapping object. An attempt to write to a committed copy-on-write page results in a private copy of the page being
            // made for the process.The private page is marked as PAGE_READWRITE, and the change is written to the new page.If Data Execution Prevention is enabled, attempting to execute code in the
            // committed region results in an access violation. This flag is not supported by the VirtualAlloc or VirtualAllocEx functions.
            PAGE_WRITECOPY = 0x08,

            // Enables execute access to the committed region of pages. An attempt to write to the committed region results in an access violation. This flag is not supported by the CreateFileMapping function.
            PAGE_EXECUTE = 0x10,

            // Enables execute or read-only access to the committed region of pages. An attempt to write to the committed region results in an access violation. Windows Server 2003 and Windows XP:
            // This attribute is not supported by the CreateFileMapping function until Windows XP with SP2 and Windows Server 2003 with SP1.
            PAGE_EXECUTE_READ = 0x20,

            // Enables execute, read-only, or read/write access to the committed region of pages. Windows Server 2003 and Windows XP: This attribute is not supported by the CreateFileMapping function
            // until Windows XP with SP2 and Windows Server 2003 with SP1.
            PAGE_EXECUTE_READWRITE = 0x40,

            // Enables execute, read-only, or copy-on-write access to a mapped view of a file mapping object. An attempt to write to a committed copy-on-write page results in a private copy of the
            // page being made for the process.The private page is marked as PAGE_EXECUTE_READWRITE, and the change is written to the new page. This flag is not supported by the VirtualAlloc or
            // VirtualAllocEx functions.Windows Vista, Windows Server 2003 and Windows XP: This attribute is not supported by the CreateFileMapping function until Windows Vista with SP1 and Windows
            // Server 2008.
            PAGE_EXECUTE_WRITECOPY = 0x80,

            // Pages in the region become guard pages. Any attempt to access a guard page causes the system to raise a STATUS_GUARD_PAGE_VIOLATION exception and turn off the guard page status. Guard
            // pages thus act as a one-time access alarm. For more information, see Creating Guard Pages. When an access attempt leads the system to turn off guard page status, the underlying page
            // protection takes over. If a guard page exception occurs during a system service, the service typically returns a failure status indicator. This value cannot be used with PAGE_NOACCESS.
            // This flag is not supported by the CreateFileMapping function.
            PAGE_GUARD = 0x100
        }

        [Flags]
        public enum RegionPageState : uint
        {
            // Indicates committed pages for which physical storage has been allocated, either in memory or in the paging file on disk.
            MEM_COMMIT = 0x1000,

            // Indicates free pages not accessible to the calling process and available to be allocated. For free pages, the information in the AllocationBase, AllocationProtect, Protect, and Type
            // members is undefined.
            MEM_FREE = 0x10000,

            // Indicates reserved pages where a range of the process's virtual address space is reserved without any physical storage being allocated. For reserved pages, the information in the
            // Protect member is undefined.
            MEM_RESERVE = 0x2000
        }
    }
}