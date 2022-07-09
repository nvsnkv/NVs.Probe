using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MQTTnet;
using MQTTnet.Client;
using NVs.Probe.Logging;
using NVs.Probe.Measuring;
using NVs.Probe.Metrics;

namespace NVs.Probe.Mqtt
{
    internal class MqttAdapter : IMqttAdapter, IDisposable
    {
        private readonly CancellationTokenSource internalCancellationTokenSource = new();
        private readonly MqttClientOptions options;
        private readonly RetryOptions retryOptions;
        private readonly IMqttAnnounceBuilder announceBuilder;
        private readonly ILogger<MqttAdapter> logger;
        private readonly IMqttClient client;

        private uint retriesCount;
        private volatile int isReconnecting;

        public MqttAdapter(MqttClientOptions options, MqttFactory factory, RetryOptions retryOptions, IMqttAnnounceBuilder announceBuilder, ILogger<MqttAdapter> logger)
        {
            if (factory == null) throw new ArgumentNullException(nameof(factory));
            this.options = options ?? throw new ArgumentNullException(nameof(options));
            this.retryOptions = retryOptions ?? throw new ArgumentNullException(nameof(retryOptions));
            this.announceBuilder = announceBuilder ?? throw new ArgumentNullException(nameof(announceBuilder));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            client = factory.CreateMqttClient();
        }

        public async Task Announce(IEnumerable<MetricConfig> configs, CancellationToken ct)
        {
            if (configs == null) throw new ArgumentNullException(nameof(configs));
            logger.LogDebug("Announcement started... ");

            if (!client.IsConnected)
            {
                logger.LogWarning("Client is not connected to broker, no announcement will happen!");
                return;
            }

            var messages = announceBuilder.BuildAnnounceMessages(configs, options).ToList();

            try
            {
                var i = 1;
                foreach (var message in messages)
                {
                    using (logger.BeginScope("Topic {@Topic}", message.Topic))
                    {
                        logger.LogDebug("Sending message {i} out of {messagesCount}", i++, messages.Count);
                        await client.PublishAsync(message, ct);
                        logger.LogDebug("Message sent!");

                        ct.ThrowIfCancellationRequested();
                    }
                }
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed to announce metrics!");
                throw;
            }

            logger.LogInformation("Announcement completed!");
        }

        public async Task Notify(SuccessfulMeasurement measurement, CancellationToken ct)
        {
            if (measurement == null) throw new ArgumentNullException(nameof(measurement));
            using (logger.WithTopic(measurement.Metric))
            {
                logger.LogDebug("Notifying broker about successful measurement ...");
                if (!client.IsConnected)
                {
                    logger.LogWarning("Client is not connected, message will not be published!");
                    return;
                }

                var message = new MqttApplicationMessageBuilder()
                    .WithTopic($"{measurement.Metric.Topic}")
                    .WithPayload(measurement.Result)
                    .Build();

                try
                {
                    await client.PublishAsync(message, ct);
                }
                catch (Exception e)
                {
                    logger.LogError(e, "Failed to publish message!");
                    throw;
                }

                logger.LogDebug("Notification sent!");
            }
        }

        public async Task Startup(CancellationToken ct)
        {
            if (internalCancellationTokenSource.IsCancellationRequested)
            {
                throw new ObjectDisposedException(nameof(MqttAdapter));
            }

            logger.LogDebug("Starting adapter... ");
            if (client.IsConnected)
            {
                logger.LogWarning("Adapter is already started!");
                return;
            }

            try
            {
                client.DisconnectedAsync += OnClientDisconnected;
                await Connect(ct);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed to connect to MQTT broker!");
                throw;
            }

            logger.LogInformation("Adapter started.");
        }

        private async Task OnClientDisconnected(MqttClientDisconnectedEventArgs arg)
        {
            using (logger.BeginScope("Reconnect"))
            {
                logger.LogWarning("Client got disconnected!");

                if (!retryOptions.ShouldRetry)
                {
                    logger.LogError("No retries configured! Application cannot proceed!");
                    throw new Exception("Client was disconnected and no retries were configured!");
                }

                if (Interlocked.CompareExchange(ref isReconnecting, 1, 0) != 0)
                {
                    logger.LogWarning("Connection recovery is already in progress, this task will be ended!");
                    return;
                }

                retriesCount = 0;

                try
                {
                    while (retriesCount < retryOptions.RetriesCount && !client.IsConnected)
                    {
                        var delay = retryOptions.Interval * (retriesCount + 1);
                        logger.LogInformation("Attempt: {attempt} of {retriesCount}, Delay: {delay}", retriesCount, retryOptions.RetriesCount, delay);
                        await Task.Delay(delay, internalCancellationTokenSource.Token);
                        internalCancellationTokenSource.Token.ThrowIfCancellationRequested();

                        try
                        {
                            await Connect(internalCancellationTokenSource.Token);
                            logger.LogInformation("Connection was restored!");
                        }
                        catch (Exception e)
                        {
                            logger.LogError(e, "Error occurred during reconnection!");
                            retriesCount++;
                        }
                    }

                    if (!client.IsConnected)
                    {
                        logger.LogError("Unable to reconnect - all attempts were not successful!");
                        throw new Exception("Failed to reconnect to client!");
                    }
                    else
                    {
                        logger.LogInformation("Connection restored!");
                    }
                }
                finally
                {
                    isReconnecting = 0;
                }
            }
        }

        private async Task Connect(CancellationToken ct)
        {
            logger.LogDebug("Connecting to MQTT server... ");
            try
            {
                await client.ConnectAsync(options, ct);
                logger.LogInformation("Client connected to MQTT server.");
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed to connect to MQTT server!");
                throw;
            }
        }

        public async Task Teardown(CancellationToken ct)
        {
            if (internalCancellationTokenSource.IsCancellationRequested)
            {
                throw new ObjectDisposedException(nameof(MqttAdapter));
            }

            logger.LogDebug("Stopping adapter... ");
            try
            {
                client.DisconnectedAsync -= OnClientDisconnected;
                var disconnectOptions = new MqttClientDisconnectOptionsBuilder()
                    .WithReason(MqttClientDisconnectReason.NormalDisconnection)
                    .Build();

                await client.DisconnectAsync(disconnectOptions, ct);
                logger.LogInformation("Adapter stopped.");
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed to stop adapter!");
                throw;
            }
        }

        public void Dispose()
        {
            logger.LogDebug("Dispose requested... ");
            if (internalCancellationTokenSource.IsCancellationRequested)
            {
                logger.LogWarning("Dispose requested for already disposed object!");
                return;
            }
            
            internalCancellationTokenSource.Cancel();

            client.DisconnectedAsync -= OnClientDisconnected;
            client?.Dispose();
            logger.LogInformation("Disposed.");
        }
    }
}