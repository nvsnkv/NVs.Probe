using System;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NVs.Probe.Execution;
using NVs.Probe.Measuring;
using NVs.Probe.Metrics;
using Xunit;

namespace NVs.Probe.Tests
{
    public sealed class MeterShould
    {
        private readonly Mock<ILogger<Meter>> logger = new Mock<ILogger<Meter>>();
        private readonly Mock<ICommandRunner> runner = new Mock<ICommandRunner>();

        [Fact, Trait("Category", "Win"), Trait("Category", "Linux")]
        public async Task ProvideResultsForSuccessfulMeasurement()
        {
            
            var config = new MetricConfig(new Metric("dotnet/version"), "dotnet --version");
            runner
                .Setup(r => r.Execute(config.Command, It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(Environment.Version.ToString()));

            var meter = new Meter(runner.Object, logger.Object);
            var result = await meter.Measure(config, CancellationToken.None);

            result.Metric.Should().BeSameAs(config.Metric);
            result.Should().BeOfType<SuccessfulMeasurement>();
            var success = result as SuccessfulMeasurement;

            // ReSharper disable once PossibleNullReferenceException
            success.Result.Should().Match(s => Regex.IsMatch(s, @"\d+.\d+.\d+"));
        }

        [Fact, Trait("Category", "Win"), Trait("Category", "Linux")]
        public async Task ReturnFailedMeasurementFromFailedCommand()
        {
            var config = new MetricConfig(new Metric("failed/measurement"), "some random stuff");
            runner
                .Setup(r => r.Execute(config.Command, It.IsAny<CancellationToken>()))
                .Throws<DivideByZeroException>();

            var meter = new Meter(runner.Object, logger.Object);
            var result = await meter.Measure(config, CancellationToken.None);

            result.Metric.Should().BeSameAs(config.Metric);
            result.Should().BeOfType(typeof(FailedMeasurement));
            var failure = result as FailedMeasurement;

            // ReSharper disable once PossibleNullReferenceException
            failure.Exception.Should().BeOfType<DivideByZeroException>();

        }
    }
}