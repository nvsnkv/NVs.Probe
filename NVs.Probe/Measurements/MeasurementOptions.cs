﻿using System;
using NVs.Probe.Measurements.CommandRunner;

namespace NVs.Probe.Measurements
{
    internal sealed class MeasurementOptions
    {
        public MeasurementOptions(RunnerOptions runnerOptions, TimeSpan measurementTimeout, TimeSpan intervalBetweenMeasurements)
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