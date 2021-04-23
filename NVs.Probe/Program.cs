using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NVs.Probe.Setup;
using Serilog;
using Serilog.Events;

[assembly: InternalsVisibleTo("NVs.Probe.Tests")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2, PublicKey=0024000004800000940000000602000000240000525341310004000001000100c547cac37abd99c8db225ef2f6c8a3602f3b3606cc9891605d02baa56104f4cfc0734aa39b93bf7852f7d9266654753cc297e7d2edfe0bac1cdcf9f717241550e0a7b191195b7667bb4f64bcb8e2121380fd1d9d46ad2d92d2d15605093924cceaf74c4861eff62abf69b9291ed0a340e113be11e6a7d3113e92484cf7045cc7")]

namespace NVs.Probe
{
    internal static class Program
    {
        private static int Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .CreateLogger();
            try
            {
                Log.Information("Starting host..");
                BuildHost(args).Run();
                return 0;
            }
            catch (Exception e)
            {
                Log.Fatal(e, "Host terminated unexpectedly!");
                return -1;
            }
        }

        private static IHost BuildHost(string[] args)
        {
            var (options, configs) = new ArgsParser().Parse(args);

            return new HostBuilder()
                .ConfigureServices(services =>
                {
                    services.AddSingleton<IHostedService>(s => new Payload(
                        configs, 
                        TimeSpan.FromMilliseconds(options.SeriesInterval), 
                        null, 
                        null, 
                        s.GetService<ILogger<Payload>>()));
                })
                .UseSerilog()
                .Build();
        }
    }
}
