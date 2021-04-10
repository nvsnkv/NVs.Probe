using System;

namespace NVs.Probe.Metrics
{
    internal sealed class Metric
    {
        public Metric(string topic)
        {
            if (string.IsNullOrEmpty(topic)) throw new ArgumentException("Value cannot be null or empty.", nameof(topic));
            Topic = topic;
        }

        public string Topic { get; }
    }
}