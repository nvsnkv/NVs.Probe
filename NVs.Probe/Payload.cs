using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NVs.Probe.Measurements;
using NVs.Probe.Metrics;
using NVs.Probe.Mqtt;

namespace NVs.Probe
{
    sealed class Payload : IHostedService
    {
        private readonly IList<MetricConfig> configs;
        private readonly IMeter meter;
        private readonly IMqttAdapter adapter;
        private readonly ILogger<Payload> logger;

        private CancellationTokenSource source;

        public Payload(IEnumerable<MetricConfig> configs, IMeter meter, IMqttAdapter adapter, ILogger<Payload> logger)
        {
            this.configs = configs?.ToList() ?? throw new ArgumentNullException(nameof(configs));
            this.meter = meter ?? throw new ArgumentNullException(nameof(meter));
            this.adapter = adapter ?? throw new ArgumentNullException(nameof(adapter));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("Starting service... ");
            try
            {
                await adapter.Start(cancellationToken);
                await adapter.Announce(configs, cancellationToken);

                source = new CancellationTokenSource();
                var _ = Task.Factory.StartNew(async() => {
                    while (!source.IsCancellationRequested) 
                    {
                        logger.LogInformation("Measurement series started.");

                        foreach(var config in configs) 
                        {
                            var result = await meter.Measure(config, source.Token);
                            if (source.IsCancellationRequested) break;

                            switch(result) 
                            {
                                case null:
                                    throw new ArgumentNullException(nameof(result));

                                case SuccessfulMeasurement successful:
                                    await adapter.Notify(successful, source.Token);
                                break;
                                
                                case FailedMeasurement failure:
                                    throw new NotImplementedException("TODO: implement error notification");
                                
                                default:
                                    throw new NotSupportedException($"Unkwnown measurement {result.GetType().Name} type received!");
                            }

                            if (source.IsCancellationRequested) break;
                        }

                        logger.LogInformation("Measurement series completed.");
                        await Task.Delay(TimeSpan.FromMinutes(1), source.Token);
                    }

                    logger.LogInformation("Measurement loop cancelled");
                }, cancellationToken);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed to start service!");
                throw;
            }

            logger.LogInformation("Service started.");
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("Stopping service... ");
            try
            {
                source.Cancel();
                source.Dispose();
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed to stop service!");
                throw;
            }

            logger.LogInformation("Service started.");
            return Task.CompletedTask;
        }
    }
}