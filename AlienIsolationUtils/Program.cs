using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlienIsolationUtils
{
    class Program
    {
        private static readonly List<Command> Commands = new List<Command>
        {
            new Command("help", null, "Shows the help text", PrintHelp, 0),
            new Command("unpack", "<filename>", "Unpacks the .PAK file into it's respective directory structure and files", Unpack, 1),
            new Command("repack", "<pak filename> <zip filename>", "Creates a PAK from a ZIP archive", Repack, 2)
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

        private static void Repack(string[] args)
        {
            var filenameOutput = args[0];
            var filenameInput = args[1];
            if (!File.Exists(filenameInput))
                ErrorHelper.FileNotFound(filenameInput);

            Pak.Repack(filenameInput, filenameOutput);
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
                var four = br.ReadUInt32(); // ???

                var headerLength = br.BaseStream.Position;

                var fileNames = new List<string>();
                for (var i = 0; i < numberOfFiles; i++)
                    fileNames.Add(br.ReadNullTermString());

                if (sr.BaseStream.Position - headerLength != offsetFromHeader)
                    ErrorHelper.CorruptFile(filename, "Embedded header offset did not match actual offset");

                var filePointers = new List<uint>();
                for (var i = 0; i < numberOfFiles; i++)
                    filePointers.Add(br.ReadUInt32());

                for (int fileIdx = 0; fileIdx < numberOfFiles; fileIdx++)
                {
                    var start = filePointers[fileIdx];
                    var end = fileIdx == (numberOfFiles - 1) ? sr.BaseStream.Length : filePointers[fileIdx + 1];

                    br.BaseStream.Seek(start, SeekOrigin.Begin);
                    var length = (int)(end - start);
                    var bytes = br.ReadBytes(length);

                    var dir = $"{pakName}\\{Path.GetDirectoryName(fileNames[fileIdx])}";
                    Directory.CreateDirectory(dir);
                    File.WriteAllBytes($"{dir}\\{Path.GetFileName(fileNames[fileIdx])}", bytes);
                }

                Console.WriteLine($"Unpacked {numberOfFiles} files");
            }
        }

        internal static void Repack(string filenameInput, string filenameOutput)
        {
            var sw = new StreamWriter(filenameOutput);
            using (var bw = new BinaryWriter(sw.BaseStream))
            {
                bw.Write(HeaderMagic.Pak);

                using (var archive = ZipFile.OpenRead(filenameInput))
                {
                    bw.Write((uint)0); // offset from header placeholder
                    bw.Write((uint)0); // num files placeholder
                    bw.Write((uint)4);

                    var headerLength = bw.BaseStream.Position;

                    var numFiles = 0;
                    foreach (var entry in archive.Entries)
                    {
                        if (entry.FullName.EndsWith("/"))
                            continue;
                        numFiles++;
                        bw.WriteNullTermString(entry.FullName);
                    }

                    var position = bw.BaseStream.Position;
                    bw.BaseStream.Seek(4, SeekOrigin.Begin);
                    bw.Write((uint)(position - headerLength));
                    bw.Write((uint)numFiles);
                    bw.BaseStream.Seek(position, SeekOrigin.Begin);

                    var cursor = headerLength + sizeof(uint) * numFiles;
                    foreach (var entry in archive.Entries)
                    {
                        if (entry.FullName.EndsWith("/"))
                            continue;
                        bw.Write((uint)cursor);
                        cursor += entry.Length;
                    }

                    foreach (var entry in archive.Entries)
                    {
                        if (entry.FullName.EndsWith("/"))
                            continue;
                        var stream = entry.Open();
                        var length = entry.Length;
                        var bytes = new byte[length];
                        stream.Read(bytes, 0, (int) length);
                        bw.Write(bytes);
                    }
                }
            }
        }
    }

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
