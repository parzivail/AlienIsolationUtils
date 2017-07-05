using System;

namespace AlienIsolationUtils
{
    internal class ErrorHelper
    {
        private static void PrintError(string error)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(error);
            Console.ResetColor();
        }

        public static void FileNotFound(string filename)
        {
            PrintError($"The specified file can not be found: \"{filename}\"");
            Environment.Exit(2);
        }

        public static void InvalidFiletype(string filename, string expectedType)
        {
            PrintError($"The specified is was not of the correct type, expected \"{expectedType}\": \"{filename}\"");
            Environment.Exit(87);
        }

        public static void CorruptFile(string filename, string corruptMessage)
        {
            PrintError($"The specified file is corrupt. {corruptMessage}: \"{filename}\"");
            Environment.Exit(13);
        }
    }
}