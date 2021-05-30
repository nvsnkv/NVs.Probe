using System;

namespace NVs.Probe.Execution
{
    internal sealed class RunnerOptions
    {
        public RunnerOptions(string shell, string flags)
        {
            Shell = shell ?? throw new ArgumentNullException(nameof(shell));
            Flags = flags ?? throw new ArgumentNullException(nameof(flags));
        }

        public string Shell { get; }

        public string Flags { get; }
    }
}