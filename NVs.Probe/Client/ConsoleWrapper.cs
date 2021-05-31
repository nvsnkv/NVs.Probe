using System;
using System.Drawing;
using Console = Colorful.Console;

namespace NVs.Probe.Client
{
    internal sealed class ConsoleWrapper : IConsole
    {
        public void WriteLine(string text)
        {
            Console.WriteLine(text);
        }

        public void WriteError(string text)
        {
            Console.WriteLine(text, Color.Red);
        }
    }
}