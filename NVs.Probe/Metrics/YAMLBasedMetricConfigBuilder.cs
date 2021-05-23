using System;
using System.Collections.Generic;
using System.IO;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace NVs.Probe.Metrics
{
    internal sealed class YAMLBasedMetricConfigBuilder
    {
        private readonly IDeserializer deserializer;

        public YAMLBasedMetricConfigBuilder()
        {
            deserializer =  new DeserializerBuilder().WithTypeConverter(new MetricTypeConverter()).Build();
        }

        public IEnumerable<MetricConfig> Build(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("Configuration file does not exists!", filePath);
            }

            using var rdr = new StreamReader(File.OpenRead(filePath));
            return Build(rdr);
        }

        public IEnumerable<MetricConfig> Build(TextReader rdr)
        {
            return deserializer.Deserialize<IEnumerable<MetricConfig>>(rdr);
        }

        private sealed class MetricTypeConverter : IYamlTypeConverter
        {
            public bool Accepts(Type type)
            {
                return typeof(MetricConfig) == type;
            }

            public object ReadYaml(IParser parser, Type type)
            {
                string topic = null;
                string command = null;

                if (!parser.TryConsume<MappingStart>(out var __))
                {
                    throw new InvalidOperationException("Unexpected token received from the parser!");
                }

                while (!parser.TryConsume<MappingEnd>(out var _))
                {
                    var propertyName = parser.Consume<Scalar>();
                    switch (propertyName.Value)
                    {
                        case nameof(topic):
                            topic = parser.Consume<Scalar>().Value;
                            break;
                        
                        case nameof(command):
                            command = parser.Consume<Scalar>().Value;
                            break;
                    }
                }

                if (topic == null && command == null)
                {
                    throw new InvalidOperationException(
                        "Unable to build metric configuration - neither topic nor command were defined!");
                }

                if (command == null)
                {
                    throw new InvalidOperationException(
                        $"Unable to build metric configuration - command was not defined for topic '{topic}'");
                }
                if (topic == null)
                {
                    throw new InvalidOperationException(
                        $"Unable to build metric configuration - topic was not defined for command '{command}'");
                }

                return new MetricConfig(new Metric(topic), command);
            }

            public void WriteYaml(IEmitter emitter, object value, Type type)
            {
                throw new NotImplementedException();
            }
        }
    }
}
