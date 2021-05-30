using System;

namespace NVs.Probe.Execution
{
    internal sealed class RunnerOptions
    {
        public RunnerOptions(string shell, string flags, TimeSpan commandTimeout)
        {
            Shell = shell ?? throw new ArgumentNullException(nameof(shell));
            Flags = flags ?? throw new ArgumentNullException(nameof(flags));
            CommandTimeout = commandTimeout;
        }

        public string Shell { get; }

        public string Flags { get; }

        public TimeSpan CommandTimeout { get; }
    }
}