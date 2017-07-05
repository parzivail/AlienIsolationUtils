using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlienIsolationUtils
{
    static class Extensions
    {
        public static string ReadNullTermString(this BinaryReader reader)
        {
            var l = new List<byte>();
            while (true)
            {
                byte b = 0;
                try
                {
                    b = reader.ReadByte();
                }
                catch (Exception)
                {
                    break;
                }
                if (b == 0x00)
                    break;
                l.Add(b);
            }
            return Encoding.UTF8.GetString(l.ToArray());
        }
    }
}
