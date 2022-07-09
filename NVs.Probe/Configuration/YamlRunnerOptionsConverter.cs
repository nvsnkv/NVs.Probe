using System;
using NVs.Probe.Execution;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace NVs.Probe.Configuration
{
    internal sealed class YamlRunnerOptionsConverter: IYamlTypeConverter
    {
        public const ulong DefaultTimeoutMilliseconds = 2000;

        public bool Accepts(Type type)
        {
            return type == typeof(RunnerOptions);
        }

        public object ReadYaml(IParser parser, Type type)
        {
            string shell = null, flags = string.Empty;
            TimeSpan timeout = TimeSpan.FromMilliseconds(DefaultTimeoutMilliseconds);

            if (!parser.TryConsume<MappingStart>(out _))
            {
                throw new InvalidOperationException("Unexpected token received from the parser!");
            }

            while (!parser.TryConsume<MappingEnd>(out _))
            {
                var propertyName = parser.Consume<Scalar>();
                switch (propertyName.Value)
                {
                    case "shell":
                        shell = parser.Consume<Scalar>().Value;
                        break;

                    case "flags":
                        flags = parser.Consume<Scalar>().Value;
                        break;

                    case "command_timeout":
                        timeout = TimeSpan.TryParse(parser.Consume<Scalar>().Value, out var t)
                            ? t
                            : throw new InvalidOperationException("Unable to build command runner configuration - invalid command_timeout given!");
                        break;
                }
            }

            if (string.IsNullOrEmpty(shell)) throw new InvalidOperationException("Unable to build command runner configuration - no shell given!");
            return new RunnerOptions(shell, flags, timeout);
        }

        public void WriteYaml(IEmitter emitter, object value, Type type)
        {
            throw new NotImplementedException();
        }
    }
}