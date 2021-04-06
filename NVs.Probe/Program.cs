using System;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NVs.Probe.Setup;
using Serilog;
using Serilog.Events;

[assembly: InternalsVisibleTo("NVs.Probe.Tests")]

namespace NVs.Probe
{
    static class Program
    {
        static int Main(string[] args)
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
            return new HostBuilder()
                .ConfigureServices(services =>
                {
                    services.AddSingleton<IHostedService>(s => new Payload(new MetricConfigBuilder(args).Build(), null, null, s.GetService<ILogger<Payload>>()));
                })
                .UseSerilog()
                .Build();
        }
    }
}
