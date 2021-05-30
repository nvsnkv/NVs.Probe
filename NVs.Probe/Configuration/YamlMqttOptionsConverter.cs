using System;
using MQTTnet.Client.Options;
using NVs.Probe.Mqtt;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace NVs.Probe.Configuration
{
    internal sealed class YamlMqttOptionsConverter : IYamlTypeConverter
    {
        public const int DefaultRetriesCount = 0;

        public bool Accepts(Type type)
        {
            return type == typeof(MqttOptions);
        }

        public object ReadYaml(IParser parser, Type type)
        {
            string clientId = null, username = null, pwd = null, broker = null;
            int? port = null, retriesCount = null;
            TimeSpan? retriesInterval = null;

            if (!parser.TryConsume<MappingStart>(out _))
            {
                throw new InvalidOperationException("Unexpected token received from the parser!");
            }

            while (!parser.TryConsume<MappingEnd>(out _))
            {
                var propertyName = parser.Consume<Scalar>();
                switch (propertyName.Value)
                {
                    case "client_id":
                        clientId = parser.Consume<Scalar>().Value;
                        break;

                    case "user":
                        username = parser.Consume<Scalar>().Value;
                        break;

                    case "password":
                        pwd = parser.Consume<Scalar>().Value;
                        break;

                    case "broker":
                        broker = parser.Consume<Scalar>().Value;
                        break;

                    case "port":
                        port = int.TryParse(parser.Consume<Scalar>().Value, out var p)
                            ? p
                            : throw new InvalidOperationException("Unable to build MQTT configuration - invalid port given!");
                        break;

                    case "retries_interval":
                        retriesInterval = TimeSpan.TryParse(parser.Consume<Scalar>().Value, out var t)
                            ? t
                            : throw new InvalidOperationException("Unable to build MQTT configuration - invalid retries_interval given!");
                        break;

                    case "retries_count":
                        retriesCount = int.TryParse(parser.Consume<Scalar>().Value, out var c)
                            ? c
                            : throw new InvalidOperationException("Unable to build MQTT configuration - invalid retries_count given!");
                        break;
                }
            }

            if (string.IsNullOrEmpty(clientId)) throw new InvalidOperationException("Unable to build MQTT configuration - client_id is not provided!");
            if (string.IsNullOrEmpty(username)) throw new InvalidOperationException("Unable to build MQTT configuration - user is not provided!");
            if (string.IsNullOrEmpty(pwd)) throw new InvalidOperationException("Unable to build MQTT configuration - password is not provided!");
            if (string.IsNullOrEmpty(broker)) throw new InvalidOperationException("Unable to build MQTT configuration - broker is not provided!");

            var client = new MqttClientOptionsBuilder()
                .WithClientId(clientId)
                .WithTcpServer(broker, port)
                .WithCredentials(username, pwd)
                .Build();

            if (retriesCount.HasValue && !retriesInterval.HasValue) throw new InvalidOperationException("Unable to build MQTT configuration! - retries_interval provided without retries_count!");
            if (!retriesCount.HasValue && retriesInterval.HasValue) throw new InvalidOperationException("Unable to build MQTT configuration! - retries_count provided without retries_interval!");
            if (retriesCount <= 0) throw new InvalidOperationException("Unable to build MQTT configuration! - retries_count invalid retries_count given!");

            return new MqttOptions(client, new RetryOptions(retriesInterval, (uint)(retriesCount ?? DefaultRetriesCount)));
        }

        public void WriteYaml(IEmitter emitter, object value, Type type)
        {
            throw new NotImplementedException();
        }
    }
}