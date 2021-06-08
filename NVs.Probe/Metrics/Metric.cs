using System;

namespace NVs.Probe.Metrics
{
    internal sealed class Metric
    {
        public Metric(string topic, string unitOfMeasurement = null)
        {
            if (string.IsNullOrEmpty(topic)) throw new ArgumentException("Value cannot be null or empty.", nameof(topic));
            Topic = topic;
            UnitOfMeasurement = unitOfMeasurement;
        }

        public string Topic { get; }

        public string UnitOfMeasurement { get; }
    }
}