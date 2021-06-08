using System;

namespace NVs.Probe.Metrics
{
    internal class MetricConfig
    {
        public MetricConfig(Metric metric, string command)
        {
            Metric = metric ?? throw new ArgumentNullException(nameof(metric));
            Command = command ?? throw new ArgumentNullException(nameof(command));
        }

        public Metric Metric { get; }

        public string Command { get; }
    }

    internal sealed class HomeAssistantMetricConfig : MetricConfig
    {
        public string DeviceClass { get; }

        public HomeAssistantMetricConfig(Metric metric, string command, string deviceClass) : base(metric, command)
        {
            DeviceClass = deviceClass;
        }
    }
}