using System;
using NVs.Probe.Metrics;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace NVs.Probe.Configuration
{
    internal sealed class YamlMetricTypeConverter : IYamlTypeConverter
    {
        public bool Accepts(Type type)
        {
            return typeof(MetricConfig) == type;
        }

        public object ReadYaml(IParser parser, Type type)
        {
            string topic = null;
            string command = null;
            string unit = null;

            if (!parser.TryConsume<MappingStart>(out _))
            {
                throw new InvalidOperationException("Unexpected token received from the parser!");
            }

            while (!parser.TryConsume<MappingEnd>(out _))
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

                    case "unit_of_measurement":
                        unit = parser.Consume<Scalar>().Value;
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

            return new MetricConfig(new Metric(topic, unit), command);
        }

        public void WriteYaml(IEmitter emitter, object value, Type type)
        {
            throw new NotImplementedException();
        }
    }
}
