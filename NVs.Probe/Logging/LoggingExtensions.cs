using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using NVs.Probe.Metrics;

namespace NVs.Probe.Logging
{
    internal static class LoggingExtensions
    {
        public static IEnumerable<KeyValuePair<string, object>> ToLogProperties(this MetricConfig config)
        {
            if (config == null) throw new ArgumentNullException(nameof(config));
            yield return new KeyValuePair<string, object>(nameof(config.Metric), config.Metric);
            yield return new KeyValuePair<string, object>(nameof(config.Command), config.Command);
        }

        public static IDisposable WithTopic(this ILogger logger, Metric metric)
        {
            if (metric == null) throw new ArgumentNullException(nameof(metric));
            return logger.BeginScope("Topic", metric.Topic);
        }
    }
}