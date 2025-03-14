namespace MHW___Reshade_Injector_Helper.Models
{
    public class AddressRange
    {
        public AddressRange(ulong start, ulong end)
        {
            Start = start;
            End = end;
        }

        public ulong Start { get; }

        public ulong End { get; }
    }
}