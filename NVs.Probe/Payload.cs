using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace NVs.Probe
{
    sealed class Payload : IHostedService
    {
        private readonly ILogger<Payload> logger;

        public Payload(ILogger<Payload> logger)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("Starting service... ");
            throw new NotImplementedException();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("Stopping service... ");
            throw new NotImplementedException();
        }
    }
}