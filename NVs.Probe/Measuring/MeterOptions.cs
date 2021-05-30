using System;
using NVs.Probe.Execution;

namespace NVs.Probe.Measuring
{
    internal sealed class MeterOptions
    {
        public MeterOptions(RunnerOptions runnerOptions, TimeSpan measurementTimeout, TimeSpan intervalBetweenMeasurements)
        {
            RunnerOptions = runnerOptions;
            MeasurementTimeout = measurementTimeout;
            IntervalBetweenMeasurements = intervalBetweenMeasurements;
        }

        public RunnerOptions RunnerOptions { get; }

        public TimeSpan MeasurementTimeout { get; }

        public TimeSpan IntervalBetweenMeasurements { get; }
    }
}