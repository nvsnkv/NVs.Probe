using System;
using System.Text;
using FluentAssertions;
using MQTTnet.Client.Options;
using NVs.Probe.Configuration;
using Xunit;

namespace NVs.Probe.Tests
{
    public class YamlConfigBuilderShould
    {
        [Fact, Trait("Category", "Win"), Trait("Category", "Linux")]
        public void ReadMetricsSuccessfully()
        {
            var topics = new[] { "/probe/echo/1", "/probe/echo/2" };
            var commands = new[] { "echo 1", "echo 2" };
            var delay = TimeSpan.Parse("00:02:00");

            var options = new YamlConfigBuilder().Build("probe.settings.yaml").ProbeOptions;
            var metrics = options.Metrics;

            metrics.Should().HaveCount(topics.Length);
            for (var i = 0; i < topics.Length; i++)
            {
                var config = metrics[i];
                config.Metric.Topic.Should().BeEquivalentTo(topics[i]);
                config.Command.Should().BeEquivalentTo(commands[i]);
            }

            options.InterSeriesDelay.Should().Be(delay);
        }

        [Fact, Trait("Category", "Win"), Trait("Category", "Linux")]
        public void ReadMqttOptions()
        {
            var clientId = "probe";
            var user = "mqtt_user";
            var pwd = "mqtt_password";
            var broker = "mqtt.local";

            uint retriesCount = 5;
            var retryInterval = TimeSpan.Parse("00:00:30");

            var options = new YamlConfigBuilder().Build("probe.settings.yaml").MqttOptions;

            options.ClientOptions.ClientId.Should().BeEquivalentTo(clientId);
            options.ClientOptions.Credentials.Username.Should().BeEquivalentTo(user);
            options.ClientOptions.Credentials.Password.Should().BeEquivalentTo(Encoding.UTF8.GetBytes(pwd));
            options.ClientOptions.ChannelOptions.Should().BeOfType<MqttClientTcpOptions>();
            ((MqttClientTcpOptions)options.ClientOptions.ChannelOptions).Server.Should().BeEquivalentTo(broker);
            ((MqttClientTcpOptions)options.ClientOptions.ChannelOptions).Port.Should().BeNull();

            options.RetryOptions.Should().NotBeNull();
            options.RetryOptions.ShouldRetry.Should().BeTrue();
            options.RetryOptions.RetriesCount.Should().Be(retriesCount);
            options.RetryOptions.Interval.Should().Be(retryInterval);

        }

        [Fact, Trait("Category", "Win"), Trait("Category", "Linux")]
        public void ReadRunnerOptions()
        {
            var shell = "sh";
            var flags = "-c";
            var timeout = TimeSpan.Parse("00:00:02");

            var options = new YamlConfigBuilder().Build("probe.settings.yaml").RunnerOptions;
            options.Shell.Should().BeEquivalentTo(shell);
            options.Flags.Should().BeEquivalentTo(flags);
            options.CommandTimeout.Should().Be(timeout);
        }
    }
}


