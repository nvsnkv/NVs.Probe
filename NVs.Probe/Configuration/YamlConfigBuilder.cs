using System;
using System.Collections.Generic;
using System.IO;
using NVs.Probe.Execution;
using NVs.Probe.Metrics;
using NVs.Probe.Mqtt;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace NVs.Probe.Configuration
{
    internal class YamlConfigBuilder
    {
        private readonly IDeserializer deserializer;

        public YamlConfigBuilder()
        {
            deserializer = new DeserializerBuilder()
                .WithTypeConverter(new YamlMetricTypeConverter())
                .WithTypeConverter(new YamlMqttOptionsConverter())
                .WithTypeConverter(new YamlRunnerOptionsConverter())
                .WithNamingConvention(UnderscoredNamingConvention.Instance)
                .Build();
        }

        public ProbeConfiguration Build(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("Configuration file does not exists!", filePath);
            }

            using var rdr = new StreamReader(File.OpenRead(filePath));
            return Build(rdr);
        }

        public ProbeConfiguration Build(TextReader reader)
        {
            var tempConfig = deserializer.Deserialize<ReadWriteConfiguration>(reader);

            return new ProbeConfiguration(new ProbeOptions(tempConfig.Metrics.AsReadOnly(), tempConfig.InterSeriesDelay), tempConfig.Runner, tempConfig.Mqtt);
        }

        // ReSharper disable once ClassNeverInstantiated.Local
        private sealed class ReadWriteConfiguration
        {
            public List<MetricConfig> Metrics { get; set; }

            public TimeSpan InterSeriesDelay { get; set; }

            public MqttOptions Mqtt { get; set; }

            public RunnerOptions Runner { get; set; }
        }
    }
}