using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using NVs.Probe.Metrics;

namespace NVs.Probe.Logging
{
    internal static class LoggingExtensions
    {
        public static IDisposable WithTopic(this ILogger logger, Metric metric)
        {
            if (metric == null) throw new ArgumentNullException(nameof(metric));
            return logger.BeginScope("Topic: {@topic}", metric.Topic);
        }
    }
}