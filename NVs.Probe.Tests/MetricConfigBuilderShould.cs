using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using NVs.Probe.Metrics;
using Xunit;

namespace NVs.Probe.Tests
{
    public class MetricConfigBuilderShould
    {
        [Fact, Trait("Category", "Win"), Trait("Category", "Linux")]
        public void ReadBLockStyledYAMLConfiguration()
        {
            var topics = new[] { "topic1", "topic2", "topic3" };
            var commands = new[] { "command1", "command2", "command3" };

            var input = string.Empty;
            for (var i = 0; i < topics.Length; i++)
            {
                input += $"- topic: {topics[i]}{Environment.NewLine}";
                input += $"  command: {commands[i]}{Environment.NewLine}";
            }

            var result = new YAMLBasedMetricConfigBuilder().Build(new StringReader(input)).ToList();
            result.Should().HaveCount(topics.Length);
            for (var i = 0; i < topics.Length; i++)
            {
                var config = result[i];
                config.Metric.Topic.Should().BeEquivalentTo(topics[i]);
                config.Command.Should().BeEquivalentTo(commands[i]);
            }
        }
    }
}
