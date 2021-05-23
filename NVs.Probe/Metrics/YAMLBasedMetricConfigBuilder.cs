using System;
using System.Collections.Generic;
using System.IO;
using NVs.Probe.Config;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace NVs.Probe.Metrics
{
    internal sealed class YamlBasedMetricConfigBuilder:YamlConfigBuilder<IEnumerable<MetricConfig>>
    {
        public YamlBasedMetricConfigBuilder():base(new MetricTypeConverter())
        {
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
