using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using CommandLine;
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

            await ValidateId(id);

            if (!File.Exists(configPath))
            {
                console.WriteError("Unable to deploy new instance of probe: configuration file does not exists, or inaccessible!");
                throw new FileNotFoundException("Configuration file not found!", configPath);
            }

            var process = StartNewProcess(id, configPath, runStub);
            await Task.Delay(TimeSpan.FromSeconds(2));

            await EnsureInstanceOnline(id, process);

            console.WriteVerbose("Instance successfully started. Instance id is:");
            console.WriteLine(id);

            console.WriteVerbose("Have a nice day!");
        }

        private async Task EnsureInstanceOnline(string id, Process process)
        {
            var instanceOnline = true;
            try
            {
                console.WriteVerbose("Attempting to connect to deployed instance...");
                await using var client = createClient(id);
                var response = await client.Send(Request.Ping);
                console.WriteVerbose($"Newly deployed instance responded with {response}.");
                if (response != Response.Pong)
                {
                    if (response.HasValue)
                    {
                        console.WriteError("Failed to verify deployed instance - instance returned unexpected response!");
                        console.WriteVerbose($"Received response: {response.Value}");
                    }
                    else
                    {
                        console.WriteError("Unable to connect to deployed instance - timeout occurred!");
                    }

                    instanceOnline = false;
                }
            }
            catch (Exception e)
            {
                console.WriteError("Unable to connect to deployed instance - exception occurred!");
                console.WriteVerbose(e.ToString());
                throw;
            }

            if (!instanceOnline)
            {
                console.WriteVerbose($"Killing process {process.Id}...");
                process.Kill();
                console.WriteVerbose("Request to terminate process sent.");
                throw new InvalidOperationException("Unable to verify deployed instance!");
            }
        }

        private Process StartNewProcess(string id, string configPath, bool runStub)
        {
            console.WriteVerbose("Parameters verified, collecting details to start new process...");
            var path = GetExecutablePath();
            var args = runStub
                ? argsParser.FormatCommandLine(new StubArguments(id))
                : argsParser.FormatCommandLine(new ServeArguments(configPath, id));

            var info = path.EndsWith(".dll")
                ? new ProcessStartInfo("dotnet", $"\"{path}\" {args}")
                : new ProcessStartInfo(path, args);

            info.WorkingDirectory = Environment.CurrentDirectory;
            info.UseShellExecute = true;

            console.WriteVerbose("Start info collected:");
            console.WriteVerbose($"  command: {info.FileName}");
            console.WriteVerbose($"  arguments: {info.Arguments}");
            console.WriteVerbose($"  working dir: {info.WorkingDirectory}");

            console.WriteVerbose("Starting new instance...");
            var process = Process.Start(info);
            if (process == null)
            {
                console.WriteError("Unable to deploy new instance of probe: failed to start process!");
                throw new InvalidOperationException("Process instance was not created!");
            }

            console.WriteVerbose(
                $"Process {process.Id} stated. Waiting 2 seconds to give it some time to initialize server...");
            return process;
        }

        private async Task ValidateId(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                console.WriteError("Unable to deploy new instance of probe: no id provided!");
                throw new ArgumentNullException(nameof(id));
            }

            bool canProceed = true;
            try
            {
                console.WriteVerbose("Checking if instance with this id is already deployed...");
                await using var client = createClient(id);
                var response = await client.Send(Request.Ping);
                if (response == Response.Pong)
                {
                    console.WriteError("Unable to deploy new instance of probe: instance with same id already exists!");
                    canProceed = false;
                }

                if (response.HasValue)
                {
                    console.WriteError(
                        "Unable to deploy new instance of probe: instance with same id (or another process which uses named pipe with the same name) exists!");
                    console.WriteVerbose($"Response received: {response.Value}");
                    canProceed = false;
                }
            }
            catch (Exception e)
            {
                console.WriteWarning("Unexpected error occurred while checking id availability!");
                console.WriteVerbose(e.ToString());
            }

            if (!canProceed)
            {
                throw new ArgumentException("Named pipe with this name already exists!", nameof(id));
            }
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

            console.WriteVerbose("Termination request successfully sent. Target instance will be terminated shortly.  Have a nice day!");
        }
    }
}