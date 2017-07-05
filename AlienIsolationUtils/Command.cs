using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlienIsolationUtils
{
    class Command
    {
        public string Name { get; }
        public string Syntax { get; }
        public string Desc { get; }
        public int NumParams { get; }

        private readonly Action<string[]> _work;

        public Command(string name, string syntax, string desc, Action<string[]> work, int numParams)
        {
            Name = name;
            Syntax = syntax;
            Desc = desc;
            _work = work;
            NumParams = numParams;
        }

        public bool AcceptParameters(string[] fullCommand)
        {
            if (fullCommand[0] != Name)
                return false;

            if (fullCommand.Length - 1 == NumParams)
            {
                var param = fullCommand.Skip(1).ToArray();
                _work(param);
            }
            else
                Console.WriteLine($"Invalid syntax: use\n\t{Name} {Syntax}");

            return true;
        }

        public string CreateHelpText()
        {
            return $"{Name} {Syntax}\n\t{Name} - {Desc}";
        }
    }
}
