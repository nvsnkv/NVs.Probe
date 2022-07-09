using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Logging;
using MQTTnet;
using MQTTnet.Client;
using Newtonsoft.Json;
using NVs.Probe.Logging;
using NVs.Probe.Metrics;

namespace NVs.Probe.Mqtt
{
    class MqttAnnounceBuilder : IMqttAnnounceBuilder
    {
        private readonly Assembly assembly;
        private readonly ILogger<MqttAnnounceBuilder> logger;

        public MqttAnnounceBuilder(Assembly assembly, ILogger<MqttAnnounceBuilder> logger)
        {
            this.assembly = assembly ?? throw new ArgumentNullException(nameof(assembly));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public IEnumerable<MqttApplicationMessage> BuildAnnounceMessages(IEnumerable<MetricConfig> configs, MqttClientOptions options)
        {
            if (configs == null) throw new ArgumentNullException(nameof(configs));
            if (options == null) throw new ArgumentNullException(nameof(options));

            var metadata = GetAssemblyMetadata();
            var device = new
            {
                identifiers = new[] { $"id_{options.ClientId}_probe_device" },
                name = $"Probe ({options.ClientId})",
                sw_version = metadata.Version,
                model = metadata.Name,
                manufacturer = metadata.Author
            };

            logger.LogInformation("Device information {@device}", device);

            return configs.Select(c => CreateAnnouncement(c, device, options));
        }

        private MqttApplicationMessage CreateAnnouncement(MetricConfig config, object device, MqttClientOptions options)
        {
            if (config == null) throw new ArgumentNullException(nameof(config));
            if (options == null) throw new ArgumentNullException(nameof(options));

            using (logger.WithTopic(config.Metric))
            {
                try
                {
                    var uniqueId = $"probe_{options.ClientId}_{config.Metric.Topic.Replace('/', '_')}";
                    using (logger.BeginScope($"UniqueId: {uniqueId}"))
                    {
                        var payload = new
                        {
                            state_topic = config.Metric.Topic,
                            name = config.Metric.Topic.Split('/').Last(),
                            unique_id = uniqueId,
                            device
                        };
                        var message = new MqttApplicationMessageBuilder()
                            .WithTopic($"homeassistant/sensor/probe/{uniqueId}/config")
                            .WithPayload(JsonConvert.SerializeObject(payload))
                            .Build();

                        logger.LogInformation("Announcement built. Payload: {@payload}", payload);
                        return message;
                    }
                }
                catch (Exception e)
                {
                    logger.LogError(e, "Failed to create announcement!");
                    throw;
                }
            }
        }

        private Metadata GetAssemblyMetadata()
        {
            string name = null, author = null, version = null;
            foreach (var attribute in assembly.GetCustomAttributes())
            {
                switch (attribute)
                {
                    case AssemblyTitleAttribute t:
                        name = t.Title;
                        break;

                    case AssemblyCompanyAttribute c:
                        author = c.Company;
                        break;

                    case AssemblyVersionAttribute v:
                        version = v.Version;
                        break;
                }
            }

            if (version is null)
            {
                logger.LogWarning("AssemblyVersion attribute was not properly populated!");
                version = assembly.GetName().Version?.ToString();
            }

            if (name is null) { logger.LogWarning("AssemblyTitle attribute was not properly populated!"); }
            if (author is null) { logger.LogWarning("AssemblyCompany attribute was not properly populated!"); }
            if (version is null) { logger.LogWarning("Version was not recognized!"); }

            return new Metadata(name, author, version);
        }

        private readonly struct Metadata
        {
            public readonly string Name;

            public readonly string Author;

            public readonly string Version;

            public Metadata(string name, string author, string version)
            {
                Name = name;
                Author = author;
                Version = version;
            }
        }
    }
}