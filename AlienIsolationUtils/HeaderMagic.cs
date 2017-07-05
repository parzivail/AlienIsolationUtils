using System.Linq;

namespace AlienIsolationUtils
{
    internal class HeaderMagic
    {
        public static readonly byte[] Pak = { 0x50, 0x41, 0x4B, 0x32 };

        public static bool IsMagic(byte[] found, byte[] expected)
        {
            //     header magic lengths same,     and none of the bytes differ
            return found.Length == expected.Length && !found.Where((t, i) => t != expected[i]).Any();
        }
    }
}