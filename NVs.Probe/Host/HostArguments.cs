using System;
using System.Collections.Generic;
using CommandLine;
using MQTTnet.Client.Options;
using NVs.Probe.Metrics;
using NVs.Probe.Mqtt;

namespace NVs.Probe.Host
{
    sealed class HostArguments
    {
        public HostArguments(string mqttClientId, string mqttBroker, string mqttUser, string mqttPassword, uint mqttBrokerPort, uint mqttBrokerReconnectAttempts, ulong mqttBrokerReconnectInterval, ulong measurementTimeout, ulong measurementSeriesInterval, IEnumerable<string> measurementArguments)
        {
            MqttClientId = mqttClientId;
            MqttBroker = mqttBroker;
            MqttUser = mqttUser;
            MqttPassword = mqttPassword;
            MqttBrokerPort = mqttBrokerPort;
            MqttBrokerReconnectAttempts = mqttBrokerReconnectAttempts;
            MqttBrokerReconnectInterval = mqttBrokerReconnectInterval;
            MeasurementTimeout = measurementTimeout;
            MeasurementSeriesInterval = measurementSeriesInterval;
            MeasurementArguments = measurementArguments;
        }

        [Option('c', "mqtt-client-id", HelpText = "MQTT Client Identifier", Required = true)]
        public string MqttClientId { get; }

        [Option('b', "mqtt-broker-host", HelpText = "The hostname or IP address of MQTT broker", Required = true)]
        public string MqttBroker { get; }

        [Option('u', "mqtt-user", HelpText = "Username used for authentication on MQTT broker", Required = true)]
        public  string MqttUser { get; }

        [Option('p', "mqtt-password", HelpText = "Password used for authentication on MQTT broker", Required = true)]
        public string MqttPassword { get; }

        [Option("mqtt-broker-port", HelpText = "Port number of MQTT broker", Default = (uint)1883)]
        public uint MqttBrokerPort { get; }

        [Option("mqtt-broker-reconnect-attempts", HelpText = "Count of attempts to reconnect to MQTT broker in case of broken connection", Default = (uint)0)]
        public uint MqttBrokerReconnectAttempts { get; }

        [Option("mqtt-broker-reconnect-interval", HelpText = "Base interval between attempts to reconnect to MQTT broker", Default = (ulong)5000)]
        public ulong MqttBrokerReconnectInterval { get; }

        [Option("measurement-timeout", HelpText = "Timeout of a single measurement", Default = (ulong)1000)]
        public ulong MeasurementTimeout { get; }

        [Option("measurement-series-interval", HelpText = "Base interval between measurement series", Default = (ulong)120000)]
        public ulong MeasurementSeriesInterval { get; }

        [Value(0, MetaName = "Measurement configuration -  a series of topic-command pairs, like 'dotnet/version' 'dotnet -- version'", Required = true)]
        public IEnumerable<string> MeasurementArguments { get; }

        public IMqttClientOptions GetMqttOptions(MqttClientOptionsBuilder builder)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            return builder
                .WithTcpServer(MqttBroker, (int)MqttBrokerPort)
                .WithClientId(MqttClientId)
                .WithCredentials(MqttUser, MqttPassword)
                .Build();
        }

        public RetryOptions GetMqttRetryOptions()
        {
            return new RetryOptions(
                MqttBrokerReconnectAttempts == 0
                    ? (TimeSpan?) null
                    : TimeSpan.FromMilliseconds(MqttBrokerReconnectInterval),
                MqttBrokerReconnectAttempts);
        }

        public IEnumerable<MetricConfig> GetMetricConfigs()
        {
            return new MetricConfigBuilder(MeasurementArguments).Build();
        }
    }
}