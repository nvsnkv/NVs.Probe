using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NVs.Probe.Execution;
using NVs.Probe.Logging;
using NVs.Probe.Metrics;

namespace NVs.Probe.Measuring
{
    internal sealed class Meter : IMeter
    {
        private readonly ILogger<Meter> logger;
        private readonly ICommandRunner runner;

        public Meter(ICommandRunner runner, ILogger<Meter> logger)
        {
            this.runner = runner ?? throw new ArgumentNullException(nameof(runner));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Measurement> Measure(MetricConfig config, CancellationToken ct)
        {
            if (config is null) throw new ArgumentNullException(nameof(config));
            using (logger.WithTopic(config.Metric))
            {
                logger.LogDebug($"Capturing new measurement ...");

                
                try
                {
                    ct.ThrowIfCancellationRequested();

                    var output = await runner.Execute(config.Command, ct);
                    logger.LogDebug("Measurement successfully completed!");
                    return new SuccessfulMeasurement(config.Metric, output);
                }
                catch (Exception e)
                {
                    logger.LogError(e, "Failed to measure {topic} !", config.Metric.Topic);
                    return new FailedMeasurement(config.Metric, e);
                }
            }
        }
    }
}
