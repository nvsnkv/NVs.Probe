using System;
using System.Collections.Generic;
using NVs.Probe.Execution;
using NVs.Probe.Metrics;
using NVs.Probe.Mqtt;
using NVs.Probe.Server;

namespace NVs.Probe.Configuration
{
    internal sealed class ProbeConfiguration
    {
        public ProbeConfiguration(ProbeOptions probeOptions, RunnerOptions runnerOptions, MqttOptions mqttOptions)
        {
            ProbeOptions = probeOptions ?? throw new ArgumentNullException(nameof(probeOptions));
            RunnerOptions = runnerOptions ?? throw new ArgumentNullException(nameof(runnerOptions));
            MqttOptions = mqttOptions ?? throw new ArgumentNullException(nameof(mqttOptions));
        }

        public ProbeOptions ProbeOptions { get; }

        public RunnerOptions RunnerOptions { get; }

        public MqttOptions MqttOptions { get; }
    }
}