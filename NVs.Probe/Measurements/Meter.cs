using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CliWrap;
using CliWrap.Buffered;
using Microsoft.Extensions.Logging;
using NVs.Probe.Logging;
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
            using (logger.WithTopic(config.Metric))
            {
                logger.LogDebug($"Capturing new measurement ...");

                var command = Wrap(config.Command)
                    .WithValidation(CommandResultValidation.None);

                try
                {
                    ct.ThrowIfCancellationRequested();

                    var result = await command.ExecuteBufferedAsync(ct);
                    using (logger.BeginScope("Exit code: {@exitCode}", result.ExitCode))
                    {
                        logger.LogDebug("Command executed.");
                        var output = result.StandardOutput.TrimEnd();
                        var error = result.StandardError.TrimEnd();
                        if (result.ExitCode != 0)
                        {
                            logger.LogWarning("Command returned non-zero exit code!");
                            throw new InvalidOperationException($"Exit code: {result.ExitCode}{Environment.NewLine}Output: {output}{Environment.NewLine}  Error: {error}");
                        }

                        return new SuccessfulMeasurement(config.Metric, output);
                    }
                }
                catch (Exception e)
                {
                    logger.LogError(e, $"Failed to measure {config.Metric.Topic} !");
                    return new FailedMeasurement(config.Metric, e);
                }
                finally
                {
                    command.Dispose();
                }
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

        private Command Wrap(string command)
        {
            return command.Split('|')
                .Reverse()
                .Select(s => 
                    Cli.Wrap(s.Split(' ').First())
                    .WithArguments(s.Split(' ').Skip(1)))
                .Aggregate((s, a) => s | a);

        }
    }
}
