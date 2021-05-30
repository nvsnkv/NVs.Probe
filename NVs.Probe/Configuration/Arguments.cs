using System;
using CommandLine;

// ReSharper disable ClassNeverInstantiated.Global

namespace NVs.Probe.Configuration
{
    [Verb("serve")]
    internal sealed class ServeArguments
    {
        [Option('c', "configuration-path", HelpText = "A path to configuration file for probe", Required = true)]
        public string ConfigurationPath { get; }

        [Option('i', "instance-id", Required = true, HelpText = "Instance identified used to communicate with it")]
        public string InstanceId { get; }

        public ServeArguments(string configurationPath, string instanceId)
        {
            ConfigurationPath = configurationPath;
            InstanceId = instanceId;
        }

        public ProbeConfiguration GetConfiguration()
        {
            return new YamlConfigBuilder().Build(ConfigurationPath);
        }
    }

    [Verb("deploy")]
    internal sealed class DeployArguments
    {
        [Option('c', "configuration-path", HelpText = "A path to configuration file for new instance of probe", Required = true)]
        public string ConfigurationPath { get; }

        [Value(0, Required = false, HelpText = "An instance identifier for new instance of probe", Default = null)]
        public string InstanceId { get; }

        [Option('v', "verbose", Required = false, HelpText = "Enable verbose output")]
        public bool Verbose { get; }

        public DeployArguments(string configurationPath, string instanceId, bool verbose)
        {
            ConfigurationPath = configurationPath;
            Verbose = verbose;
            InstanceId = instanceId ?? Guid.NewGuid().ToString();
        }
    }
}