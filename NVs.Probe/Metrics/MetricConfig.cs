using System;

namespace NVs.Probe.Metrics
{
    internal sealed class MetricConfig
    {
        public MetricConfig(Metric metric, string command)
        {
            Metric = metric ?? throw new ArgumentNullException(nameof(metric));
            Command = command ?? throw new ArgumentNullException(nameof(command));
        }

        public Metric Metric { get; }

        public string Command { get; }
    }
}