using System;
using NVs.Probe.Metrics;

namespace NVs.Probe.Measuring
{
    internal abstract class Measurement
    {
        protected Measurement(Metric metric)
        {
            Metric = metric ?? throw new ArgumentNullException(nameof(metric));
        }

        public Metric Metric { get; }
    }
}