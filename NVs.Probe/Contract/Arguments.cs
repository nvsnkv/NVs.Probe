using System;
using CommandLine;
using NVs.Probe.Configuration;

// ReSharper disable ClassNeverInstantiated.Global

namespace NVs.Probe.Contract
{
    [Verb("serve", HelpText = "Launch a new instance of probe and wait until it ended")]
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

    [Verb("stub", HelpText = "Launch a probe with a dummy payload and wait until it ended. Useful for debugging, useless in production")]
    internal sealed class StubArguments
    {
        [Value(0, Required = false, HelpText = "An instance identifier for new instance of probe", Default = null)]
        public string InstanceId { get; }

        public StubArguments(string instanceId)
        {
            InstanceId = instanceId ?? Guid.NewGuid().ToString();
        }
    }

    [Verb("deploy", HelpText = "Launch a probe in the new process and exit")]
    internal sealed class DeployArguments
    {
        [Option('c', "configuration-path", HelpText = "A path to configuration file for new instance of probe", Required = true)]
        public string ConfigurationPath { get; }

        [Value(0, Required = false, HelpText = "An identifier for new instance of probe", Default = null)]
        public string InstanceId { get; }

        [Option('v', "verbose", Required = false, Default = false, HelpText = "Enable verbose output")]
        public bool Verbose { get; }

        [Option('s', "stub", Required = false, Default = false, HelpText = "Deploy a dummy version of probe - useful for development and makes no sense in production")]
        public bool Stub { get; }

        [Option('t', "connection-timeout", Required = false, Default = 1900, HelpText = "Communications timeout (in milliseconds)")]
        public long Timeout { get; }

        public DeployArguments(string configurationPath, string instanceId, bool verbose, bool stub, long timeout)
        {
            ConfigurationPath = configurationPath;
            Verbose = verbose;
            Stub = stub;
            Timeout = timeout;
            InstanceId = instanceId ?? Guid.NewGuid().ToString();
        }
    }

    [Verb("stop", HelpText = "Stop previously started instance")]
    internal sealed class StopArguments
    {
        [Value(0, Required = true, HelpText = "An identifier of existing probe instance to stop", Default = null)]
        public string InstanceId { get; }

        [Option('v', "verbose", Required = false, Default = false, HelpText = "Enable verbose output")]
        public bool Verbose { get; }


        [Option('t', "connection-timeout", Required = false, Default = 1000, HelpText = "Communications timeout (in milliseconds)")]
        public long Timeout { get; }

        public StopArguments(string instanceId, bool verbose, long timeout)
        {
            InstanceId = instanceId;
            Verbose = verbose;
            Timeout = timeout;
        }
    }
}