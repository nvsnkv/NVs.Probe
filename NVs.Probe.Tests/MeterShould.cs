using System;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NVs.Probe.Measurements;
using NVs.Probe.Metrics;
using Serilog;
using Xunit;

namespace NVs.Probe.Tests
{
    public sealed class MeterShould
    {
        private readonly Mock<ILogger<Meter>> logger = new Mock<ILogger<Meter>>();

        [Fact]
        public async Task ProvideResultsForSuccessfulMeasurement()
        {
            var config = new MetricConfig(new Metric("dotnet/version"), "dotnet --version");

            var meter = new Meter(TimeSpan.FromMilliseconds(100), logger.Object);
            var result = await meter.Measure(config, CancellationToken.None);

            result.Should().BeOfType<SuccessfulMeasurement>();
            var success = result as SuccessfulMeasurement;

            // ReSharper disable once PossibleNullReferenceException
            success.Result.Should().Match(s => Regex.IsMatch(s, @"\d+.\d+.\d+"));
        }
    }
}