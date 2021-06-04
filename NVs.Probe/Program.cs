using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using CommandLine;
using CommandLine.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MQTTnet;
using NVs.Probe.Client;
using NVs.Probe.Contract;
using NVs.Probe.Execution;
using NVs.Probe.Measuring;
using NVs.Probe.Mqtt;
using NVs.Probe.Server;
using NVs.Probe.Server.Shutdown;
using Serilog;

[assembly: InternalsVisibleTo("NVs.Probe.Tests")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2, PublicKey=0024000004800000940000000602000000240000525341310004000001000100c547cac37abd99c8db225ef2f6c8a3602f3b3606cc9891605d02baa56104f4cfc0734aa39b93bf7852f7d9266654753cc297e7d2edfe0bac1cdcf9f717241550e0a7b191195b7667bb4f64bcb8e2121380fd1d9d46ad2d92d2d15605093924cceaf74c4861eff62abf69b9291ed0a340e113be11e6a7d3113e92484cf7045cc7")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]

namespace NVs.Probe
{
    internal static class Program
    {
        private static async Task<int> Main(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddYamlFile("probe.settings.logging.yaml", true)
                .Build();

            Log.Logger = new LoggerConfiguration().ReadFrom.Configuration(configuration).CreateLogger();

            try
            {
                Log.Information("Starting host..");
                var parser = new Parser(with => with.EnableDashDash = true);
                var result = parser.ParseArguments<ServeArguments, DeployArguments, StopArguments, StubArguments>(args);
                var task = Task.CompletedTask;
                result
                    .WithParsed((ServeArguments a) =>
                    {
                        BuildProbeHost(a).Run();
                    })
                    .WithParsed((StubArguments a) =>
                    {
                        BuildStubHost(a).Run();
                    })
                    .WithParsed((DeployArguments a) =>
                    {
                        var console = new ConsoleWrapper(a.Verbose);
                        task = new Bootstrapper(parser, console, new PipeClientBuilder(console, TimeSpan.FromMilliseconds(a.Timeout)).Build).Start(a.InstanceId, a.ConfigurationPath, a.Stub);
                    })
                    .WithParsed((StopArguments a) =>
                    {
                        var console = new ConsoleWrapper(a.Verbose);
                        task = new Bootstrapper(parser, console, new PipeClientBuilder(console, TimeSpan.FromMilliseconds(a.Timeout)).Build).Stop(a.InstanceId);
                    })
                    .WithNotParsed(err =>
                    {
                        Log.Error("Failed to parse arguments!");
                        foreach (var error in err)
                        {
                            Log.Error("{@error}", error);
                        }

                        var helpText = HelpText.AutoBuild(result, h => HelpText.DefaultParsingErrorsHandler(result, h), e => e);
                        Console.WriteLine(helpText);
                    });

                await task;
                return result.Tag == ParserResultType.Parsed ? 0 : 1;
            }
            catch (Exception e)
            {
                Log.Fatal(e, "Host terminated unexpectedly!");
                return -1;
            }
        }

        private static IHost BuildStubHost(StubArguments args)
        {
            return new HostBuilder()
                .ConfigureServices(services =>
                {
                    services.AddHostedService(s => new Stub(s.GetService<ILogger<Stub>>()));

                    services.AddHostedService(s => new LifeTimeController(
                        () => new ShutdownRequestListener(args.InstanceId, s.GetService<ILogger<ShutdownRequestListener>>()),
                        s.GetService<IHostApplicationLifetime>(),
                        s.GetService<ILogger<LifeTimeController>>()));
                })
                .UseSerilog()
                .Build();
        }

        private static IHost BuildProbeHost(ServeArguments args)
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Logger(Log.Logger)
                .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} <s:{SourceContext}>{NewLine}{Exception}")
                .CreateLogger();

            var configuration = args.GetConfiguration();
            return new HostBuilder()
                .ConfigureServices(services =>
                {
                    services.AddHostedService(s => new Server.Probe(
                        configuration.ProbeOptions,
                        new Meter(
                            new ShellCommandRunner(configuration.RunnerOptions, 
                            s.GetService<ILogger<ShellCommandRunner>>()), 
                            s.GetService<ILogger<Meter>>()),
                        new MqttAdapter(configuration.MqttOptions.ClientOptions,
                            new MqttFactory(),
                            configuration.MqttOptions.RetryOptions,
                            new MqttAnnounceBuilder(typeof(Program).Assembly, 
                                s.GetService<ILogger<MqttAnnounceBuilder>>()),
                            s.GetService<ILogger<MqttAdapter>>()),
                        s.GetService<ILogger<Server.Probe>>()));

                    services.AddHostedService(s => new LifeTimeController(
                        () => new ShutdownRequestListener(args.InstanceId, s.GetService<ILogger<ShutdownRequestListener>>()),
                        s.GetService<IHostApplicationLifetime>(), 
                        s.GetService<ILogger<LifeTimeController>>()));
                })
                .UseSerilog()
                .Build();
        }
    }
}
