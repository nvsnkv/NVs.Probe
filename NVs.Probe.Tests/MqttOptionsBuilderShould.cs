using System;
using System.IO;
using System.Text;
using System.Threading;
using FluentAssertions;
using FluentAssertions.Common;
using MQTTnet.Client.Options;
using NVs.Probe.Mqtt;
using Xunit;

namespace NVs.Probe.Tests
{
    public class MqttOptionsBuilderShould
    {
        private readonly string clientId = "TEST_CLIENT_ID";
        private readonly string user = "TEST_USER";
        private readonly string pwd = "TEST_PWD";
        private readonly string broker = "broker.local";
        private readonly string requiredInput;

        public MqttOptionsBuilderShould()
        {
            requiredInput = $"client_id: {clientId}{Environment.NewLine}";
            requiredInput += $"user: {user}{Environment.NewLine}";
            requiredInput += $"password: {pwd}{Environment.NewLine}";
            requiredInput += $"broker: {broker}{Environment.NewLine}";
        }

        [Fact, Trait("Category", "Win"), Trait("Category", "Linux")]
        public void ReadBlockStyledOptions()
        {
            var options = new YAMLBasedMqttOptionsBuilder().Build(new StringReader(requiredInput));
            options.ClientOptions.ClientId.Should().BeEquivalentTo(clientId);
            options.ClientOptions.Credentials.Username.Should().BeEquivalentTo(user);
            options.ClientOptions.Credentials.Password.Should().BeEquivalentTo(Encoding.UTF8.GetBytes(pwd));
            options.ClientOptions.ChannelOptions.Should().BeOfType<MqttClientTcpOptions>();
            ((MqttClientTcpOptions) options.ClientOptions.ChannelOptions).Server.Should().BeEquivalentTo(broker);
            ((MqttClientTcpOptions) options.ClientOptions.ChannelOptions).Port.Should().BeNull();

            options.RetryOptions.Should().NotBeNull();
            options.RetryOptions.ShouldRetry.Should().BeFalse();
        }

        [Fact, Trait("Category", "Win"), Trait("Category", "Linux")]
        public void ReadOptionalPortIfProvided()
        {
            var port = 8800;
            var input = requiredInput + $"port: {port}";

            var options = new YAMLBasedMqttOptionsBuilder().Build(new StringReader(input));
            options.ClientOptions.ChannelOptions.Should().BeOfType<MqttClientTcpOptions>();
            ((MqttClientTcpOptions)options.ClientOptions.ChannelOptions).Port.Should().HaveValue();
            // ReSharper disable once PossibleInvalidOperationException
            ((MqttClientTcpOptions)options.ClientOptions.ChannelOptions).Port.Value.Should().Be(port);
        }

        [Fact, Trait("Category", "Win"), Trait("Category", "Linux")]
        public void ReadOptionalRetryOptionsIfProvided()
        {
            uint attempts = 5;
            var intervalBetweenRetries = TimeSpan.FromSeconds(20);
            var input = requiredInput + $"retries_count: {attempts}{Environment.NewLine}";
            input = input + $"retries_interval: {intervalBetweenRetries}{Environment.NewLine}";

            var options = new YAMLBasedMqttOptionsBuilder().Build(new StringReader(input));
            options.RetryOptions.Should().NotBeNull();
            options.RetryOptions.ShouldRetry.Should().BeTrue();
            options.RetryOptions.Interval.Should().Be(intervalBetweenRetries);
            options.RetryOptions.RetriesCount.Should().Be(attempts);
        }
    }
}