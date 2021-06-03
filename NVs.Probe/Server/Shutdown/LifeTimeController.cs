using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace NVs.Probe.Server.Shutdown
{
    internal sealed class LifeTimeController : IHostedService
    {
        private readonly Func<IShutdownRequestListener> listenerFactory;
        private readonly IHostApplicationLifetime lifetime;
        private readonly ILogger<LifeTimeController> logger;
        
        private int isRunning;
        private IShutdownRequestListener listener;

        public LifeTimeController(Func<IShutdownRequestListener> listenerFactory, IHostApplicationLifetime lifetime,
            ILogger<LifeTimeController> logger)
        {
            this.listenerFactory = listenerFactory ?? throw new ArgumentNullException(nameof(listenerFactory));
            this.lifetime = lifetime ?? throw new ArgumentNullException(nameof(lifetime));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            if (Interlocked.CompareExchange(ref isRunning, 1, 0) == 1)
            {
                logger.LogWarning("An attempt was made to start already running service!");
                return;
            }

            try
            {
                if (listener != null)
                {
                    logger.LogWarning("Previous listener was not cleaned up, disposing it now.");
                    await DisposeListener();
                }

                listener = listenerFactory();
                listener.ShutdownRequested += Shutdown;
                logger.LogInformation("Service started.");
            }
            catch (Exception e)
            {
                logger.LogCritical(e, "Failed to start shutdown listener!");
                isRunning = 0;
            }
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            if (Interlocked.CompareExchange(ref isRunning, 0, 1) == 0)
            {
                logger.LogWarning("An attempt was made to stop already stopped service!");
                return;
            }

            try
            {
                await DisposeListener();
                logger.LogInformation("Service stopped.");
            }
            catch (Exception e)
            {
                //assuming service was stopped, leaving isRunning == 0

                logger.LogCritical(e, "Failed to stop shutdown listener!");
                throw;
            }
        }

        private async Task DisposeListener()
        {
            listener.ShutdownRequested -= Shutdown;
            await listener.DisposeAsync();
        }

        private void Shutdown(object sender, EventArgs e)
        {
            logger.LogInformation("Shutdown requested, terminating application.");
            lifetime.StopApplication();
        }
    }

}

     