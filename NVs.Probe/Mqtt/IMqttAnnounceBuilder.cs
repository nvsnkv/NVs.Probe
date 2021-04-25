using System.Collections.Generic;
using MQTTnet;
using MQTTnet.Client.Options;
using NVs.Probe.Metrics;

namespace NVs.Probe.Mqtt
{
    internal interface IMqttAnnounceBuilder
    {
        IEnumerable<MqttApplicationMessage> BuildAnnounceMessages(IEnumerable<MetricConfig> configs, IMqttClientOptions options);
    }
}