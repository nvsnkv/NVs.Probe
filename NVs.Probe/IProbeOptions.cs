using System;
using System.Collections.Generic;
using NVs.Probe.Metrics;

namespace NVs.Probe
{
    internal sealed class ProbeOptions
    {
        public ProbeOptions(IReadOnlyList<MetricConfig> metrics, TimeSpan interSeriesDelay)
        {
            Metrics = metrics ?? throw new ArgumentNullException(nameof(metrics));
            InterSeriesDelay = interSeriesDelay;
        }

        public IReadOnlyList<MetricConfig> Metrics { get; }

        public TimeSpan InterSeriesDelay { get; }
    }
}