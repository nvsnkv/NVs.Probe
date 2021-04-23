using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NVs.Probe.Metrics;
using Xunit;

namespace NVs.Probe.Tests
{
    public class MetricConfigBuilderShould
    {
        [Fact]
        public void BuildTheConfigFromArgs()
        {
            var topics = new[] { "topic1", "topic2", "topic3" };
            var commands = new[] { "command1", "command2", "command3" };

            IEnumerable<string> GetArgs()
            {
                for (var i = 0; i < topics.Length; i++)
                {
                    yield return topics[i];
                    yield return commands[i];
                }
            }

            var configs = new MetricConfigBuilder(GetArgs()).Build().ToList();

            configs.Should().HaveCount(topics.Length);
            for (var i = 0; i < topics.Length; i++)
            {
                var config = configs[i];
                config.Metric.Topic.Should().BeEquivalentTo(topics[i]);
                config.Command.Should().BeEquivalentTo(commands[i]);
            }

        }
    }
}
