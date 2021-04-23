using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NVs.Probe.Metrics;

namespace NVs.Probe.Measurements
{
    internal sealed class Meter : IMeter
    {
        private readonly TimeSpan commandTimeout;
        private readonly ILogger<Meter> logger;

        public Meter(TimeSpan commandTimeout, ILogger<Meter> logger)
        {
            this.commandTimeout = commandTimeout;
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Measurement> Measure(MetricConfig config, CancellationToken ct)
        {
            if (config is null) throw new ArgumentNullException(nameof(config));
            logger.LogDebug($"Capturing new measurement for{config.Metric.Topic} ...");

            var (filename, args) = ParseCommand(config.Command);

            var process = new Process
            {
                StartInfo = new ProcessStartInfo()
                {
                    FileName = filename,
                    Arguments = args,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                    UseShellExecute = false
                }
            };

            try
            {
                ct.ThrowIfCancellationRequested();

                if (!process.Start())
                    throw new InvalidOperationException("Failed to start process!");

                logger.LogDebug($"Process {process.Id} started.");

                ct.ThrowIfCancellationRequested();

                if (!process.WaitForExit((int)commandTimeout.TotalMilliseconds))
                    throw new InvalidOperationException($"Process did not exited in {commandTimeout}");

                logger.LogDebug("Process stopped.");
                var result = await process.StandardOutput.ReadToEndAsync();
                result = result.TrimEnd();

                if (process.ExitCode != 0)
                {
                    var stdErr = await process.StandardError.ReadToEndAsync();
                    if (string.IsNullOrEmpty(stdErr))
                    {
                        stdErr = await process.StandardOutput.ReadToEndAsync();
                    }

                    throw new InvalidOperationException($"Exit code: {process.ExitCode}{Environment.NewLine}Output: {result}{Environment.NewLine}  Error: {stdErr}");
                }

                logger.LogDebug("Measurement completed.");
                return new SuccessfulMeasurement(config.Metric, result);
            }
            catch (Exception e)
            {
                logger.LogError(e, $"Failed to measure {config.Metric.Topic} !");
                return new FailedMeasurement(config.Metric, e);
            }
            finally
            {
                process.Dispose();
            }
        }

        private Tuple<string, string> ParseCommand(string command)
        {
            var i = 0;
            if ("'\"".Contains(command[i]))
            {
                i = command.IndexOf(command[i], 1);
                if (i < 0)
                {
                    throw new ArgumentException("Failed to parse the command - unable to identify path to executable!")
                    {
                        Data = { { "Command", command } }
                    };
                }
            }
            else
            {
                i = command.IndexOf(' ');
            }

            return i < 0 
                ? new Tuple<string, string>(command, string.Empty) 
                : new Tuple<string, string>(command.Substring(0, i), command[i..]);
        }
    }
}
