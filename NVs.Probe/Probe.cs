using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NVs.Probe.Measuring;
using NVs.Probe.Metrics;
using NVs.Probe.Mqtt;

namespace NVs.Probe
{
    internal sealed class Probe : IHostedService
    {
        private readonly IReadOnlyList<MetricConfig> configs;
        private readonly TimeSpan delay;
        private readonly IMeter meter;
        private readonly IMqttAdapter adapter;
        private readonly ILogger<Probe> logger;

        private CancellationTokenSource source;

        public Probe(ProbeOptions options, IMeter meter, IMqttAdapter adapter, ILogger<Probe> logger)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));
            this.configs = options.Metrics;
            this.delay = options.InterSeriesDelay;
            this.meter = meter ?? throw new ArgumentNullException(nameof(meter));
            this.adapter = adapter ?? throw new ArgumentNullException(nameof(adapter));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            logger.LogDebug("Starting service... ");
            try
            {
                await adapter.Start(cancellationToken);
                await adapter.Announce(configs, cancellationToken);

                source = new CancellationTokenSource();
                var _ = Task.Factory.StartNew(async () =>
                {
                    using (logger.BeginScope("Measurement cycle"))
                    {
                        while (!source.IsCancellationRequested)
                        {
                            logger.LogDebug("Measurement series started.");

                            foreach (var config in configs)
                            {
                                using (logger.BeginScope("Config: {@config}", config))
                                {
                                    var result = await meter.Measure(config, source.Token);
                                    if (source.IsCancellationRequested) break;

                                    switch (result)
                                    {
                                        case null:
                                            throw new ArgumentNullException(nameof(result));

                                        case SuccessfulMeasurement successful:
                                            await adapter.Notify(successful, source.Token);
                                            break;

                                        case FailedMeasurement failure:
                                            logger.LogError(failure.Exception, $"Failed to measure {failure.Metric.Topic}");
                                            break;

                                        default:
                                            throw new NotSupportedException(
                                                $"Unknown measurement {result.GetType().Name} type received!");
                                    }

                                    if (source.IsCancellationRequested) break;

                                    logger.LogDebug("Measurement series completed.");
                                }
                            }

                            await Task.Delay(delay, source.Token);
                        }

                        logger.LogInformation("Measurement cycle stopped.");
                    }
                }, cancellationToken);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed to start service!");
                throw;
            }

            logger.LogInformation("Service started.");
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            logger.LogDebug("Stopping service... ");
            try
            {
                source.Cancel();
                source.Dispose();
                await adapter.Stop(cancellationToken);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed to stop service!");
                throw;
            }

            logger.LogInformation("Service stopped.");
        }
    }
}