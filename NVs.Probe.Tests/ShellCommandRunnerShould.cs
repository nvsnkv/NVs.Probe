using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NVs.Probe.Measurements.CommandRunner;
using Xunit;

namespace NVs.Probe.Tests
{
    public sealed class ShellCommandRunnerShould
    {
        private readonly Mock<ILogger<ShellCommandRunner>> logger = new Mock<ILogger<ShellCommandRunner>>();

        [Fact, Trait("Category", "Windows")]
        public async Task InvokeWindowsCommandsWithPipingSupport()
        {
            var command = "echo 'abc def' | find abc";
            var runner = new ShellCommandRunner(new RunnerOptions("cmd", "/c"), TimeSpan.FromMilliseconds(1000), logger.Object);
            var output = await runner.Execute(command, CancellationToken.None);

            output.Should().BeEquivalentTo("abc");
        }

        [Fact, Trait("Category", "Linux")]
        public async Task InvokeLinuxCommandsWithPipingSupport()
        {
            var command = "ps -ax | grep ps";
            var runner = new ShellCommandRunner(new RunnerOptions("sh", "-c"), TimeSpan.FromMilliseconds(1000), logger.Object);
            var output = await runner.Execute(command, CancellationToken.None);

            output.Should().EndWith(" ps");
        }
    }
}