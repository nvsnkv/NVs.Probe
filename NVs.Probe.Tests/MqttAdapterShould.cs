using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Castle.DynamicProxy.Generators.Emitters.SimpleAST;
using Microsoft.Extensions.Logging;
using Moq;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;
using NVs.Probe.Measuring;
using NVs.Probe.Metrics;
using NVs.Probe.Mqtt;
using Xunit;

namespace NVs.Probe.Tests
{
    public sealed class MqttAdapterShould
    {
        private readonly Mock<IMqttClientOptions> options = new Mock<IMqttClientOptions>();
        private readonly Mock<IMqttClientFactory> factory = new Mock<IMqttClientFactory>();
        private readonly Mock<ILogger<MqttAdapter>> logger = new Mock<ILogger<MqttAdapter>>();
        private readonly Mock<IMqttAnnounceBuilder> builder = new Mock<IMqttAnnounceBuilder>();
        private readonly Mock<IMqttClient> client = new Mock<IMqttClient>();

        public MqttAdapterShould()
        {
            factory.Setup(f => f.CreateMqttClient()).Returns(client.Object);
        }

        [Fact, Trait("Category", "Win"), Trait("Category", "Linux")]
        public async Task NotifyAboutSuccessfulMeasurement()
        {
            var measurement = new SuccessfulMeasurement(new Metric("successful/topic"), "success");
            var ct = CancellationToken.None;

            options.SetupGet(o => o.ClientId).Returns("test_client");
            client.SetupGet(c => c.IsConnected).Returns(true);

            client.Setup(
                c => c.PublishAsync(It.Is<MqttApplicationMessage>(m =>
                    m.Topic == $"{measurement.Metric.Topic}" && m.ConvertPayloadToString() == measurement.Result), ct))
                .Verifiable("Expected message was not published!");

            var adapter = new MqttAdapter(options.Object, factory.Object, new RetryOptions(null), builder.Object, logger.Object);
            await adapter.Notify(measurement, ct);

            client.VerifyAll();
        }

        [Fact, Trait("Category", "Win"), Trait("Category", "Linux")]
        public async Task AnnounceTopics()
        {
            var configs = new MetricConfig[0];
            var messages = new[] { new MqttApplicationMessage { Topic = "announce_1" }, new MqttApplicationMessage { Topic = "announce_2" }, new MqttApplicationMessage { Topic = "announce_3" } };
            var ct = CancellationToken.None;

            options.SetupGet(o => o.ClientId).Returns("test_client");
            client.SetupGet(c => c.IsConnected).Returns(true);

            builder.Setup(b => b.BuildAnnounceMessages(configs, options.Object)).Returns(messages);

            foreach (var message in messages)
            {
                client.Setup(c => c.PublishAsync(message, ct)).Verifiable($"Message {message.Topic} was not published!");
            }

            var adapter = new MqttAdapter(options.Object, factory.Object, new RetryOptions(null), builder.Object, logger.Object);
            await adapter.Announce(configs, ct);

            client.VerifyAll();
        }
    }
}