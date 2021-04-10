using System;
using NVs.Probe.Metrics;

namespace NVs.Probe.Measurements
{
    internal sealed class FailedMeasurement : Measurement
    {
        public FailedMeasurement(Metric metric, Exception exception) : base(metric)
        {
            Exception = exception ?? throw new ArgumentNullException(nameof(exception));
        }

        public Exception Exception { get; }
    }
}