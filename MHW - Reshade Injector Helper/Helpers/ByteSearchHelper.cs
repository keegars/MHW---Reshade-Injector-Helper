namespace MHW___Reshade_Injector_Helper.Helpers
{
    public static class ByteSearchHelper
    {
        public static int Locate(byte[] self, byte[] candidate)
        {
            if (self == null || candidate == null || self.Length < candidate.Length)
            {
                return -1;
            }

            int[] badCharTable = BuildBadCharTable(candidate);
            int scan = 0;
            int lastIndex = self.Length - candidate.Length;

            while (scan <= lastIndex)
            {
                int j = candidate.Length - 1;

                while (j >= 0 && self[scan + j] == candidate[j])
                {
                    j--;
                }

                if (j < 0)
                {
                    return scan; // Match found
                }

                // Shift using bad character rule
                scan += badCharTable[self[scan + candidate.Length - 1]];
            }

            return -1;
        }

        private static int[] BuildBadCharTable(byte[] pattern)
        {
            const int tableSize = 256; // Byte values range from 0 to 255
            int[] table = new int[tableSize];

            for (int i = 0; i < tableSize; i++)
            {
                table[i] = pattern.Length; // Default shift is pattern length
            }

            for (int i = 0; i < pattern.Length - 1; i++)
            {
                table[pattern[i]] = pattern.Length - 1 - i; // Set up table based on the pattern
            }

            return table;
        }
    }
}