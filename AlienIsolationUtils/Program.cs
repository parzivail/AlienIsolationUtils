using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
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
            //new Command("repack", "<pak filename> <zip filename>", "Creates a PAK from a ZIP archive", Repack, 2)
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

            //Pak.Repack(filenameInput, filenameOutput);
        }
    }
}
