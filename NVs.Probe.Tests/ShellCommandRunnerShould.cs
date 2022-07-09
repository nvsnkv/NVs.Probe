using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NVs.Probe.Execution;
using Xunit;

namespace NVs.Probe.Tests
{
    public sealed class ShellCommandRunnerShould
    {
        private readonly Mock<ILogger<ShellCommandRunner>> logger = new();

        [Fact, Trait("Category", "Windows")]
        public async Task InvokeWindowsCommandsWithPipingSupport()
        {
            var command = "echo 'abc def' | sls 'abc'";
            var runner = new ShellCommandRunner(new RunnerOptions("powershell", "", TimeSpan.FromMilliseconds(10000)), logger.Object);
            var output = await runner.Execute(command, CancellationToken.None);

            output.Should().BeEquivalentTo("abc def");
        }

        [Fact, Trait("Category", "Linux")]
        public async Task InvokeLinuxCommandsWithPipingSupport()
        {
            var command = @"dotnet --info | grep "".NET SDKs""";
            var runner = new ShellCommandRunner(new RunnerOptions("sh", "-c", TimeSpan.FromMilliseconds(1000)), logger.Object);
            var output = await runner.Execute(command, CancellationToken.None);

            output.Should().EndWith(" installed:");
        }
    }
}