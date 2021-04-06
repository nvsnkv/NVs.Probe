using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NVs.Probe.Metrics;

namespace NVs.Probe
{
    sealed class Payload : IHostedService
    {
        private readonly IList<MetricConfig> configs;
        private readonly ILogger<Payload> logger;
        private CancellationTokenSource source;

        public Payload(IEnumerable<MetricConfig> configs, ILogger<Payload> logger)
        {
            this.configs = configs?.ToList() ?? throw new ArgumentNullException(nameof(configs));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("Starting service... ");
            try
            {
                source = new CancellationTokenSource();
                throw new NotImplementedException();
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed to start service!");
                throw;
            }

            logger.LogInformation("Service started.");
            return Task.CompletedTask;
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