using System;

namespace AlienIsolationUtils
{
    internal class ErrorHelper
    {
        public static void FileNotFound(string filename)
        {
            Console.WriteLine($"The specified file could not be found: \"{filename}\"");
            Environment.Exit(0);
        }

        public static void InvalidFiletype(string filename, string expectedType)
        {
            Console.WriteLine($"The specified file was not of the correct type, expected \"{expectedType}\": \"{filename}\"");
            Environment.Exit(0);
        }
    }
}