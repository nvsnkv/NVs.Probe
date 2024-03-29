﻿using System;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NVs.Probe.Contract;

namespace NVs.Probe.Server.Shutdown
{
    internal sealed class ShutdownRequestListener : IShutdownRequestListener
    {
        private readonly string name;
        private readonly ILogger<ShutdownRequestListener> logger;
        private readonly Task listener;
        private readonly CancellationTokenSource cts = new();

        public ShutdownRequestListener(string name, ILogger<ShutdownRequestListener> logger)
        {
            this.name = name ?? throw new ArgumentNullException(nameof(name));
            this.logger = logger;
            listener = Task.Factory.StartNew(Listen, cts.Token);
        }

        private async Task Listen()
        {
            using (logger.BeginScope("ShutdownRequestListener {@name}", name))
            {
                var buffer = new byte[1];
                while (!cts.IsCancellationRequested)
                {
                    logger.LogDebug("Awaiting connection...");

                    await using var pipe = new NamedPipeServerStream(name, PipeDirection.InOut, 1, PipeTransmissionMode.Byte);
                    await pipe.WaitForConnectionAsync(cts.Token);
                    if (cts.IsCancellationRequested)
                    {
                        logger.LogInformation("Cancellation requested.");
                        return;
                    }

                    logger.LogDebug("Reading request");
                    var bytesRed = await pipe.ReadAsync(buffer, cts.Token);
                    if (bytesRed != 1)
                    {
                        logger.LogError("Failed to read byte from the pipe!");
                    }
                    else
                    {
                        if (cts.IsCancellationRequested)
                        {
                            logger.LogInformation("Cancellation requested.");
                            return;
                        }

                        using (logger.BeginScope("{@request}", buffer[0]))
                        {
                            switch (buffer[0])
                            {
                                case (byte)Request.Ping:
                                    logger.LogDebug("Received ping request...");
                                    await Reply(pipe, Response.Pong);
                                    break;

                                case (byte)Request.Shutdown:
                                    logger.LogInformation("Received shutdown request.");
                                    RaiseShutdownRequested();
                                    await Reply(pipe, Response.Bye);
                                    break;

                                default:
                                    logger.LogWarning("Unknown request received");
                                    break;
                            }
                        }
                    }
                }
            }
        }

        private void RaiseShutdownRequested()
        {
            logger.LogDebug("Notifying subscribers...");
            var @event = ShutdownRequested;
            @event?.Invoke(this, EventArgs.Empty);
            logger.LogInformation("Notification sent.");
        }

        private async Task Reply(NamedPipeServerStream pipe, Response response)
        {
            using (logger.BeginScope("{@response}", response))
            {
                logger.LogDebug("Replying...");
                await pipe.WriteAsync(new[] { (byte)response }, cts.Token);
                logger.LogInformation("Response sent.");
            }

        }

        public async ValueTask DisposeAsync()
        {
            cts.Cancel();
            await listener;
        }

        public event EventHandler ShutdownRequested;
    }
}