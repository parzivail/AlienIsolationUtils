using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlienIsolationUtils
{
    class Program
    {
        private static readonly List<Command> Commands = new List<Command>
        {
            new Command("help", "", "Shows the help text", PrintHelp, 0),
            new Command("unpack", "<filename>", "Unpacks the .PAK file into it's respective directory structure and files", Unpack, 1)
        };

        static void Main(string[] args)
        {
            foreach (var command in Commands)
                if (command.AcceptParameters(args))
                    break;
        }

        private static void PrintHelp(string[] args)
        {
            Console.WriteLine($"Parzivail's Alien: Isolation Utils v1.0");
            foreach (var command in Commands)
                Console.WriteLine(command.CreateHelpText());
        }

        private static void Unpack(string[] args)
        {
            var filename = args[0];
            if (!File.Exists(filename))
                ErrorHelper.FileNotFound(filename);

            Pak.Unpack(filename);
        }
    }

    internal class Pak
    {
        public static void Unpack(string filename)
        {
            var pakName = Path.GetFileNameWithoutExtension(filename);

            using (var sr = new StreamReader(filename))
            using (var br = new BinaryReader(sr.BaseStream))
            {
                var magic = br.ReadBytes(4);

                if (!HeaderMagic.IsMagic(magic, HeaderMagic.Pak))
                    ErrorHelper.InvalidFiletype(filename, "pak");

                var offsetFromHeader = br.ReadUInt32();
                var numberOfFiles = br.ReadUInt32();
                var unknown = br.ReadUInt32();

                var fileNames = new List<string>();
                for (var i = 0; i < numberOfFiles; i++)
                    fileNames.Add(br.ReadNullTermString());

                var filePointers = new List<uint>();
                for (var i = 0; i < numberOfFiles; i++)
                    filePointers.Add(br.ReadUInt32());

                for (int fileIdx = 0; fileIdx < numberOfFiles; fileIdx++)
                {
                    var start = filePointers[fileIdx];
                    var end = fileIdx == (numberOfFiles - 1) ? sr.BaseStream.Length : filePointers[fileIdx + 1];

                    br.BaseStream.Seek(start, SeekOrigin.Begin);
                    var length = (int) (end - start);
                    var bytes = br.ReadBytes(length);

                    var dir = $"{pakName}\\{Path.GetDirectoryName(fileNames[fileIdx])}";
                    Directory.CreateDirectory(dir);
                    File.WriteAllBytes($"{dir}\\{Path.GetFileName(fileNames[fileIdx])}", bytes);
                }
                
                Console.WriteLine($"Unpacked {numberOfFiles} files");
            }
        }
    }

    internal class HeaderMagic
    {
        public static readonly byte[] Pak = {0x50, 0x41, 0x4B, 0x32};

        public static bool IsMagic(byte[] found, byte[] expected)
        {
            //     header magic lengths same,     and none of the bytes differ
            return found.Length == expected.Length && !found.Where((t, i) => t != expected[i]).Any();
        }
    }
}
