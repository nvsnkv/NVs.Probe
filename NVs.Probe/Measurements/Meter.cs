using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NVs.Probe.Metrics;

namespace NVs.Probe.Measurements
{
    sealed class Meter : IMeter
    {
        private readonly string shell;
        private readonly TimeSpan commandTimeout;
        private readonly ILogger<Meter> logger;

        public Meter(string shell, TimeSpan commandTimeout, ILogger<Meter> logger)
        {
            if (string.IsNullOrWhiteSpace(shell))
            {
                throw new ArgumentException($"\"{nameof(shell)}\" should not be null or consists only of white-space characters!", nameof(shell));
            }

            this.shell = shell;
            this.commandTimeout = commandTimeout;
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Measurement> Measure(MetricConfig config, CancellationToken ct)
        {
            if (config is null) throw new ArgumentNullException(nameof(config));
            logger.LogInformation($"Capturing new measurement for{config.Metric.Topic} ...");
            
            var process  = new Process();
            process.StartInfo = new ProcessStartInfo() 
            {
                FileName = shell,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                UseShellExecute = false
            };

            try 
            {
                ct.ThrowIfCancellationRequested();
                
                if (!process.Start())
                    throw new InvalidOperationException("Failed to start process!");

                logger.LogInformation($"Process {process.Id} started.");
                await process.StandardInput.WriteLineAsync(config.Command);
                await process.StandardInput.FlushAsync();
                process.StandardInput.Close();
                logger.LogInformation("Command sent.");

                ct.ThrowIfCancellationRequested();

                if (!process.WaitForExit((int)commandTimeout.TotalMilliseconds))
                    throw new InvalidOperationException($"Process did not exited in {commandTimeout}");

               throw new NotImplementedException();

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
    }
}
