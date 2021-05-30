using System;
using NVs.Probe.Configuration;
using NVs.Probe.Execution;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace NVs.Probe.Measuring
{
    internal sealed class MeterConfigBuilder : YamlConfigBuilder<MeterOptions> 
    {

        public MeterConfigBuilder() : base(new MeasurementOptionsConverter())
        {
        }

        private class MeasurementOptionsConverter : IYamlTypeConverter
        {
            public bool Accepts(Type type)
            {
                return type == typeof(RunnerOptions);
            }

            public object ReadYaml(IParser parser, Type type)
            {
                string interpreter = null;
                string flags = null;
                TimeSpan timepout = TimeSpan.FromSeconds(30);
                TimeSpan interval = TimeSpan.FromSeconds(120);

                if (!parser.TryConsume<MappingStart>(out var __))
                {
                    throw new InvalidOperationException("Unexpected token received from the parser!");
                }

                while (!parser.TryConsume<MappingEnd>(out var _))
                {
                    var propertyName = parser.Consume<Scalar>();
                    switch (propertyName.Value)
                    {
                        case "interpreter":
                            interpreter = parser.Consume<Scalar>().Value;
                            break;

                        case "interpreter_flags":
                            flags = parser.Consume<Scalar>().Value;
                            break;

                        case "measurement_timeout":
                            timepout = TimeSpan.TryParse(parser.Consume<Scalar>().Value, out var t)
                                ? t
                                : throw new InvalidOperationException("Failed to build meter config - unable to parse measurement_timeout!");
                            break;

                        case "series_interval":
                            interval = TimeSpan.TryParse(parser.Consume<Scalar>().Value, out var i)
                                ? i
                                : throw new InvalidOperationException("Failed to build meter config - unable to parse series_interval!");
                            break;
                    }
                }

                if (string.IsNullOrEmpty(interpreter)) throw new InvalidOperationException("Failed to build meter config - interpreter is not provided!");

                return new MeterOptions(new RunnerOptions(interpreter, flags), timepout, interval);
            }

            public void WriteYaml(IEmitter emitter, object value, Type type)
            {
                throw new NotImplementedException();
            }
        }
    }
}