using System;

namespace NVs.Probe.Metrics
{
    abstract class Measurement
    {
        protected Measurement(Metric metric)
        {
            Metric = metric ?? throw new ArgumentNullException(nameof(metric));
        }

        public Metric Metric { get; }
    }
}