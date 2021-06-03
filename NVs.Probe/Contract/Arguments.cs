using System;
using CommandLine;
using NVs.Probe.Configuration;

// ReSharper disable ClassNeverInstantiated.Global

namespace NVs.Probe.Contract
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

    [Verb("stub")]
    internal sealed class StubArguments
    {
        [Value(0, Required = false, HelpText = "An instance identifier for new instance of probe", Default = null)]
        public string InstanceId { get; }

        public StubArguments(string instanceId)
        {
            InstanceId = instanceId ?? Guid.NewGuid().ToString();
        }
    }

    [Verb("deploy")]
    internal sealed class DeployArguments
    {
        [Option('c', "configuration-path", HelpText = "A path to configuration file for new instance of probe", Required = true)]
        public string ConfigurationPath { get; }

        [Value(0, Required = true, HelpText = "An identifier for new instance of probe", Default = null)]
        public string InstanceId { get; }

        [Option('v', "verbose", Required = false, Default = false, HelpText = "Enable verbose output")]
        public bool Verbose { get; }

        [Option('s', "stub", Required = false, Default = false, HelpText = "Deploy a dummy version of probe - useful for development and makes no sense in production")]
        public bool Stub { get; }

        public DeployArguments(string configurationPath, string instanceId, bool verbose, bool stub)
        {
            ConfigurationPath = configurationPath;
            Verbose = verbose;
            Stub = stub;
            InstanceId = instanceId ?? Guid.NewGuid().ToString();
        }
    }

    [Verb("stop")]
    internal sealed class StopArguments
    {
        [Value(0, Required = true, HelpText = "An identifier of existing probe instance to stop", Default = null)]
        public string InstanceId { get; }

        [Option('v', "verbose", Required = false, Default = false, HelpText = "Enable verbose output")]
        public bool Verbose { get; }


        public StopArguments(string instanceId, bool verbose)
        {
            InstanceId = instanceId;
            Verbose = verbose;
        }
    }
}