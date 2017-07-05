using System;
using System.Collections.Generic;
using System.IO;

namespace AlienIsolationUtils
{
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

                for (var fileIdx = 0; fileIdx < numberOfFiles; fileIdx++)
                {
                    var start = filePointers[fileIdx];
                    var end = fileIdx == numberOfFiles - 1 ? sr.BaseStream.Length : filePointers[fileIdx + 1];

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
    }
}