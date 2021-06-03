using System;
using System.Drawing;
using Console = Colorful.Console;

namespace NVs.Probe.Client
{
    internal sealed class ConsoleWrapper : IConsole
    {
        private readonly bool verbose;

        public ConsoleWrapper(bool verbose)
        {
            this.verbose = verbose;
        }

        public void WriteLine(string text)
        {
            Console.WriteLine(text);
        }

        public void WriteError(string text)
        {
            Console.WriteLine(text, Color.Red);
        }

        public void WriteVerbose(string text)
        {
            if (verbose)
            {
                Console.WriteLine(text, Color.Gray);
            }
        }

        public void WriteWarning(string text)
        {
            Console.WriteLine(text, Color.Yellow);
        }
    }
}