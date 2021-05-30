using System;
using System.Collections.Generic;
using System.Linq;
using CommandLine;
using NVs.Probe.Execution;
using NVs.Probe.Metrics;
using NVs.Probe.Mqtt;

namespace NVs.Probe.Configuration
{
    [Verb("deploy")]
    internal sealed class DeployArguments
    {
        [Option('c', "configuration-path", HelpText = "A path to configuration file for probe", Required = true)]
        public string ConfigurationPath { get; }

        public DeployArguments(string configurationPath)
        {
            ConfigurationPath = configurationPath;
        }

        public ProbeConfiguration GetConfiguration()
        {
            return new YamlConfigBuilder().Build(ConfigurationPath);
        }
    }
}