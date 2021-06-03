using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using CommandLine;
using NVs.Probe.Configuration;
using NVs.Probe.Contract;

namespace NVs.Probe.Client
{
    internal sealed class Bootstrapper
    {
        private readonly Parser argsParser;
        private readonly IConsole console;
        private readonly Func<string, IPipeClient> createClient;

        public Bootstrapper(Parser argsParser, IConsole console, Func<string, IPipeClient> createClient)
        {
            this.argsParser = argsParser ?? throw new ArgumentNullException(nameof(argsParser));
            this.console = console ?? throw new ArgumentNullException(nameof(console));
            this.createClient = createClient ?? throw new ArgumentNullException(nameof(createClient));
        }

        public async Task Start(string id, string configPath, bool runStub)
        {
            console.WriteVerbose("Attempting to deploy new instance...");
            console.WriteVerbose($"  instance id: {id}");
            console.WriteVerbose($"  config path: {configPath}");
            if (runStub)
            {
                console.WriteWarning("Warning: Stub version of probe requested!");
            }

            if (string.IsNullOrEmpty(id))
            {
                console.WriteError("Unable to deploy new instance of probe: no id provided!");
                throw new ArgumentNullException(nameof(id));
            }

            if (!File.Exists(configPath))
            {
                console.WriteError("Unable to deploy new instance of probe: configuration file does not exists, or inaccessible!");
                throw new FileNotFoundException("Configuration file not found!", configPath);
            }

            console.WriteVerbose("Parameters verified, collecting details to start new process...");
            var path = GetExecutablePath();
            var args = runStub
                    ? argsParser.FormatCommandLine(new StubArguments(id))
                    : argsParser.FormatCommandLine(new ServeArguments(configPath, id));

            var info = path.EndsWith(".dll")
                ? new ProcessStartInfo("dotnet", $"\"{path}\" {args}")
                : new ProcessStartInfo(path, args);

            console.WriteVerbose("Start info collected:");
            console.WriteVerbose($"  command: {info.FileName}");
            console.WriteVerbose($"  arguments: {info.Arguments}");

            console.WriteVerbose("Starting new instance...");
            var process = Process.Start(info);
            if (process == null)
            {
                console.WriteError("Unable to deploy new instance of probe: failed to start process!");
                throw new InvalidOperationException("Process instance was not created!");
            }

            console.WriteVerbose($"Process {process.Id} stated. Waiting 2 seconds to give it some time to initialize server...");
            await Task.Delay(TimeSpan.FromSeconds(2));

            try
            {
                console.WriteVerbose("Attempting to connect to deployed instance...");
                await using var client = createClient(id);
                var response = await client.Send(Request.Ping);
                console.WriteVerbose($"Newly deployed instance responded with {response}.");
                if (response != Response.Pong)
                {
                    console.WriteError("Failed to connect to deployed instance - instance returned unexpected response!");
                    console.WriteVerbose($"Killing process {process.Id}...");
                    process.Kill();
                    console.WriteVerbose("Request to terminate process sent. Bootstrapper will be terminated now.");
                    return;
                }
            }
            catch (Exception e)
            {
                console.WriteError("Unable to connect to deployed instance - exception occurred!");
                console.WriteVerbose(e.ToString());
                throw;
            }

            console.WriteVerbose("Instance successfully started. Instance id is:");
            console.WriteLine(id);

            console.WriteVerbose("Have a nice day!");
        }

        private string GetExecutablePath()
        {
            var assembly = Assembly.GetCallingAssembly();
            if (assembly?.CodeBase == null)
            {
                console.WriteError("Unable to deploy new instance of probe: unable to locate assembly!");
                throw new InvalidOperationException("Failed to identify calling assembly!");
            }

            var path = new Uri(assembly.CodeBase).LocalPath;
            return path;
        }

        public async Task Stop(string id)
        {
            console.WriteVerbose("Attempting to stop existing instance...");
            console.WriteVerbose($"  instance id: {id}");
            if (string.IsNullOrEmpty(id))
            {
                console.WriteError("Unable to stop instance of probe: no id provided!");
                throw new ArgumentNullException(nameof(id));
            }

            try
            {
                console.WriteVerbose("Attempting to connect to deployed instance...");
                await using var client = createClient(id);
                var response = await client.Send(Request.Shutdown);
                console.WriteVerbose($"Newly deployed instance responded with {response}.");

                if (response != Response.Bye)
                {
                    console.WriteError("Failed to stop deployed instance - instance returned unexpected response!");
                    return;
                }
            }
            catch (Exception e)
            {
                console.WriteError("Unable to connect to deployed instance - exception occurred!");
                console.WriteVerbose(e.ToString());
                throw;
            }

            console.WriteVerbose("Termination request successfully sent. Target instance will be terminated shortly. Have a nice day!");
        }
    }
}