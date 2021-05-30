using System.Collections.Generic;
using System.Linq;
using CommandLine;
using NVs.Probe.Execution;
using NVs.Probe.Metrics;
using NVs.Probe.Mqtt;

namespace NVs.Probe.Configuration
{
    internal sealed class HostArguments
    {
        public HostArguments(string interpreter, string interpreterFlags, string metricsSetupPath, string mqttSetupPath, ulong measurementTimeout, ulong measurementSeriesInterval)
        {
            Interpreter = interpreter;
            InterpreterFlags = interpreterFlags;
            MeasurementTimeout = measurementTimeout;
            MeasurementSeriesInterval = measurementSeriesInterval;
            MetricsSetupPath = metricsSetupPath;
            MqttSetupPath = mqttSetupPath;
        }

        [Option('i', "interpreter", HelpText = "Interpreter used to execute commands", Required = true)]
        public string Interpreter { get; }
        
        [Option('f', "interpreter-flags", HelpText = "Interpreter flags", Required = true)]
        public string InterpreterFlags { get; }

        [Option('s', "metrics-setup", HelpText =  "Path to metrics configuration", Required = true, Default = "probe.metrics.yaml")]
        public string MetricsSetupPath { get; }

        [Option('m', "mqtt-options", HelpText = "Path to MQTT configuration", Required = true, Default = "probe.mqtt.yaml")]
        public string MqttSetupPath { get; }

        [Option("measurement-timeout", HelpText = "Timeout of a single measurement", Default = (ulong)1000)]
        public ulong MeasurementTimeout { get; }

        [Option("measurement-series-interval", HelpText = "Base interval between measurement series", Default = (ulong)120000)]
        public ulong MeasurementSeriesInterval { get; }

        public MqttOptions GetMqttOptions()
        {
            return new YamlBasedMqttOptionsBuilder().Build(MqttSetupPath);
        }
        
        public RunnerOptions GetRunnerOptions()
        {
            return new RunnerOptions(Interpreter, InterpreterFlags);
        }

        public IReadOnlyList<MetricConfig> GetMetricConfigs()
        {
            return new YamlBasedMetricConfigBuilder().Build(MetricsSetupPath).ToList().AsReadOnly();
        }
    }
}