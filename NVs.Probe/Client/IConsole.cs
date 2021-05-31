using System;

namespace NVs.Probe.Client
{
    internal interface IConsole
    {
        void WriteLine(string text);

        void WriteError(string text);
    }
}