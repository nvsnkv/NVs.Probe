using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Disconnecting;
using MQTTnet.Client.Options;
using NVs.Probe.Logging;
using NVs.Probe.Measurements;
using NVs.Probe.Metrics;

namespace NVs.Probe.Mqtt
{
    internal class MqttAdapter : IMqttAdapter, IDisposable
    {
        private readonly CancellationTokenSource internalCancellationTokenSource = new CancellationTokenSource();
        private readonly IMqttClientOptions options;
        private readonly RetryOptions retryOptions;
        private readonly IMqttAnnounceBuilder announceBuilder;
        private readonly ILogger<MqttAdapter> logger;
        private readonly IMqttClient client;

        private uint retriesCount = 0;

        public MqttAdapter(IMqttClientOptions options, IMqttClientFactory factory, RetryOptions retryOptions, IMqttAnnounceBuilder announceBuilder, ILogger<MqttAdapter> logger)
        {
            if (factory == null) throw new ArgumentNullException(nameof(factory));
            this.options = options ?? throw new ArgumentNullException(nameof(options));
            this.retryOptions = retryOptions ?? throw new ArgumentNullException(nameof(retryOptions));
            this.announceBuilder = announceBuilder ?? throw new ArgumentNullException(nameof(announceBuilder));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            client = factory.CreateMqttClient();
            client.UseDisconnectedHandler(HandleClientDisconnected);
        }

        private async Task HandleClientDisconnected(MqttClientDisconnectedEventArgs arg)
        {
            if (internalCancellationTokenSource.IsCancellationRequested)
            {
                logger.LogWarning("Attempted to handle disconnect on disposed adapter!");
                return;
            }

            if (arg.Exception != null)
            {
                logger.LogError(arg.Exception, "Client got disconnected from the server due to following reason: {@reason}", arg.Reason);
            }
            else
            {
                logger.LogInformation("Client got disconnected from the server due to following reason: {@reason}", arg.Reason);
            }

            if (arg.Reason != MqttClientDisconnectReason.NormalDisconnection || arg.Exception != null)
            {
                if (retryOptions.ShouldRetry)
                {
                    retriesCount++;
                    if (retriesCount < retryOptions.RetriesCount)
                    {
                        var delay = retryOptions.Interval * retriesCount;
                        logger.LogInformation($"Attempting to reconnect in {delay} ({retriesCount} out of {retryOptions.RetriesCount})");
                        await Task.Delay(delay, internalCancellationTokenSource.Token);
                        await Connect(internalCancellationTokenSource.Token);
                    }
                    else
                    {
                        logger.LogError("Maximum retry attempts count reached, connection won't be restored automatically!");
                    }
                }
                else
                {
                    logger.LogWarning("Retry is not configured, connection won't be restored automatically!");
                }
            }
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
                        logger.LogDebug($"Sending message {i++} out of {messages.Count}");
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

        public async Task Start(CancellationToken ct)
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
                await Connect(ct);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed to connect to MQTT broker!");
                throw;
            }

            logger.LogInformation("Adapter started.");
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

        public async Task Stop(CancellationToken ct)
        {
            if (internalCancellationTokenSource.IsCancellationRequested)
            {
                throw new ObjectDisposedException(nameof(MqttAdapter));
            }

            logger.LogDebug("Stopping adapter... ");
            try
            {
                await client.DisconnectAsync(ct);
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
            client.UseDisconnectedHandler((IMqttClientDisconnectedHandler)null);
            client?.Dispose();
            logger.LogInformation("Disposed.");
        }
    }
}