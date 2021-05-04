using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace NVs.Probe.Measurements.CommandRunner
{
    internal sealed class ShellCommandRunner : ICommandRunner
    {
        private readonly string shell;
        private readonly string flags;
        private readonly int commandTimeout;
        private readonly ILogger<ShellCommandRunner> logger;

        public ShellCommandRunner(RunnerOptions options, TimeSpan commandTimeout, ILogger<ShellCommandRunner> logger)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));
            this.shell = options.Shell;
            this.flags = options.Flags;

            this.commandTimeout = (int)commandTimeout.TotalMilliseconds;
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        public async Task<string> Execute(string command, CancellationToken ct)
        {
            logger.LogDebug("Creating process...");

            var processStartInfo = new ProcessStartInfo(shell)
            {
                ArgumentList = {flags, command},
                RedirectStandardError = true,
                RedirectStandardOutput = true
            };

            var process = new Process()
            {
                StartInfo = processStartInfo
            };

            ct.ThrowIfCancellationRequested();

            try
            {
                logger.LogDebug("Starting the process");
                process.Start();
                using (logger.BeginScope("Running process {@processId}", process.Id))
                {
                    logger.LogDebug("Started process");
                    if (!process.WaitForExit(commandTimeout))
                    {
                        throw new InvalidOperationException($"The process was not ended within {commandTimeout} ms!");
                    }

                    if (process.ExitCode != 0)
                    {
                        var errors = (await process.StandardError.ReadToEndAsync()).Trim();
                        throw new IOException($"Process exited with exit code {process.ExitCode}!{Environment.NewLine}StdErr: {errors}");
                    }

                    return (await process.StandardOutput.ReadToEndAsync()).Trim();
                }
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed to execute {@command}", command);
                throw;
            }
            finally
            {
                process.Dispose();
            }
        }
    }
}