using System.Linq;
using FluentAssertions;
using NVs.Probe.Setup;
using Xunit;

namespace NVs.Probe.Tests
{
    public sealed class ArgParserShould
    {
        [Fact]
        public void ParseArguments()
        {
            var clientId = "expectedClientId";
            var server = "expectedServer";
            int port = 2020;
            var user = "expectedUser";
            var password = "expextedPassword";
            var seriesInterval = 750;
            var measurementTimeout = 100;

            var args = new[]
            {
                "-c", clientId, "-s", server, "--port", port.ToString(), "-u", user, "-p", password, "-i", seriesInterval.ToString(), "-t", measurementTimeout.ToString(), 
                "--", "topic", "command"
            };

            var (options, configs) = new ArgsParser().Parse(args);

            options.ClientId.Should().BeSameAs(clientId);
            options.Server.Should().BeSameAs(server);
            options.Port.Should().Be(port);
            options.User.Should().BeSameAs(user);
            options.Password.Should().BeSameAs(password);
            options.SeriesInterval.Should().Be(seriesInterval);
            options.MeasurementTimeout.Should().Be(measurementTimeout);

            var configsList = configs.ToList();
            configsList.Should().HaveCount(1);
        }
    }
}