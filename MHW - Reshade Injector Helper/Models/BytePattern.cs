using MHW___Reshade_Injector_Helper.Helpers;
using System.Collections.Generic;

namespace MHW___Reshade_Injector_Helper.Models
{
    public class BytePatternConfig
    {
        public BytePatternConfig(byte[] patternString, ulong addressRangeStart, ulong addressRangeEnd, params WindowsMemoryHelper.RegionPageProtection[] pageProtections)
        {
            Bytes = patternString;
            AddressRangeStart = addressRangeStart;
            AddressRangeEnd = addressRangeEnd;
            PageProtections = pageProtections;
        }

        public byte[] Bytes { get; }

        public ulong AddressRangeStart { get; } = 0x140000000;

        public ulong AddressRangeEnd { get; } = 0x145000000;

        public WindowsMemoryHelper.RegionPageProtection[] PageProtections { get; }
    }

    public class BytePattern
    {
        public BytePattern(BytePatternConfig config)
        {
            Config = config;
        }

        public BytePatternConfig Config { get; }

        public byte[] Bytes
        {
            get
            {
                return Config.Bytes;
            }
        }

        public List<ulong> MatchedAddresses { get; } = new List<ulong>();

        public AddressRange AddressRange
        {
            get
            {
                if (_addressRange == null)
                {
                    _addressRange = new AddressRange(Config.AddressRangeStart, Config.AddressRangeEnd);
                }

                return _addressRange;
            }
        }

        private AddressRange _addressRange { get; set; }
    }
}