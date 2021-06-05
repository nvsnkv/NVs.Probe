using System;
using NVs.Probe.Metrics;

namespace NVs.Probe.Measuring
{
    internal sealed class SuccessfulMeasurement : Measurement
    {
        public SuccessfulMeasurement(Metric metric, string result) : base(metric)
        {
            Result = result ?? throw new ArgumentNullException(nameof(result));
        }

        public string Result { get; }
    }
}