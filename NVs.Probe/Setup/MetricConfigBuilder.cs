using System;
using System.Collections.Generic;
using NVs.Probe.Metrics;

namespace NVs.Probe.Setup
{
    internal sealed class MetricConfigBuilder
    {
        private readonly IEnumerable<string> args;

        public MetricConfigBuilder(IEnumerable<string> args)
        {
            this.args = args ?? throw new ArgumentNullException(nameof(args));
        }

        public IEnumerable<MetricConfig> Build()
        {
            string topic = null;
            foreach (var arg in args)
            {
                if (topic == null)
                {
                    topic = arg;
                }
                else
                {
                    MetricConfig result;
                    try
                    {
                        result = new MetricConfig(new Metric(topic), arg);
                    }
                    catch (Exception e)
                    {
                        throw new InvalidOperationException($"Unable to create a config for topic {topic}!", e);
                    }

                    yield return result;
                    topic = null;
                }
            }

            if (topic != null)
            {
                throw new InvalidOperationException($"Argument list given does not provide a command for topic {topic}!");
            }
        }
    }
}