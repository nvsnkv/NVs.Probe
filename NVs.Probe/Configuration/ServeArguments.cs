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
}