using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace NVs.Probe.Server
{
    internal sealed class Stub : IHostedService
    {
        private readonly string instanceId;
        private readonly ILogger<Stub> logger;
        private readonly CancellationTokenSource cts;
        private Task payload;

        public Stub(string instanceId, ILogger<Stub> logger)
        {
            this.instanceId = instanceId ?? throw new ArgumentNullException(nameof(instanceId));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.cts = new CancellationTokenSource();
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            payload = Task.Factory.StartNew(StubPayload, cancellationToken);
            logger.LogDebug("Dummy payload started!");
            return Task.CompletedTask;
        }

        private async Task StubPayload()
        {
            var counter = 0;
            var delay = TimeSpan.FromMilliseconds(333);
            while (!cts.IsCancellationRequested)
            {
                Console.Clear();

                Console.WriteLine("PROBE - DUMMY PAYLOAD");
                Console.WriteLine($"  instance id: {instanceId}");
                Console.WriteLine($"  command line: {Environment.CommandLine}");
                Console.WriteLine($"  working directory: {Environment.CurrentDirectory}");
                switch (counter % 4)
                {
                    case 0:
                    {
                        Console.WriteLine("╔════╤╤╤╤════╗");
                        Console.WriteLine("║    │││ \\   ║");
                        Console.WriteLine("║    │││  O  ║");
                        Console.WriteLine("║    OOO     ║");
                        break;
                    };
                    case 1:
                    {
                        Console.WriteLine("╔════╤╤╤╤════╗");
                        Console.WriteLine("║    ││││    ║");
                        Console.WriteLine("║    ││││    ║");
                        Console.WriteLine("║    OOOO    ║");
                        break;
                    };
                    case 2:
                    {
                        Console.WriteLine("╔════╤╤╤╤════╗");
                        Console.WriteLine("║   / │││    ║");
                        Console.WriteLine("║  O  │││    ║");
                        Console.WriteLine("║     OOO    ║");
                        break;
                    };
                    case 3:
                    {
                        Console.WriteLine("╔════╤╤╤╤════╗");
                        Console.WriteLine("║    ││││    ║");
                        Console.WriteLine("║    ││││    ║");
                        Console.WriteLine("║    OOOO    ║");
                        break;
                    };
                }

                counter++;
                await Task.Delay(delay, cts.Token);
            }
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            cts.Cancel();
            await payload;
            logger.LogDebug("Dummy payload stopped!");
        }
    }
}