using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Castle.DynamicProxy.Generators.Emitters.SimpleAST;
using Microsoft.Extensions.Logging;
using Moq;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;
using NVs.Probe.Measurements;
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
        private readonly Mock<IMqttClient> client = new Mock<IMqttClient>();

        public MqttAdapterShould()
        {
            factory.Setup(f => f.CreateMqttClient()).Returns(client.Object);
        }

        [Fact]
        public async Task NotifyAboutSuccessfulMeasurement()
        {
            var measurement = new SuccessfulMeasurement(new Metric("successful/topic"), "success");
            var ct = CancellationToken.None;

            options.SetupGet(o => o.ClientId).Returns("test_client");

            client.Setup(
                c => c.PublishAsync(It.Is<MqttApplicationMessage>(m =>
                    m.Topic == $"{options.Object.ClientId}/{measurement.Metric.Topic}" && m.ConvertPayloadToString() == measurement.Result), ct))
                .Verifiable("Expected message was not published!");

            var adapter = new MqttAdapter(options.Object, factory.Object, new RetryOptions(null), logger.Object);
            await adapter.Start(ct);
            await adapter.Notify(measurement, ct);

            client.VerifyAll();
        }

        [Fact]
        public async Task AnnounceTopics()
        {
            throw new NotImplementedException();
            var configs = new[]
            {
                new MetricConfig(new Metric("dotnet/version"), "dotnet --version"),
                new MetricConfig(new Metric("hardware/cpu_load"), "top ..."),
                new MetricConfig(new Metric("hardware/temp"), "/opt/vc...")
            };
            var ct = CancellationToken.None;

            options.SetupGet(o => o.ClientId).Returns("test_client");

            client.Setup(
                    c => c.PublishAsync(It.Is<MqttApplicationMessage>(m =>
                        m.Topic == $"homeassistant/sensor/nvs_probe/{options.Object.ClientId}/config" && m.ConvertPayloadToString() == null), ct))
                .Verifiable("Expected message was not published!");

            var adapter = new MqttAdapter(options.Object, factory.Object, new RetryOptions(null), logger.Object);
            await adapter.Start(ct);
            await adapter.Notify(null, ct);

            client.VerifyAll();
        }
    }
}